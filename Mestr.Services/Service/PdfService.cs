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

    public PdfService()
    {
        _projectService = new ProjectService();
    }

    public byte[] GeneratePdfInvoice(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        // Get only unpaid earnings that will be included in this invoice
        var unpaidEarnings = project.Earnings?.Where(e => !e.IsPaid).ToList() ?? new List<Earning>();

        // Earnings total (only unpaid)
        decimal earningsTotal = unpaidEarnings.Sum(e => e.Amount);

        // VAT + total with VAT
        decimal vat = earningsTotal * 0.25m;
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
                        });
                    });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(20);

                    // Two-column layout: Sender (left) and Client (right)
                    col.Item().Row(row =>
                    {
                        // Sender info (your company - placeholder)
                        row.RelativeItem().Column(sender =>
                        {
                            sender.Spacing(2);
                            sender.Item().Text("Fra:")
                                .FontSize(10)
                                .SemiBold();
                            sender.Item().Text("Dit Firma ApS")
                                .FontSize(11);
                            sender.Item().Text("Din Vej 123")
                                .FontSize(10);
                            sender.Item().Text("1234 By")
                                .FontSize(10);
                            sender.Item().Text("CVR: 12345678")
                                .FontSize(10);
                        });

                        // Client info
                        row.RelativeItem().Column(client =>
                        {
                            client.Spacing(2);
                            client.Item().Text("Faktureres til:")
                                .FontSize(10)
                                .SemiBold();
                            client.Item().Text(project.Client.Name)
                                .FontSize(11);
                            client.Item().Text($"Att: {project.Client.ContactPerson}")
                                .FontSize(10);
                            client.Item().Text(project.Client.Address)
                                .FontSize(10);
                            client.Item().Text($"{project.Client.PostalAddress} {project.Client.City}")
                                .FontSize(10);
                            
                            if (!string.IsNullOrEmpty(project.Client.Cvr))
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

                    // Totals section (right-aligned)
                    col.Item().AlignRight().Width(250).Column(totals =>
                    {
                        totals.Spacing(3);
                        totals.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal:");
                            row.RelativeItem().AlignRight().Text($"{earningsTotal:N2} DKK");
                        });

                        totals.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Moms (25%):");
                            row.RelativeItem().AlignRight().Text($"{vat:N2} DKK");
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
                    });

                    col.Item().PaddingTop(20).LineHorizontal(1);

                    // Payment info
                    col.Item().PaddingTop(10).Column(payment =>
                    {
                        payment.Spacing(2);
                        payment.Item().Text("Betalingsinformation:")
                            .FontSize(11)
                            .SemiBold();
                        payment.Item().Text("Reg.nr: 0000")
                            .FontSize(10);
                        payment.Item().Text("Konto: 00000000")
                            .FontSize(10);
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

