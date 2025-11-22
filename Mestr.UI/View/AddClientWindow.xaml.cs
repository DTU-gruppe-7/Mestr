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
    /// Interaction logic for AddClientWindow.xaml
    /// </summary>
    public partial class AddClientWindow : Window
    {
        public Client? CreatedClient { get; private set; }

        public AddClientWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Get the created client from the ViewModel if it exists
            if (DataContext is AddClientViewModel viewModel)
            {
                CreatedClient = viewModel.CreatedClient;
            }
        }
    }
}
