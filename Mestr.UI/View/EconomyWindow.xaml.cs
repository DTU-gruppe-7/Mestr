using Mestr.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mestr.UI.View
{
    /// <summary>
    /// Interaction logic for EconomyWindow.xaml
    /// </summary>
    public partial class EconomyWindow : Window
    {
        private readonly Regex regex = new Regex(@"^[0-9]*(\.[0-9]*)?$");

        public EconomyWindow()
        {
            InitializeComponent();
            DataContext = new EconomyViewModel();
        }
        private void AmountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow digits and optional decimal separator

            if (sender is not TextBox textBox)
            {
                e.Handled = true;
                return;
            }
            else
            {
                string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                e.Handled = !regex.IsMatch(newText);
            }
        }


        private void AmountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "0";

                // Move caret to end so user can continue typing
                textBox.CaretIndex = textBox.Text.Length;

            }
            else
            {
                e.Handled = true;
                return;
            }


        }


        private void AmountBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));
                if (!regex.IsMatch(pasteText))
                {
                    e.CancelCommand();
                }
            }
        }
    }
}
