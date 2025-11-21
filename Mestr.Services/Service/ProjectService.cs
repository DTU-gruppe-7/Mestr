using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Data.Repository;
using Mestr.Services.Interface;
using SQLitePCL;

namespace Mestr.Services.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;

        public ProjectService()
        {
            _projectRepository = new ProjectRepository();

        }
        public Project CreateProject(string name, string description, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Project name cannot be null or empty.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(description)) {
                description = "";
            }

            var newProject = new Project(
                uuid: Guid.NewGuid(),
                name: name,
                createdDate: DateTime.Now,
                startDate: DateTime.Now,
                description: description,
                status: ProjectStatus.Planlagt,
                endDate: endDate
            );

            _projectRepository.Add(newProject);

            return newProject;
        }

        public Project? GetProjectByUuid(Guid uuid)
        {
            return _projectRepository.GetByUuid(uuid);
        }

        public IEnumerable<Project> LoadAllProjects()
        {
            return _projectRepository.GetAll();
        }

        public IEnumerable<Project> LoadOngoingProjects() 
        {
            return _projectRepository.GetAll()
                .Where(p => p.Status == ProjectStatus.Aktiv || p.Status == ProjectStatus.Planlagt);
        }

        public IEnumerable<Project> LoadCompletedProjects()
        {
            return _projectRepository.GetAll()
                .Where(p => p.Status == ProjectStatus.Afsluttet);
        }

        public void UpdateProject(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project), "Project must not be null.");
            }
            _projectRepository.Update(project);
        }

        // --- NY METODE TIL GENÅBNING (TOGGLE STATUS) ---
        public void UpdateProjectStatus(Guid projectId, ProjectStatus newStatus)
        {
            var project = _projectRepository.GetByUuid(projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.", nameof(projectId));
            }
            
            if (newStatus == ProjectStatus.Afsluttet)
            {
                if (project.EndDate == null)
                {
                    project.EndDate = DateTime.Now;
                }
            }

            project.Status = newStatus;
            _projectRepository.Update(project);
        }

        public void CompleteProject(Guid projectId)
        {
            var project = _projectRepository.GetByUuid(projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.", nameof(projectId));
            }
            project.Status = ProjectStatus.Afsluttet;
            project.EndDate= DateTime.Now;
            _projectRepository.Update(project);
        }
        public void DeleteProject(Guid projectId)
        {
            var project = _projectRepository.GetByUuid(projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.", nameof(projectId));
            }
            _projectRepository.Delete(projectId);
        }
    }
}
