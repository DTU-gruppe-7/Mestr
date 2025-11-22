using Mestr.Core.Interface;
using System;

namespace Mestr.Core.Model;
public class Earning : IEarning
{
	private Guid _uuid;
	private string description = string.Empty;
	private decimal amount;
	private DateTime date;
	private bool isPaid;
	private Project? project;
	private Guid projectUuid;

    // EF Core requires a parameterless constructor
    private Earning()
    {
    }

    public Earning(Guid uuid, string description, decimal amount,
		DateTime date, bool isPaid)
	{
		this._uuid = uuid;
        this.description = description;
		this.amount = amount;
		this.date = date;
		this.isPaid = isPaid;
    }

	public Guid Uuid { get => _uuid; private set => _uuid = value; }
	public string Description { get => description; set => description = value; }
	public decimal Amount { get => amount; set => amount = value; }
	public DateTime Date { get => date; set => date = value; }
	public bool IsPaid { get => isPaid; set => isPaid = value; }
	
	// Navigation properties
	public Project? Project { get => project; set => project = value; }
	public Guid ProjectUuid { get => projectUuid; set => projectUuid = value; }

	public void MarkAsPaid(DateTime paymentDate)
	{
		this.date = paymentDate;
		this.isPaid = true;
    }
}
