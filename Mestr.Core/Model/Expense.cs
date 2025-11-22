using System;
using Mestr.Core.Enum;
using Mestr.Core.Interface;

namespace Mestr.Core.Model;

public class Expense : IExpense
{
	private Guid _uuid;
    private string description = string.Empty;
	private decimal amount;
	private DateTime date;
	private ExpenseCategory category;
	private bool isAccepted;
	private Project? project;
	private Guid projectUuid;

    // EF Core requires a parameterless constructor
    private Expense()
    {
    }

    public Expense(Guid uuid, string description, decimal amount, DateTime date,
        ExpenseCategory category, bool isAccepted)
	{
		this._uuid = uuid;
        this.description = description;
		this.amount = amount;
		this.date = date;
		this.category = category;
		this.isAccepted = isAccepted;
    }

	public Guid Uuid { get => _uuid; private set => _uuid = value; }
    public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public ExpenseCategory Category { get => category; set => category = value; }
	public bool IsAccepted { get => isAccepted; set => isAccepted = value; }
	
	// Navigation properties
	public Project? Project { get => project; set => project = value; }
	public Guid ProjectUuid { get => projectUuid; set => projectUuid = value; }

	public void Accept()
	{
		this.isAccepted = true;
    }
}
