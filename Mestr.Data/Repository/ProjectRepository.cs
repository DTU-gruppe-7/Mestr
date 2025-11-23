using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.EntityFrameworkCore;

namespace Mestr.Data.Repository
{
    public class ProjectRepository : IRepository<Project>
    {
        public void Add(Project entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            dbContext.DatabaseLock.Wait();
            try
            {
                // Check if the client already exists in the database
                var existingClient = dbContext.Instance.Clients.Find(entity.Client.Uuid);
                if (existingClient != null)
                {
                    // Attach the existing client as unchanged
                    entity.Client = existingClient;
                }
                else
                {
                    // Mark the client as Added if it doesn't exist
                    dbContext.Instance.Clients.Add(entity.Client);
                }

                dbContext.Instance.Projects.Add(entity);
                dbContext.Instance.SaveChanges();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public Project? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            dbContext.DatabaseLock.Wait();
            try
            {
                return dbContext.Instance.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .FirstOrDefault(p => p.Uuid == uuid);
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public IEnumerable<Project> GetAll()
        {
            dbContext.DatabaseLock.Wait();
            try
            {
                return dbContext.Instance.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .AsNoTracking()
                    .ToList();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public void Update(Project entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            dbContext.DatabaseLock.Wait();
            try
            {
                // Reload the entity from the database with tracking
                var existingProject = dbContext.Instance.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .FirstOrDefault(p => p.Uuid == entity.Uuid);

                if (existingProject != null)
                {
                    // Update scalar properties
                    dbContext.Instance.Entry(existingProject).CurrentValues.SetValues(entity);
                    
                    // Update client if changed
                    if (existingProject.Client.Uuid != entity.Client.Uuid)
                    {
                        existingProject.Client = entity.Client;
                    }
                    
                    // Update expenses collection
                    UpdateCollection(existingProject.Expenses, entity.Expenses, dbContext.Instance);
                    
                    // Update earnings collection
                    UpdateCollection(existingProject.Earnings, entity.Earnings, dbContext.Instance);
                    
                    dbContext.Instance.SaveChanges();
                }
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        private void UpdateCollection<T>(IList<T> existingCollection, IList<T> newCollection, dbContext context) 
            where T : class
        {
            // Remove items that are no longer in the collection
            var itemsToRemove = existingCollection.Except(newCollection).ToList();
            foreach (var item in itemsToRemove)
            {
                existingCollection.Remove(item);
                context.Entry(item).State = EntityState.Deleted;
            }

            // Add new items
            var itemsToAdd = newCollection.Except(existingCollection).ToList();
            foreach (var item in itemsToAdd)
            {
                existingCollection.Add(item);
            }

            // Update existing items
            foreach (var item in newCollection.Intersect(existingCollection))
            {
                context.Entry(item).CurrentValues.SetValues(item);
            }
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            dbContext.DatabaseLock.Wait();
            try
            {
                var project = dbContext.Instance.Projects.FirstOrDefault(p => p.Uuid == uuid);
                if (project != null)
                {
                    dbContext.Instance.Projects.Remove(project);
                    dbContext.Instance.SaveChanges();
                }
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }
    }
}