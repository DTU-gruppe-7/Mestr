using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mestr.Core.Interface;
using Mestr.Core.Model;
using QuestPDF.Helpers;
using Mestr.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;

public class PdfService : IPdfService
{
    public byte[] GeneratePdfInvoice(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        // Earnings total
        decimal earningsTotal = project.Earnings.Sum(e => e.Amount);

        // VAT + total with VAT
        decimal vat = earningsTotal * 0.25m;
        decimal totalWithVat = earningsTotal + vat;

        // Create PDF
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header().Text($"FAKTURA — {project.Name}")
                    .FontSize(22)
                    .Bold();

                page.Content().Column(col =>
                {
                    col.Spacing(20);

                    // Projektinfo
                    col.Item().Text($"Kunde: {"project.CustomerName"}")
                        .FontSize(12);

                    col.Item().Text($"Dato: {DateTime.Now:dd-MM-yyyy}")
                        .FontSize(12);

                    col.Item().LineHorizontal(1);

                    // Earnings overview
                    col.Item().Text("Ydelser:")
                        .FontSize(16)
                        .Bold();

                    // Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Beskrivelse").Bold();
                            header.Cell().Element(CellStyle).Text("Pris (DKK)").Bold();
                        });

                        // Earnings rows
                        foreach (var e in project.Earnings)
                        {
                            table.Cell().Element(CellStyle).Text(e.Description);
                            table.Cell().Element(CellStyle).Text($"{e.Amount:N2}");
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.PaddingVertical(5);
                        }
                    });

                    col.Item().LineHorizontal(1);

                    // VAT + totals
                    col.Item().Column(c =>
                    {
                        c.Spacing(5);

                        c.Item().Text($"Subtotal (uden moms): {earningsTotal:N2} DKK");

                        c.Item().Text($"Moms (25%): {vat:N2} DKK");

                        c.Item().Text($"Total (inkl. moms): {totalWithVat:N2} DKK")
                            .FontSize(14)
                            .Bold();

                        c.Item().Text($"Moms udgør 25% ({vat:N2} kr.) af subtotallen.");
                    });

                    col.Item().LineHorizontal(1);

                    // Payment info
                    col.Item().Text("Betalingsinformation:")
                        .Bold();
                    col.Item().Text("Reg.nr: 0000");
                    col.Item().Text("Konto: 00000000");
                });

                page.Footer()
                    .AlignCenter()
                    .Text("Tak for samarbejdet!")
                    .FontSize(10);
            });
        })
        .GeneratePdf();
    }
}

