using System;
using System.Collections.Generic;
using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Core.Enum;
using System.Threading.Tasks;

namespace Mestr.Services.Interface
{
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(string name, Client client, string description, DateTime? endDate);
        Task<Project?> GetProjectByUuidAsync(Guid uuid);
        Task<IEnumerable<Project>> LoadAllProjectsAsync();
        Task<IEnumerable<Project>> LoadOngoingProjectsAsync();
        Task<IEnumerable<Project>> LoadCompletedProjectsAsync();
        Task CompleteProjectAsync(Guid projectId);
        Task UpdateProjectAsync(Project project);
        Task UpdateProjectStatusAsync(Guid projectId, ProjectStatus newStatus);
        Task DeleteProjectAsync(Guid projectId);
        
        // Nye metoder til at håndtere expenses og earnings
        Task AddExpenseToProjectAsync(Guid projectUuid, Expense expense);
        Task AddEarningToProjectAsync(Guid projectUuid, Earning earning);
        Task RemoveExpenseFromProjectAsync(Guid projectUuid, Guid expenseUuid);
        Task RemoveEarningFromProjectAsync(Guid projectUuid, Guid earningUuid);
    }
}
