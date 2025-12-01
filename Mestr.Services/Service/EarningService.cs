using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Data.Interface;
using Mestr.Data.Interface;

namespace Mestr.Services.Service
{
    public class EarningService : IEarningService
    {
        private readonly IRepository<Earning> _earningRepository;
        
        public EarningService(IRepository<Earning> earningRepo)
        {
            _earningRepository = earningRepo ?? throw new ArgumentNullException(nameof(earningRepo));
        }
        
        public async Task<Earning> GetByUuidAsync(Guid uuid)
        {
            if (uuid == Guid.Empty) 
                throw new ArgumentException("UUID cannot be empty.", nameof(uuid));
            
            var earning = await _earningRepository.GetByUuidAsync(uuid).ConfigureAwait(false);
            if (earning == null)
                throw new ArgumentException("Earning not found.", nameof(uuid));
            
            return earning;
        }
        
        public async Task<Earning> AddNewEarningAsync(Guid projectUuid, string description, decimal amount, DateTime date)
        {
            if (projectUuid == Guid.Empty)
                throw new ArgumentException("Project UUID cannot be empty.", nameof(projectUuid));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty.", nameof(description));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            
            var earning = new Earning(Guid.NewGuid(), description, amount, date, false)
            {
                ProjectUuid = projectUuid
            };
            
            await _earningRepository.AddAsync(earning).ConfigureAwait(false);
            return earning;
        }
        
        public async Task<bool> DeleteAsync(Earning entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            if (entity.IsPaid)
                throw new InvalidOperationException("Betalte indtægter kan ikke slettes.");
            
            await _earningRepository.DeleteAsync(entity.Uuid).ConfigureAwait(false);
            return true;
        }
        
        public async Task<Earning> UpdateAsync(Earning entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            await _earningRepository.UpdateAsync(entity).ConfigureAwait(false);
            
            var updatedEarning = await _earningRepository.GetByUuidAsync(entity.Uuid).ConfigureAwait(false);
            return updatedEarning 
                ?? throw new InvalidOperationException("Failed to retrieve updated earning.");
        }
    }
}
