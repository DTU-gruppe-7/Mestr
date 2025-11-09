using System;
using System.Collections.Generic;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Core.Enum;

namespace Mestr.Services.Interface
{
    public interface IProjectService
    {
        Project CreateProject(string name, string description, DateTime? endDate);
        Project? GetProjectByUuid(Guid uuid);
        IEnumerable<Project> LoadAllProjects();
        IEnumerable<Project> LoadOngoingProjects();
        IEnumerable<Project> LoadCompletedProjects();
        void CompleteProject(Guid projectId);

    }
}
