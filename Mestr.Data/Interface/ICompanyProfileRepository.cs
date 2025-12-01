using Mestr.Core.Model;

namespace Mestr.Data.Interface
{
    public interface ICompanyProfileRepository
    {
        CompanyProfile? Get();
        void Save(CompanyProfile profile);
    }
}