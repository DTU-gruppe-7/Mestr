using System;
using Mestr.Core.Enum;
using Mestr.Core.Interface;

namespace Mestr.Core.Model;

public class Expense : IExpense
{
	private readonly Guid _uuid;
    private Guid projectUuid;
    private string description;
	private decimal amount;
	private DateTime date;
	private ExpenseCategory category;
	private bool isAccepted;
	

    public Expense(Guid uuid, Guid projectUuid, string description, decimal amount, DateTime date,
        ExpenseCategory category, bool isAccepted)
	{
		this._uuid = uuid;
        this.projectUuid = projectUuid;
        this.description = description;
		this.amount = amount;
		this.date = date;
		this.category = category;
		this.isAccepted = isAccepted;
		
    }
	public Guid Uuid { get => _uuid;}
	public Guid ProjectUuid { get => projectUuid; set => projectUuid = value; }
    public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public ExpenseCategory Category { get => category; set => category = value; }
	public bool IsAccepted { get => isAccepted; set => isAccepted = value; }

	public void Accept()
	{
		this.isAccepted = true;
    }
}
