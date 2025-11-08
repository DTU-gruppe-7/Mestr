using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Core.Enum;

namespace Mestr.Services.Interface
{
    public interface IProjectService 
    {
        Project CreateProject(string name, string description, DateTime? endDate);
        IEnumerable <Project> LoadAllProject();
    }
}
