using Mestr.Core.Enum;
using Mestr.Core.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;

namespace Mestr.Core.Model;
public class Project : IProject, INotifyPropertyChanged
{
    private Guid _uuid;
    private string name;
    private DateTime createdDate;
    private DateTime startDate;
    private DateTime? endDate; 
    private string description;
    private ProjectStatus status;
    private IList<IExpense> expenses;
    private IList<IEarning> earnings;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Project(Guid uuid, string name, DateTime createdDate, DateTime startDate, 
                   string description, ProjectStatus status, DateTime? endDate = null)
    {
        this._uuid = uuid;
        this.name = name;
        this.createdDate = createdDate;
        this.startDate = startDate;
        this.description = description;
        this.status = status;
        this.endDate = endDate;

        this.expenses = new List<IExpense>();
        this.earnings = new List<IEarning>();

    }

    // Properties 
    public Guid Uuid { get => _uuid; private set => _uuid = value; }
    public string Name { get => name; set => name = value; }
    public DateTime CreatedDate { get => createdDate; set => createdDate = value; }
    public DateTime StartDate { get => startDate; set => startDate = value; }
    public DateTime? EndDate { get => endDate; set => endDate = value; }
    public string Description { get => description; set => description = value; }
    public ProjectStatus Status { get => status; set => status = value; }
    public IList<IExpense> Expenses { get => expenses; set => expenses = value; }
    public IList<IEarning> Earnings { get => earnings; set => earnings = value; }
    
    public bool IsFinished()
    {
        return endDate.HasValue && endDate.Value <= DateTime.Now;
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
}
