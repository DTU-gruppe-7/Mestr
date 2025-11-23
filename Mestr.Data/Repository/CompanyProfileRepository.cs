using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using System.Linq;

namespace Mestr.Data.Repository
{
    public class CompanyProfileRepository : ICompanyProfileRepository
    {
        public CompanyProfile Get()
        {
            dbContext.DatabaseLock.Wait();
            try
            {
                var profile = dbContext.Instance.CompanyProfile.FirstOrDefault();
                if (profile == null)
                {
                    // Opret en standard profil hvis ingen findes
                    profile = new CompanyProfile("Mit Firma", "mig@example.com");
                    dbContext.Instance.CompanyProfile.Add(profile);
                    dbContext.Instance.SaveChanges();
                }
                return profile;
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }

        public void Save(CompanyProfile profile)
        {
            dbContext.DatabaseLock.Wait();
            try
            {
                // Fordi vi ved der kun er én, kan vi bare opdatere den
                dbContext.Instance.CompanyProfile.Update(profile);
                dbContext.Instance.SaveChanges();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }
    }
}