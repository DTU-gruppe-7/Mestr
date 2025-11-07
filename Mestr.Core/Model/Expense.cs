using System;

public class Expense
{
	private Guid uuid;
	private Guid projectuuid;
	private string description;
	private decimal amount;
	private DateTime date;
	private ExpenseCategory category;// Assuming ExpenseCategory is defined elsewhere 
	private string receiptpath;
	private bool isAccepted;
	private Project project;

    public Expense(Guid uuid, Guid projectuuid, string description, decimal amount, DateTime date,
        ExpenseCategory category, string receiptpath, bool isAccepted, Project project)
	{
		this.uuid = uuid;
		this.projectuuid = projectuuid;
		this.description = description;
		this.amount = amount;
		this.date = date;
		this.category = category;
		this.receiptpath = receiptpath;
		this.isAccepted = isAccepted;
		this.project = project;
    }
	public Guid Uuid { get => uuid; set => uuid = value; }
	public Guid Projectuuid { get => projectuuid; set => projectuuid = value; }
	public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public ExpenseCategory Category { get => category; set => category = value; }
	public string Receiptpath { get => receiptpath; set => receiptpath = value; }
	public bool IsAccepted { get => isAccepted; set => isAccepted = value; }
	public Project Project { get => project; set => project = value; }

	public void Accept()
	{

	}
}
