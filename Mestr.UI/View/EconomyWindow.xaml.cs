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
using System.Windows.Shapes;
using Mestr.UI.ViewModels;

namespace Mestr.UI.View
{
    /// <summary>
    /// Interaction logic for EconomyWindow.xaml
    /// </summary>
    public partial class EconomyWindow : Window
    {
        public EconomyWindow()
        {
            InitializeComponent();
          DataContext = new EconomyViewModel();
        }
    }
}
