using Mestr.Core.Enum;
using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mestr.Data.Repository
{
    public class ProjectRepository : IRepository<Project>
    {
        public async Task AddAsync(Project entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                var existingClient = await context.Clients.FindAsync(entity.Client.Uuid);
                if (existingClient != null)
                {
                    entity.Client = existingClient;
                }
                else
                {
                    context.Clients.Add(entity.Client);
                }

                context.Projects.Add(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Project?> GetByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return await context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .FirstOrDefaultAsync(p => p.Uuid == uuid);
            }
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            using (var context = new dbContext())
            {
                return await context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task UpdateAsync(Project entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                var existingProject = context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .FirstOrDefault(p => p.Uuid == entity.Uuid);

                if (existingProject != null)
                {
                    context.Entry(existingProject).CurrentValues.SetValues(entity);

                    if (existingProject.Client.Uuid != entity.Client.Uuid)
                    {
                        var newClient = context.Clients.Find(entity.Client.Uuid);
                        if (newClient != null)
                        {
                            existingProject.Client = newClient;
                        }
                        else
                        {
                            existingProject.Client = entity.Client;
                        }
                    }
                    UpdateCollection(existingProject.Expenses, entity.Expenses, context);
                    UpdateCollection(existingProject.Earnings, entity.Earnings, context);

                    await context.SaveChangesAsync();
                }
            }
        }

        private void UpdateCollection<T>(IList<T> existingCollection, IList<T> newCollection, dbContext context)
            where T : class
        {
            var itemsToRemove = existingCollection.Where(e => !newCollection.Contains(e)).ToList();
            foreach (var item in itemsToRemove)
            {
                existingCollection.Remove(item);
                context.Remove(item);
            }
            var itemsToAdd = newCollection.Where(e => !existingCollection.Contains(e)).ToList();
            foreach (var item in itemsToAdd)
            {
                existingCollection.Add(item);
            }
            foreach (var existingItem in existingCollection)
            {
                var newItem = newCollection.FirstOrDefault(i => i.Equals(existingItem));
                if (newItem != null)
                {
                    context.Entry(existingItem).CurrentValues.SetValues(newItem);
                }
            }
        }

        public async Task DeleteAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var project = context.Projects.FirstOrDefault(p => p.Uuid == uuid);
                if (project != null)
                {
                    context.Projects.Remove(project);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}