using System;
using System.Collections.Generic;

public class Project
{
    // Private fields
    private Guid uuid;
    private string name;
    private DateTime createdDate;
    private DateTime startDate;
    private DateTime? endDate; 
    private string description;
    private ProjectStatus status;
    //private Client client;
    private List<Expense> expenses;
    private List<Earning> earnings;
    //private decimal totalExpenses;
    //private decimal totalEarnings;
    //private decimal result;
    //private double profitability;

    // Constructor
    public Project(Guid uuid, string name, DateTime createdDate, DateTime startDate, 
                   string description, ProjectStatus status, DateTime? endDate = null)
    {
        this.uuid = uuid;
        this.name = name;
        this.createdDate = createdDate;
        this.createdDate = DateTime.Now;
        this.startDate = startDate;
        this.description = description;
        this.status = status;
        //this.client = client;
        this.endDate = endDate;

        // Initialiser lister
        this.expenses = new List<Expense>();
        this.earnings = new List<Earning>();

        //this.totalExpenses = 0;
        //this.totalEarnings = 0;
        //this.result = 0;
        //this.profitability = 0;
    }

    // Properties 
    public Guid Uuid { get => uuid;}
    public string Name { get => name;}
    public DateTime CreatedDate { get => createdDate; set => createdDate = value; }
    public DateTime StartDate { get => startDate; set => startDate = value; }
    public DateTime? EndDate { get => endDate; set => endDate = value; }
    public string Description { get => description; set => description = value; }
    public ProjectStatus Status { get => status; set => status = value; }
    //public Client Client { get => client; set => client = value; }
    public List<Expense> Expenses { get => expenses; set => expenses = value; }
    public List<Earning> Earnings { get => earnings; set => earnings = value; }
    //public decimal TotalExpenses { get => totalExpenses; set => totalExpenses = value; }
    //public decimal TotalEarnings { get => totalEarnings; set => totalEarnings = value; }
    //public decimal Result { get => result; set => result = value; }
    //public double Profitability { get => profitability; set => profitability = value; }
    
    public bool isFinished()
    {
        return endDate.HasValue && endDate.Value <= DateTime.Now;
    }
}
