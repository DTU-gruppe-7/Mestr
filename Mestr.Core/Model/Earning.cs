using System;

namespace Mestr.Core.Model;
public class Earning
{
	private Guid uuid;
	private Guid projectUuid;
	private string description;
	private decimal amount;
	private DateTime date;
	private bool isPaid;
	//private Project project;

    public Earning(Guid uuid, Guid projectuuid, string description, decimal amount,
		DateTime date, bool isPaid)
	{
		this.uuid = uuid;
		this.projectUuid = projectuuid;
		this.description = description;
		this.amount = amount;
		this.date = date;
		this.isPaid = isPaid;
		//this.project = project;
    }
	public Guid Uuid { get => uuid; set => uuid = value; }
	public Guid ProjectUuid { get => projectUuid; set => projectUuid = value; }
	public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public bool IsPaid { get => isPaid; set => isPaid = value; }
	//private Project Project { get => project; set => project = value; }

	public void MarkAsPaid(DateTime paymentDate)
	{
		this.isPaid = true;
    }
}
