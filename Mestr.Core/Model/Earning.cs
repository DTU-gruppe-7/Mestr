using System;

public class Earning
{
	private Guid uuid;
	private Guid projectuuid;
	private string description;
	private decimal amount;
	private DateTime date;
	private bool isPaid;
	private DateTime? paymentDate;
	private Project project;

    public Earning(Guid uuid, Guid projectuuid, string description, decimal amount,
		DateTime date, bool isPaid, DateTime? paymentDate = null, Project project)
	{
		this.uuid = uuid;
		this.projectuuid = projectuuid;
		this.description = description;
		this.amount = amount;
		this.date = date;
		this.isPaid = isPaid;
		this.paymentDate = paymentDate;
		this.project = project;
    }
	public Guid Uuid { get => uuid; set => uuid = value; }
	public Guid Projectuuid { get => projectuuid; set => projectuuid = value; }
	public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public bool IsPaid { get => isPaid; set => isPaid = value; }
	public DateTime? PaymentDate { get => paymentDate; set => paymentDate = value; }
	private Project Project { get => project; set => project = value; }

	public void MarkAsPaid(DateTime paymentDate)
	{
		this.isPaid = true;
		this.paymentDate = paymentDate;
    }
}
