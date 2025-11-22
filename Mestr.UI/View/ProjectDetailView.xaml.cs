using Mestr.Core.Model;
using Mestr.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mestr.UI.View
{
    public partial class ProjectDetailView : UserControl
    {
        public ProjectDetailView()
        {
            InitializeComponent();
        }
        private void Row_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 1. Find rækken der blev trykket på
            if (sender is DataGridRow row)
            {
                // 2. Find vores ViewModel (DataContext for hele siden)
                if (this.DataContext is ProjectDetailViewModel vm)
                {
                    // 3. Tjek hvad rækken indeholder og kør den rette kommando
                    if (row.DataContext is Earning earning)
                    {
                        vm.EditEarningCommand.Execute(earning);
                    }
                    else if (row.DataContext is Expense expense)
                    {
                        vm.EditExpenseCommand.Execute(expense);
                    }
                }
            }
        }
    }

}   