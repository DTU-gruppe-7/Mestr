using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }

            set
            {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }


        private string _clientName;
        public string ClientName
        {
            get { return _clientName; }
            set
            {
                _clientName = value;
                OnPropertyChanged(nameof(ClientName));
            }
        }


        private DateTime _deadline;
        public DateTime Deadline
        {
            get { return _deadline; }
            set
            {
                _deadline = value;
                OnPropertyChanged(nameof(Deadline));
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }

        }

        public ICommand CreateProjectCommand { get; }
        public ICommand CancelCommand { get; }

        public ProjectViewModel()
        {
            CreateProjectCommand = new Command.ProjectCommand();
            CancelCommand = new Command.ProjectCommand();
        }

    }

}
