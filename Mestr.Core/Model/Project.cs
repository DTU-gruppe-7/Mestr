using Mestr.Core.Enum;
using Mestr.Core.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Mestr.Core.Model;
public class Project : IProject, INotifyPropertyChanged
{
    private Guid _uuid;
    private string name = string.Empty;
    private Client client = null!;
    private DateTime createdDate;
    private DateTime startDate;
    private DateTime? endDate; 
    private string description = string.Empty;
    private ProjectStatus status;
    private IList<Expense> expenses = new List<Expense>();
    private IList<Earning> earnings = new List<Earning>();

    // EF Core requires a parameterless constructor
    private Project()
    {
    }

    public Project(Guid uuid, string name, IClient client, DateTime createdDate, DateTime startDate, 
                   string description, ProjectStatus status, DateTime? endDate = null,
                   IList<IExpense>? expenses = null, IList<IEarning>? earnings = null)
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
    IClient IProject.Client { get => client; set => client = (Client)value; }
    public DateTime CreatedDate { get => createdDate; set => createdDate = value; }
    public DateTime StartDate { get => startDate; set => startDate = value; }
    public DateTime? EndDate { get => endDate; set => endDate = value; }
    public string Description { get => description; set => description = value; }
    public ProjectStatus Status { get => status; set => status = value; }
    public IList<Expense> Expenses { get => expenses; set => expenses = value; }
    IList<IExpense> IProject.Expenses { get => expenses.Cast<IExpense>().ToList(); set => expenses = value.Select(e => (Expense)e).ToList(); }
    public IList<Earning> Earnings { get => earnings; set => earnings = value; }
    IList<IEarning> IProject.Earnings { get => earnings.Cast<IEarning>().ToList(); set => earnings = value.Select(e => (Earning)e).ToList(); }
    
    public bool IsFinished()
    {
        return _endDate.HasValue && _endDate.Value <= DateTime.Now;
    }
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
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
}
