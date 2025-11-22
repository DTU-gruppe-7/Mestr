using Mestr.UI.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Configuration;
using System.Data;
using System.Globalization;
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
