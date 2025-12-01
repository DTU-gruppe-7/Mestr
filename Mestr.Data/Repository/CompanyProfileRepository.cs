using Mestr.Core.Model;
using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using System.Linq;

namespace Mestr.Data.Repository
{
    public class CompanyProfileRepository : ICompanyProfileRepository
    {
        public CompanyProfile? Get()
        {
            using (var context = new dbContext())
            {
                // Return the first profile or null if none exists
                return context.CompanyProfile.FirstOrDefault();
            }
        }

        public void Save(CompanyProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            
            using (var context = new dbContext())
            {
                // Check if profile already exists in database (based on UUID)
                var exists = context.CompanyProfile.Any(x => x.Uuid == profile.Uuid);

                if (!exists)
                {
                    // If not found, add it (Insert)
                    context.CompanyProfile.Add(profile);
                }
                else
                {
                    // If found, update it (Update)
                    context.CompanyProfile.Update(profile);
                }

                context.SaveChanges();
            }
        }
    }
}