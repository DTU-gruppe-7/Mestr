using System;
using Mestr.Core.Enum;

namespace Mestr.Core.Model;

public class Expense
{
	private Guid uuid;
    private Guid projectUuid;
    private string description;
	private decimal amount;
	private DateTime date;
	private ExpenseCategory category;// Assuming ExpenseCategory is defined elsewhere 
	//private string receiptPath;
	private bool isAccepted;
	

    public Expense(Guid uuid, Guid projectUuid, string description, decimal amount, DateTime date,
        ExpenseCategory category, bool isAccepted)
	{
		this.uuid = uuid;
        this.projectUuid = projectUuid;
        this.description = description;
		this.amount = amount;
		this.date = date;
		this.category = category;
		//this.receiptPath = receiptPath;
		this.isAccepted = isAccepted;
		
    }
	public Guid Uuid { get => uuid;}
	public Guid ProjectUuid { get => projectUuid; }
    public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public ExpenseCategory Category { get => category; set => category = value; }
	//public string ReceiptPath { get => receiptPath; set => receiptPath = value; }
	public bool IsAccepted { get => isAccepted; set => isAccepted = value; }

	public void Accept()
	{

	}
}
