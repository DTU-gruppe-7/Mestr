using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Services.Interface;
using SQLitePCL;

namespace Mestr.Services.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;

        public ProjectService(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;

        }
        public Project CreateProject(string name, string description, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Project name cannot be null or empty.", nameof(name));
            }

            var newProject = new Project(
                uuid: Guid.NewGuid(),
                name: name,
                createdDate: DateTime.Now,
                startDate: DateTime.Now,
                description: description,
                status: ProjectStatus.Planned,
                endDate: endDate
            );

            _projectRepository.Add(newProject);

            return newProject;
        }

        public IEnumerable<Project> LoadAllProjects()
        {
            return _projectRepository.GetAll();
        } 
    }
}
