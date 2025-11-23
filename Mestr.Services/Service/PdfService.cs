using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mestr.Core.Model;
using QuestPDF.Helpers;
using Mestr.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using Mestr.Services.Service;

public class PdfService : IPdfService
{
    private readonly IProjectService _projectService;
    private readonly ICompanyProfileService _companyProfileService;

    public PdfService()
    {
        _projectService = new ProjectService();
        _companyProfileService = new CompanyProfileService();
    }

    public byte[] GeneratePdfInvoice(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        // Get company profile
        var companyProfile = _companyProfileService.GetProfile();
        if (companyProfile == null)
            throw new InvalidOperationException("Firmaprofil ikke fundet. Opret venligst en firmaprofil før generering af faktura.");

        // Determine if client is business or private
        bool isBusinessClient = project.Client.Cvr != null;

        // Get only unpaid earnings that will be included in this invoice
        var unpaidEarnings = project.Earnings?.Where(e => !e.IsPaid).ToList() ?? new List<Earning>();

        // Earnings total (only unpaid)
        decimal earningsTotal = unpaidEarnings.Sum(e => e.Amount);

        // VAT calculation - only for business clients (B2B)
        decimal vat = isBusinessClient ? earningsTotal * 0.25m : 0m;
        decimal totalWithVat = earningsTotal + vat;

        // Mark all unpaid earnings as paid
        if (unpaidEarnings.Any())
        {
            foreach (var earning in unpaidEarnings)
            {
                earning.MarkAsPaid(DateTime.Now);
            }
            
            // Update project in database
            _projectService.UpdateProject(project);
        }

        // Create PDF
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header().Column(header =>
                {
                    header.Spacing(10);

                    // Invoice title and date
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Text("FAKTURA")
                            .FontSize(24)
                            .Bold();

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Fakturadato: {DateTime.Now:dd-MM-yyyy}")
                                .FontSize(10);
                            col.Item().Text($"Faktura nr: {project.Uuid.ToString().Substring(0, 8).ToUpper()}")
                                .FontSize(10);
                            
                            // Add client type indicator
                            col.Item().Text(isBusinessClient ? "Type: Erhvervskunde (B2B)" : "Type: Privatkunde (B2C)")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(20);

                    // Two-column layout: Sender (left) and Client (right)
                    col.Item().Row(row =>
                    {
                        // Sender info (from company profile)
                        row.RelativeItem().Column(sender =>
                        {
                            sender.Spacing(2);
                            sender.Item().Text("Fra:")
                                .FontSize(10)
                                .SemiBold();
                            sender.Item().Text(companyProfile.CompanyName)
                                .FontSize(11);
                            sender.Item().Text(companyProfile.Address)
                                .FontSize(10);
                            sender.Item().Text($"{companyProfile.ZipCode} {companyProfile.City}")
                                .FontSize(10);
                            
                            if (!string.IsNullOrEmpty(companyProfile.Cvr))
                            {
                                sender.Item().Text($"CVR: {companyProfile.Cvr}")
                                    .FontSize(10);
                            }
                            
                            if (!string.IsNullOrEmpty(companyProfile.Email))
                            {
                                sender.Item().Text($"E-mail: {companyProfile.Email}")
                                    .FontSize(10);
                            }
                            
                            if (!string.IsNullOrEmpty(companyProfile.PhoneNumber))
                            {
                                sender.Item().Text($"Tlf: {companyProfile.PhoneNumber}")
                                    .FontSize(10);
                            }
                        });

                        // Client info (adjusted based on client type)
                        row.RelativeItem().Column(client =>
                        {
                            client.Spacing(2);
                            client.Item().Text("Faktureres til:")
                                .FontSize(10)
                                .SemiBold();
                            
                            if (isBusinessClient)
                            {
                                // Business client format
                                client.Item().Text(project.Client.Name)
                                    .FontSize(11)
                                    .Bold();
                                client.Item().Text($"Att: {project.Client.ContactPerson}")
                                    .FontSize(10);
                            }
                            else
                            {
                                // Private client format - name is more prominent
                                client.Item().Text(project.Client.ContactPerson)
                                    .FontSize(11)
                                    .Bold();
                                if (!string.IsNullOrEmpty(project.Client.Name) && project.Client.Name != project.Client.ContactPerson)
                                {
                                    client.Item().Text($"({project.Client.Name})")
                                        .FontSize(9)
                                        .Italic();
                                }
                            }
                            
                            client.Item().Text(project.Client.Address)
                                .FontSize(10);
                            client.Item().Text($"{project.Client.PostalAddress} {project.Client.City}")
                                .FontSize(10);
                            
                            // Only show CVR for business clients
                            if (isBusinessClient && !string.IsNullOrEmpty(project.Client.Cvr))
                            {
                                client.Item().Text($"CVR: {project.Client.Cvr}")
                                    .FontSize(10);
                            }
                        });
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    // Project details
                    col.Item().Column(projectDetails =>
                    {
                        projectDetails.Spacing(2);
                        projectDetails.Item().Text($"Projekt: {project.Name}")
                            .FontSize(11)
                            .SemiBold();
                        projectDetails.Item().Text($"Betalingsbetingelser: Netto 14 dage")
                            .FontSize(10);
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    // Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("Nr.").Bold();
                            header.Cell().Element(HeaderCellStyle).Text("Beskrivelse").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Antal").Bold();
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Beløb (DKK)").Bold();
                        });

                        // Earnings rows (only unpaid ones)
                        if (unpaidEarnings.Any())
                        {
                            int index = 1;
                            foreach (var e in unpaidEarnings)
                            {
                                table.Cell().Element(CellStyle).Text(index.ToString());
                                table.Cell().Element(CellStyle).Text(e.Description);
                                table.Cell().Element(CellStyle).AlignRight().Text("1");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{e.Amount:N2}");
                                index++;
                            }
                        }

                        static IContainer HeaderCellStyle(IContainer container)
                        {
                            return container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Darken2)
                                .PaddingVertical(8)
                                .PaddingHorizontal(5);
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(8)
                                .PaddingHorizontal(5);
                        }
                    });

                    // Totals section (right-aligned) - adjusted based on client type
                    col.Item().AlignRight().Width(250).Column(totals =>
                    {
                        totals.Spacing(3);
                        
                        if (isBusinessClient)
                        {
                            // Business client - show VAT breakdown
                            totals.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal (ekskl. moms):");
                                row.RelativeItem().AlignRight().Text($"{earningsTotal:N2} DKK");
                            });

                            totals.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Moms (25%):");
                                row.RelativeItem().AlignRight().Text($"{vat:N2} DKK");
                            });

                            totals.Item().PaddingTop(5).BorderTop(2).BorderColor(Colors.Grey.Darken2).PaddingVertical(8).Row(row =>
                            {
                                row.RelativeItem().Text("I alt at betale (inkl. moms):")
                                    .FontSize(14)
                                    .Bold();
                                row.RelativeItem().AlignRight().Text($"{totalWithVat:N2} DKK")
                                    .FontSize(14)
                                    .Bold();
                            });
                        }
                        else
                        {
                            // Private client - simpler total (VAT included in price)
                            totals.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Text("Total:");
                                row.RelativeItem().AlignRight().Text($"{earningsTotal:N2} DKK");
                            });

                            totals.Item().PaddingTop(5).BorderTop(2).BorderColor(Colors.Grey.Darken2).PaddingVertical(8).Row(row =>
                            {
                                row.RelativeItem().Text("I alt at betale:")
                                    .FontSize(14)
                                    .Bold();
                                row.RelativeItem().AlignRight().Text($"{totalWithVat:N2} DKK")
                                    .FontSize(14)
                                    .Bold();
                            });
                            
                            // Add note for private clients
                            totals.Item().PaddingTop(5).Text("Alle priser er inkl. moms")
                                .FontSize(9)
                                .Italic()
                                .FontColor(Colors.Grey.Darken1);
                        }
                    });

                    col.Item().PaddingTop(20).LineHorizontal(1);

                    // Payment info (from company profile)
                    col.Item().PaddingTop(10).Column(payment =>
                    {
                        payment.Spacing(2);
                        payment.Item().Text("Betalingsinformation:")
                            .FontSize(11)
                            .SemiBold();
                        
                        if (!string.IsNullOrEmpty(companyProfile.BankRegNumber))
                        {
                            payment.Item().Text($"Reg.nr: {companyProfile.BankRegNumber}")
                                .FontSize(10);
                        }
                        
                        if (!string.IsNullOrEmpty(companyProfile.BankAccountNumber))
                        {
                            payment.Item().Text($"Konto: {companyProfile.BankAccountNumber}")
                                .FontSize(10);
                        }
                    });
                });

                page.Footer()
                    .AlignCenter()
                    .Text("Tak for samarbejdet!")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        })
        .GeneratePdf();
    }
}

