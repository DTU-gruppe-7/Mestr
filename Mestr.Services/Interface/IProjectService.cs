using System;
using System.Collections.Generic;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Core.Enum;
using Mestr.Core.Interface;

namespace Mestr.Services.Interface
{
    public interface IProjectService
    {
        Project CreateProject(string name, string client, string description, DateTime? endDate);
        Project? GetProjectByUuid(Guid uuid);
        IEnumerable<Project> LoadAllProjects();
        IEnumerable<Project> LoadOngoingProjects();
        IEnumerable<Project> LoadCompletedProjects();
        void CompleteProject(Guid projectId);
        void UpdateProject(Project project);
        void UpdateProjectStatus(Guid projectId, ProjectStatus newStatus);
        void DeleteProject(Guid projectId);
        
        // Nye metoder til at håndtere expenses og earnings
        void AddExpenseToProject(Guid projectUuid, Expense expense);
        void AddEarningToProject(Guid projectUuid, Earning earning);
        void RemoveExpenseFromProject(Guid projectUuid, Guid expenseUuid);
        void RemoveEarningFromProject(Guid projectUuid, Guid earningUuid);
    }
}
