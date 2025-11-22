using System.Configuration;
using System.Data;
using System.Windows;
using Mestr.UI.ViewModels;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace Mestr.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(),
            };

            MainWindow.Show();

            base.OnStartup(e);
            
        }
    }

}
