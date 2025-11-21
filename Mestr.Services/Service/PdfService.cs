using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mestr.Core.Interface;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;

public class PdfService : IPdfService
{
    public byte[] GenerateInvoice(Project project)
    {
        // Selve PDF genereringen
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(20);

                page.Header().Text($"Faktura - {project.Name}")
                    .FontSize(20)
                    .SemiBold();

                page.Content().Column(col =>
                {
                    // Basic info
                    col.Item().Text($"Projekt ID: {project.Uuid}");
                    col.Item().Text($"Dato: {DateTime.Now:dd/MM/yyyy}");

                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    // Earnings
                    col.Item().Text("Indtjeninger").FontSize(16).SemiBold();

                    if (project.Earnings != null && project.Earnings.Any())
                    {
                        foreach (var earning in project.Earnings)
                            col.Item().Text($"{earning.Description} - {earning.Amount} kr.");
                    }
                    else
                    {
                        col.Item().Text("Ingen indtjeninger registreret");
                    }

                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    // Expenses
                    col.Item().Text("Udgifter").FontSize(16).SemiBold();

                    if (project.Expenses != null && project.Expenses.Any())
                    {
                        foreach (var expense in project.Expenses)
                            col.Item().Text($"{expense.Description} - {expense.Amount} kr.");
                    }
                    else
                    {
                        col.Item().Text("Ingen udgifter registreret");
                    }

                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    // Totals
                    decimal totalEarn = project.Earnings?.Sum(x => x.Amount) ?? 0;
                    decimal totalExp = project.Expenses?.Sum(x => x.Amount) ?? 0;
                    decimal total = totalEarn - totalExp;

                    col.Item().Text($"Total indtjening: {totalEarn} kr.");
                    col.Item().Text($"Total udgifter: {totalExp} kr.");
                    col.Item().Text($"Resultat: {total} kr.")
                        .FontSize(16)
                        .SemiBold();
                });
            });
        }).GeneratePdf(); // <-- PDF’en returneres som byte-array
    }
}

