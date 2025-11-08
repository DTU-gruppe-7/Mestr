using Mestr.Core.Interface;
using System;

namespace Mestr.Core.Model;
public class Earning : IEarning
{
	private readonly Guid _uuid;
	private Guid projectUuid;
	private string description;
	private decimal amount;
	private DateTime date;
	private bool isPaid;

    public Earning(Guid uuid, Guid projectuuid, string description, decimal amount,
		DateTime date, bool isPaid)
	{
		this._uuid = uuid;
		this.projectUuid = projectuuid;
		this.description = description;
		this.amount = amount;
		this.date = date;
		this.isPaid = isPaid;
    }
	public Guid Uuid { get => _uuid;}
	public Guid ProjectUuid { get => projectUuid; set => projectUuid = value; }
	public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public bool IsPaid { get => isPaid; set => isPaid = value; }

	public void MarkAsPaid(DateTime paymentDate)
	{
		this.date = paymentDate;
		this.isPaid = true;
    }
}
