using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mestr.Data.Repository
{
    public class ClientRepository : IRepository<Client>
    {
        public void Add(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                context.Clients.Add(entity);
                context.SaveChanges();
            }
        }

        public Client? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return context.Clients
                    .Include(c => c.Projects)
                    .FirstOrDefault(c => c.Uuid == uuid);
            }
        }

        public IEnumerable<Client> GetAll()
        {
            using (var context = new dbContext())
            {
                return context.Clients
                    .Include(c => c.Projects)
                    .AsNoTracking()
                    .ToList();
            }
        }

        public void Update(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                // Attach og set state til Modified er ofte den enkleste måde at opdatere disconnected entities
                context.Clients.Update(entity);
                context.SaveChanges();
            }
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var client = context.Clients.FirstOrDefault(c => c.Uuid == uuid);
                if (client != null)
                {
                    context.Clients.Remove(client);
                    context.SaveChanges();
                }
            }
        }
    }
}