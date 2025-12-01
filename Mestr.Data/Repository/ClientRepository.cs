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
        public async Task AddAsync(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                context.Clients.Add(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Client?> GetByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                return await context.Clients
                    .Include(c => c.Projects)
                    .FirstOrDefaultAsync(c => c.Uuid == uuid);
            }
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            using (var context = new dbContext())
            {
                return await context.Clients
                    .Include(c => c.Projects)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task UpdateAsync(Client entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = new dbContext())
            {
                // Attach og set state til Modified er ofte den enkleste måde at opdatere disconnected entities
                context.Clients.Update(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) throw new ArgumentNullException(nameof(uuid));

            using (var context = new dbContext())
            {
                var client = context.Clients.FirstOrDefault(c => c.Uuid == uuid);
                if (client != null)
                {
                    context.Clients.Remove(client);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}