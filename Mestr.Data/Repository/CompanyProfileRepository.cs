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
                // Returner blot den første, eller null hvis ingen findes.
                return dbContext.Instance.CompanyProfile.FirstOrDefault()!;
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
                // Tjek om profilen allerede findes i databasen (baseret på UUID)
                var exists = dbContext.Instance.CompanyProfile.Any(x => x.Uuid == profile.Uuid);

                if (!exists)
                {
                    // Hvis den ikke findes, tilføjer vi den (Insert)
                    dbContext.Instance.CompanyProfile.Add(profile);
                }
                else
                {
                    // Hvis den findes, opdaterer vi den (Update)
                    dbContext.Instance.CompanyProfile.Update(profile);
                }

                dbContext.Instance.SaveChanges();
            }
            finally
            {
                dbContext.DatabaseLock.Release();
            }
        }
    }
}