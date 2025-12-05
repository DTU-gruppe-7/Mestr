using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Services.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Mestr.Services.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;

        public ProjectService(IRepository<Project> projectRepo)
        {
            _projectRepository = projectRepo ?? throw new ArgumentNullException(nameof(projectRepo));
        }

        public async Task<Project> CreateProjectAsync(string name, Client client, string description, DateTime? endDate)
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

            await _projectRepository.AddAsync(newProject).ConfigureAwait(false);
            return newProject;
        }

        public async Task<Project?> GetProjectByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty)
                throw new ArgumentException("UUID cannot be empty.", nameof(uuid));
            
            return await _projectRepository.GetByUuidAsync(uuid).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Project>> LoadAllProjectsAsync()
        {
            return await _projectRepository.GetAllAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<Project>> LoadOngoingProjectsAsync() 
        {
            var projects = await _projectRepository.GetAllAsync().ConfigureAwait(false);
            return projects.Where(p => p.Status == ProjectStatus.Aktiv || 
                            p.Status == ProjectStatus.Planlagt || 
                            p.Status == ProjectStatus.Aflyst);
        }

        public async Task<IEnumerable<Project>> LoadCompletedProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync().ConfigureAwait(false);
            return projects.Where(p => p.Status == ProjectStatus.Afsluttet);
        }

        public async Task UpdateProjectAsync(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            
            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
        }

        public async Task UpdateProjectStatusAsync(Guid projectId, ProjectStatus newStatus)
        {
            var project = await _projectRepository.GetByUuidAsync(projectId).ConfigureAwait(false);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectId));
            
            if (newStatus == ProjectStatus.Afsluttet && project.EndDate == null)
            {
                project.EndDate = DateTime.Now;
            }

            project.Status = newStatus;
            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
        }

        public async Task CompleteProjectAsync(Guid projectId)
        {
            var project = await _projectRepository.GetByUuidAsync(projectId).ConfigureAwait(false);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectId));
            
            project.Status = ProjectStatus.Afsluttet;
            project.EndDate = DateTime.Now;
            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
        }

        public async Task DeleteProjectAsync(Guid projectId)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
            
            var project = await _projectRepository.GetByUuidAsync(projectId).ConfigureAwait(false);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectId));
            
            await _projectRepository.DeleteAsync(projectId).ConfigureAwait(false);
        }

        public async Task AddExpenseToProjectAsync(Guid projectUuid, Expense expense)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));
            
            var project = await _projectRepository.GetByUuidAsync(projectUuid).ConfigureAwait(false);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            expense.ProjectUuid = projectUuid;
            project.Expenses.Add(expense);
            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
        }

        public async Task AddEarningToProjectAsync(Guid projectUuid, Earning earning)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (earning == null)
                throw new ArgumentNullException(nameof(earning));
            
            var project = await _projectRepository.GetByUuidAsync(projectUuid).ConfigureAwait(false);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            earning.ProjectUuid = projectUuid;
            project.Earnings.Add(earning);
            await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
        }

        public async Task RemoveExpenseFromProjectAsync(Guid projectUuid, Guid expenseUuid)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (expenseUuid == Guid.Empty)
                throw new ArgumentException("Expense UUID cannot be empty.", nameof(expenseUuid));
            
            var project = await _projectRepository.GetByUuidAsync(projectUuid).ConfigureAwait(false);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            var expense = project.Expenses.FirstOrDefault(e => e.Uuid == expenseUuid);
            if (expense != null)
            {
                project.Expenses.Remove(expense);
                await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
            }
        }

        public async Task RemoveEarningFromProjectAsync(Guid projectUuid, Guid earningUuid)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (earningUuid == Guid.Empty)
                throw new ArgumentException("Earning UUID cannot be empty.", nameof(earningUuid));
            
            var project = await _projectRepository.GetByUuidAsync(projectUuid).ConfigureAwait(false);
            if (project == null)
                throw new ArgumentException("Project not found.", nameof(projectUuid));
            
            var earning = project.Earnings.FirstOrDefault(e => e.Uuid == earningUuid);
            if (earning != null)
            {
                project.Earnings.Remove(earning);
                await _projectRepository.UpdateAsync(project).ConfigureAwait(false);
            }
        }
    }
}
