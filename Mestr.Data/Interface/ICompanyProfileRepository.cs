using Mestr.Core.Model;

namespace Mestr.Data.Interface
{
    public interface ICompanyProfileRepository
    {
        CompanyProfile Get(); // Henter den ene profil der findes (eller opretter en tom)
        void Save(CompanyProfile profile);
    }
}