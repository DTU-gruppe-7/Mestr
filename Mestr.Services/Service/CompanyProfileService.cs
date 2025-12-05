// Implementer ICompanyProfileService først
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Data.Repository;
using Mestr.Services.Interface;

namespace Mestr.Services.Service
{
    public class CompanyProfileService : ICompanyProfileService
    {
        private readonly ICompanyProfileRepository _repository;

        public CompanyProfileService(ICompanyProfileRepository companyProfileServiceRepo)
        {
            _repository = companyProfileServiceRepo ?? throw new ArgumentNullException(nameof(companyProfileServiceRepo)); ;
        }

        public CompanyProfile? GetProfile()
        {
            return _repository.Get();
        }

        public void UpdateProfile(CompanyProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            
            _repository.Save(profile);
        }
    }
}