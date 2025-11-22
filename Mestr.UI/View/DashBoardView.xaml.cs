using Mestr.Core.Model;
using Mestr.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mestr.UI.View
{
    /// <summary>
    /// Interaction logic for DashBoardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }
        private void ProjectRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 1. Tjek at vi har fat i en række
            if (sender is DataGridRow row)
            {
                // 2. Hent data fra rækken (det projekt man trykkede på)
                if (row.DataContext is Project clickedProject)
                {
                    // 3. Hent ViewModel og kør kommandoen
                    if (this.DataContext is DashboardViewModel vm)
                    {
                        if (vm.ViewProjectDetailsCommand.CanExecute(clickedProject.Uuid))
                        {
                            vm.ViewProjectDetailsCommand.Execute(clickedProject.Uuid);
                        }
                    }
                }
            }
        }
    }
}
