using Mestr.Data.Interface;
using System.Runtime.InteropServices.Marshalling;

namespace Mestr.Data.Repository
{
    public class ProjectRepository : IRepository<Project>
    {
        private readonly List<Project> _projects = new List<Project>();

        public void Add(Project entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _projects.Add(entity);
        }

        public Project GetByID(int uuid)
        {
            return _projects.FirstOrDefault(p => p.Id == uuid);
        }

        public IEnumerable<Project> GetAll()
        {
            return _projects.ToList();
        }

        public void Update(Project entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var existingProject = _projects.FirstOrDefault(p => p.Id == entity.Id);
            if (existingProject != null)
            {
                var index = _projects.IndexOf(existingProject);
                _projects[index] = entity;
            }
        }

        public void Delete(int uuid)
        {
            var project = _projects.FirstOrDefault(p => p.Id == uuid);
            if (project != null)
            {
                _projects.Remove(project);
            }
        }
    }
}