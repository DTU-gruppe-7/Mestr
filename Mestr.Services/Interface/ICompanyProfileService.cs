using Mestr.Core.Model;
using Mestr.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mestr.Services.Interface
{
    public interface ICompanyProfileService
    {
        CompanyProfile? GetProfile();
        void UpdateProfile(CompanyProfile profile);
        
    }
}
