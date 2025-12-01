using Mestr.Data.DbContext; // VIGTIGT: Husk denne using for at kunne se dbContext
using Mestr.UI.Utilities;
using Mestr.UI.View;
using Mestr.UI.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System; // Til Exception
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Mestr.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Sæt kultur FØRST, før noget UI oprettes
            var cultureInfo = new CultureInfo("da-DK");

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // VIGTIGT for WPF bindings:
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));

            // --- DATABASE INITIALISERING START ---
            // Her opretter vi databasen én gang ved opstart.
            // Hvis databasen allerede findes, gør EnsureCreated ingenting.
            try
            {
                using (var context = new dbContext())
                {
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(
                    $"Databasen kunne ikke initialiseres.\n\nFejl: {ex.Message}",
                    "Kritisk fejl");

                // Luk applikationen da den ikke kan virke uden database
                Shutdown();
                return;
            }
            // --- DATABASE INITIALISERING SLUT ---

            base.OnStartup(e);

            QuestPDF.Settings.License = LicenseType.Community;
            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(),
            };

            MainWindow.Show();
        }
    }
}