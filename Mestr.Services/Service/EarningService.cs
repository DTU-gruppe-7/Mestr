using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mestr.Services.Interface;
using Mestr.Core.Model;
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
            return _earningRepository.GetByUuid(uuid);
        }
        public List<Earning> GetAllByProjectUuid(Guid projektUuid)
        {
            return _earningRepository.GetAll().Where(e => e.ProjectUuid == projektUuid).ToList();
        }
        public Earning AddNewEarning(Guid projectUuid, string description, decimal amount, DateTime date)
        {
            var earning = new Earning(Guid.NewGuid(), projectUuid, description, amount, date, false);
            _earningRepository.Add(earning);
            return earning;
        }
        public bool Delete(Earning entity)
        {
            _earningRepository.Delete(entity.Uuid);
            return true;
        }  
        public Earning Update(Earning entity)
        {
            _earningRepository.Update(entity);
            return entity;
        }

    }

}
