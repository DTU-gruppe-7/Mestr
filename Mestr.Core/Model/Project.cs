using Mestr.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Mestr.Core.Model;
public class Project
{
    private Guid _uuid;
    private string name = string.Empty;
    private Client client = null!;
    private DateTime createdDate;
    private DateTime startDate;
    private DateTime? endDate; 
    private string description = string.Empty;
    private ProjectStatus status;
    private IList<Expense> expenses;
    private IList<Earning> earnings;

    // EF Core requires a parameterless constructor
    private Project()
    { 
    }

    public Project(Guid uuid, string name, Client client, DateTime createdDate, DateTime startDate, 
                   string description, ProjectStatus status, DateTime? endDate = null,
                   IList<Expense>? expenses = null, IList<Earning>? earnings = null)
    {
        this._uuid = uuid;
        this.name = name;
        this.client = (Client)client;
        this.createdDate = createdDate;
        this.startDate = startDate;
        this.description = description;
        this.status = status;
        this.endDate = endDate;

        this.expenses = expenses?.Select(e => (Expense)e).ToList() ?? new List<Expense>();
        this.earnings = earnings?.Select(e => (Earning)e).ToList() ?? new List<Earning>();
    }

    // Properties - use concrete types for EF Core
    public Guid Uuid { get => _uuid; private set => _uuid = value; }
    public string Name { get => name; set => name = value; }
    public Client Client { get => client; set => client = value; }
    public DateTime CreatedDate { get => createdDate; set => createdDate = value; }
    public DateTime StartDate { get => startDate; set => startDate = value; }
    public DateTime? EndDate { get => endDate; set => endDate = value; }
    public string Description { get => description; set => description = value; }
    public ProjectStatus Status { get => status; set => status = value; }
    public IList<Expense> Expenses { get => expenses; set => expenses = value; }
    public IList<Earning> Earnings { get => earnings; set => earnings = value; }
    
    public bool IsFinished()
    {
        return endDate.HasValue && endDate.Value <= DateTime.Now;
    }

    public decimal Result
    {
        get
        {
            decimal totalEarnings = earnings?.Sum(e => e.Amount) ?? 0;
            Console.WriteLine("The earnings sum op to: " + totalEarnings );
            decimal totalExpenses = expenses?.Sum(e => e.Amount) ?? 0;
            Console.WriteLine("The expenses sum op to: " + totalExpenses);
            return totalEarnings - totalExpenses;
        }
    }
    public String ResultColor
    {
        get
        {
            if (Result > 0)
                return "#22C55E";
            else if (Result < 0)
                return "#EF4444";
            else 
                return "#6B7280";
        }
    }
}
