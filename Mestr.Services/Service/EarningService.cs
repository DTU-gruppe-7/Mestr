using System;
using System.Collections.Generic;
using System.Linq;
using Mestr.Core.Model;
using Mestr.Services.Interface;
using Mestr.Data.Interface;
using Mestr.Data.Repository;

namespace Mestr.Services.Service
{
    public class EarningService : IEarningService
    {
        private readonly IRepository<Earning> _earningRepository;
        
        public EarningService()
        {
            _earningRepository = new EarningRepository();
        }
        
        public Earning GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty) 
                throw new ArgumentException("UUID cannot be empty.", nameof(uuid));
            
            var earning = _earningRepository.GetByUuid(uuid);
            if (earning == null)
                throw new ArgumentException("Earning not found.", nameof(uuid));
            
            return earning;
        }
        
        public Earning AddNewEarning(Guid projectUuid, string description, decimal amount, DateTime date)
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
            
            _earningRepository.Add(earning);
            return earning;
        }
        
        public bool Delete(Earning entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            _earningRepository.Delete(entity.Uuid);
            return true;
        }
        
        public Earning Update(Earning entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
            
            var existing = _earningRepository.GetByUuid(entity.Uuid);
            if (existing == null)
                throw new ArgumentException("Earning not found.", nameof(entity));
            
            // Update properties
            existing.Description = entity.Description;
            existing.Amount = entity.Amount;
            existing.Date = entity.Date;
            existing.IsPaid = entity.IsPaid;
            
            _earningRepository.Update(existing);
            return existing;
        }
    }
}
