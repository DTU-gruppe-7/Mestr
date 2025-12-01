using Mestr.Core.Model;
using Mestr.Core.Constants;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Mestr.Data.Repository;
using Mestr.Services.Interface;
using Mestr.Services.Service;
using Mestr.UI.Utilities;
using Mestr.UI.View;
using Mestr.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
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
        public IServiceProvider Services { get; }
        
        public App()
        {
            Services = ConfigureServices();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Sæt kultur FØRST, før noget UI oprettes
            var cultureInfo = new CultureInfo(AppConstants.Culture.Danish);

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
                    string.Format(AppConstants.ErrorMessages.DatabaseInitError, ex.Message),
                    AppConstants.WindowTitles.CriticalError);

                // Luk applikationen da den ikke kan virke uden database
                Shutdown();
                return;
            }
            // --- DATABASE INITIALISERING SLUT ---

            base.OnStartup(e);

            QuestPDF.Settings.License = LicenseType.Community;

            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            MainWindow = new MainWindow();
            MainWindow.DataContext = mainViewModel;

            MainWindow.Show();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<IRepository<Project>, ProjectRepository>();
            services.AddTransient<IRepository<Client>, ClientRepository>();
            services.AddTransient<IRepository<Expense>, ExpenseRepository>();
            services.AddTransient<IRepository<Earning>, EarningRepository>();
            services.AddTransient<ICompanyProfileRepository, CompanyProfileRepository>();

            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IExpenseService, ExpenseService>();
            services.AddTransient<IEarningService, EarningService>();
            services.AddTransient<ICompanyProfileService, CompanyProfileService>();

            services.AddSingleton<MainViewModel>(); // Singleton fordi vi kun vil have én MainViewModel

            return services.BuildServiceProvider();
        }
    }
}