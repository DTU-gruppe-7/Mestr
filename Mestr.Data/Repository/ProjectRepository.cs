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
        public void Add(Project entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                // Tjek om klienten allerede findes i databasen
                var existingClient = context.Clients.Find(entity.Client.Uuid);
                if (existingClient != null)
                {
                    // Brug den eksisterende klient instans, som nu er tracked af context
                    entity.Client = existingClient;
                }
                else
                {
                    // Marker klienten som Added hvis den ikke findes
                    context.Clients.Add(entity.Client);
                }

                context.Projects.Add(entity);
                context.SaveChanges();
            }
        }

        public Project? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                // Vi bruger AsNoTracking her, hvis objektet kun skal bruges til visning.
                // Hvis det skal redigeres og gemmes igen via Update, er det fint at returnere det detached.
                return context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .FirstOrDefault(p => p.Uuid == uuid);
            }
        }

        public IEnumerable<Project> GetAll()
        {
            using (var context = new dbContext())
            {
                return context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .AsNoTracking()
                    .ToList();
            }
        }

        public void Update(Project entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                // Hent den eksisterende entitet fra databasen med tracking
                var existingProject = context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Expenses)
                    .Include(p => p.Earnings)
                    .FirstOrDefault(p => p.Uuid == entity.Uuid);

                if (existingProject != null)
                {
                    // Opdater simple properties
                    context.Entry(existingProject).CurrentValues.SetValues(entity);

                    // Opdater klient hvis ændret
                    if (existingProject.Client.Uuid != entity.Client.Uuid)
                    {
                        // Forsøg at finde den nye klient i db
                        var newClient = context.Clients.Find(entity.Client.Uuid);
                        if (newClient != null)
                        {
                            existingProject.Client = newClient;
                        }
                        else
                        {
                            // Hvis klienten er helt ny (burde sjældent ske ved update af projekt, men muligt)
                            existingProject.Client = entity.Client;
                        }
                    }

                    // Opdater expenses kollektion
                    UpdateCollection(existingProject.Expenses, entity.Expenses, context);

                    // Opdater earnings kollektion
                    UpdateCollection(existingProject.Earnings, entity.Earnings, context);

                    context.SaveChanges();
                }
            }
        }

        private void UpdateCollection<T>(IList<T> existingCollection, IList<T> newCollection, dbContext context)
            where T : class
        {
            // Find items der skal fjernes (findes i existing, men ikke i new)
            // Vi sammenligner typisk på ID. Da vi ikke har ID her i generisk metode,
            // antager vi at objekternes Equals metode eller reference virker, 
            // men EF Core 'SetValues' tilgang er ofte sikrere.
            // Her bevarer vi din originale logik, men tilpasset context scope.

            var itemsToRemove = existingCollection.Where(e => !newCollection.Contains(e)).ToList();
            foreach (var item in itemsToRemove)
            {
                existingCollection.Remove(item);
                // Slet explicit fra context
                context.Remove(item);
            }

            // Find items der skal tilføjes (findes i new, men ikke i existing)
            var itemsToAdd = newCollection.Where(e => !existingCollection.Contains(e)).ToList();
            foreach (var item in itemsToAdd)
            {
                existingCollection.Add(item);
                // Attach eller Add håndteres automatisk når de lægges i existingCollection, 
                // da existingProject er tracked.
            }

            // Opdater eksisterende items
            foreach (var existingItem in existingCollection)
            {
                var newItem = newCollection.FirstOrDefault(i => i.Equals(existingItem));
                if (newItem != null)
                {
                    context.Entry(existingItem).CurrentValues.SetValues(newItem);
                }
            }
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var project = context.Projects.FirstOrDefault(p => p.Uuid == uuid);
                if (project != null)
                {
                    context.Projects.Remove(project);
                    context.SaveChanges();
                }
            }
        }
    }
}