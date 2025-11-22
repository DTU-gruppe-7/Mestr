using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Core.Interface;
using Mestr.Data.Interface;
using Mestr.Services.Interface;
using Mestr.Data.Repository;

namespace Mestr.Services.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;

        public ProjectService()
        {
            _projectRepository = new ProjectRepository();
            _clientRepository = new ClientRepository();
        }

        public Project CreateProject(string name, Client client, string description, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name cannot be null or empty.", nameof(name));
            if (client == null)
                throw new ArgumentException("There must be a client when adding a new project", nameof(client));
            
            description ??= string.Empty;

            var newProject = new Project(
                Guid.NewGuid(),
                name,
                client,
                DateTime.Now,
                DateTime.Now,
                description,
                ProjectStatus.Planlagt,
                endDate
            );

            _projectRepository.Add(newProject);
            return newProject;
        }

        public Project? GetProjectByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty)
                throw new ArgumentException("UUID cannot be empty.", nameof(uuid));
            
            return _projectRepository.GetByUuid(uuid);
        }

        public IEnumerable<Project> LoadAllProjects()
        {
            return _projectRepository.GetAll();
        }

        public IEnumerable<Project> LoadOngoingProjects() 
        {
            return _projectRepository.GetAll()
                .Where(p => p.Status == ProjectStatus.Aktiv || 
                            p.Status == ProjectStatus.Planlagt || 
                            p.Status == ProjectStatus.Aflyst);
        }

        public IEnumerable<Project> LoadCompletedProjects()
        {
            return _projectRepository.GetAll()
                .Where(p => p.Status == ProjectStatus.Afsluttet);
        }

        public void UpdateProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            
            _projectRepository.Update(project);
        }

        public void UpdateProjectStatus(Guid projectId, ProjectStatus newStatus)
        {
            var project = _projectRepository.GetByUuid(projectId);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectId));
            
            if (newStatus == ProjectStatus.Afsluttet && project.EndDate == null)
            {
                project.EndDate = DateTime.Now;
            }

            project.Status = newStatus;
            _projectRepository.Update(project);
        }

        public void CompleteProject(Guid projectId)
        {
            var project = _projectRepository.GetByUuid(projectId);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectId));
            
            project.Status = ProjectStatus.Afsluttet;
            project.EndDate = DateTime.Now;
            _projectRepository.Update(project);
        }

        public void DeleteProject(Guid projectId)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
            
            var project = _projectRepository.GetByUuid(projectId);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectId));
            
            _projectRepository.Delete(projectId);
        }

        public void AddExpenseToProject(Guid projectUuid, Expense expense)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));
            
            var project = _projectRepository.GetByUuid(projectUuid);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            expense.ProjectUuid = projectUuid;
            project.Expenses.Add(expense);
            _projectRepository.Update(project);
        }

        public void AddEarningToProject(Guid projectUuid, Earning earning)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (earning == null)
                throw new ArgumentNullException(nameof(earning));
            
            var project = _projectRepository.GetByUuid(projectUuid);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            earning.ProjectUuid = projectUuid;
            project.Earnings.Add(earning);
            _projectRepository.Update(project);
        }

        public void RemoveExpenseFromProject(Guid projectUuid, Guid expenseUuid)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (expenseUuid == Guid.Empty)
                throw new ArgumentException("Expense UUID cannot be empty.", nameof(expenseUuid));
            
            var project = _projectRepository.GetByUuid(projectUuid);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            var expense = project.Expenses.FirstOrDefault(e => e.Uuid == expenseUuid);
            if (expense != null)
            {
                project.Expenses.Remove(expense);
                _projectRepository.Update(project);
            }
        }

        public void RemoveEarningFromProject(Guid projectUuid, Guid earningUuid)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (earningUuid == Guid.Empty)
                throw new ArgumentException("Earning UUID cannot be empty.", nameof(earningUuid));
            
            var project = _projectRepository.GetByUuid(projectUuid);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            var earning = project.Earnings.FirstOrDefault(e => e.Uuid == earningUuid);
            if (earning != null)
            {
                project.Earnings.Remove(earning);
                _projectRepository.Update(project);
            }
        }
    }
}
