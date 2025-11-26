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
            using (var context = new dbContext())
            {
                // Returner blot den første, eller null hvis ingen findes.
                return context.CompanyProfile.FirstOrDefault()!;
            }
        }

        public void Save(CompanyProfile profile)
        {
            using (var context = new dbContext())
            {
                // Tjek om profilen allerede findes i databasen (baseret på UUID)
                var exists = context.CompanyProfile.Any(x => x.Uuid == profile.Uuid);

                if (!exists)
                {
                    // Hvis den ikke findes, tilføjer vi den (Insert)
                    context.CompanyProfile.Add(profile);
                }
                else
                {
                    // Hvis den findes, opdaterer vi den (Update)
                    context.CompanyProfile.Update(profile);
                }

                context.SaveChanges();
            }
        }
    }
}