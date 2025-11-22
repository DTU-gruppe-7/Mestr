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
    private string _name;
    private DateTime _createdDate;
    private DateTime _startDate;
    private DateTime? _endDate;
    private string _description;
    private ProjectStatus _status;
    private IList<IExpense> _expenses;
    private IList<IEarning> _earnings;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Project(Guid uuid, string name, DateTime createdDate, DateTime startDate, 
                   string description, ProjectStatus status, DateTime? endDate = null)
    {
        this._uuid = uuid;
        this._name = name;
        this._createdDate = createdDate;
        this._startDate = startDate;
        this._description = description;
        this._status = status;
        this._endDate = endDate;

        this._expenses = new List<IExpense>();
        this._earnings = new List<IEarning>();

    }

    // Properties 
    public Guid Uuid { get => _uuid; private set => _uuid = value; }

    public string Name
    {
        get => _name; set => SetProperty(ref _name, value); 
    }
    public DateTime CreatedDate
    {
        get => _createdDate; set => SetProperty(ref _createdDate, value); 
    }
    public DateTime StartDate
    {
        get => _startDate; set => SetProperty(ref _startDate, value); 
    }
    public DateTime? EndDate
    {
        get => _endDate;  set => SetProperty(ref _endDate, value);
    }
    public string Description
    {
        get => _description; set => SetProperty(ref _description, value); 
    }
    public ProjectStatus Status
    {
        get => _status; set => SetProperty(ref _status, value); 
    }
    public IList<IExpense> Expenses
    {
        get => _expenses; set => SetProperty(ref _expenses, value);
    }
    public IList<IEarning> Earnings
    {
        get => _earnings; set => SetProperty(ref _earnings, value); 
    }

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
}
