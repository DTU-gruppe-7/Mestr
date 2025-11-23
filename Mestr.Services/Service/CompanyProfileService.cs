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

        public CompanyProfileService()
        {
            _repository = new CompanyProfileRepository();
        }

        public CompanyProfile GetProfile()
        {
            return _repository.Get();
        }

        public void UpdateProfile(CompanyProfile profile)
        {
            // Her kunne du validere input (f.eks. at CVR er tal)
            _repository.Save(profile);
        }
    }
}