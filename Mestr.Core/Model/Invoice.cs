using System;

public class Invoice
{
	private Guid uuid;
	private Guid projectUuid;
	private string invoiceNumber;
	private DateTime invoiceDate;
	private DateTime dueDate;
	private decimal subTotal;
	private decimal vatPercentage;
	private decimal vatAmount;
	private decimal totalAmount;
	private InvoiceStatus status;
	private string filePath;
	private Project project;

    public Invoice(Guid uuid, Guid projectUuid, string invoiceNumber, DateTime invoiceDate, DateTime dueDate, decimal subTotal,
		decimal vatPercentage, decimal vatAmount, decimal totalAmount, InvoiceStatus status, string filePath, Project project)
	{
		this.uuid = uuid;
		this.projectUuid = projectUuid;
		this.invoiceNumber = invoiceNumber;
		this.invoiceDate = invoiceDate;
		this.dueDate = dueDate;
		this.subTotal = subTotal;
		this.vatPercentage = vatPercentage;
		this.vatAmount = vatAmount;
		this.totalAmount = totalAmount;
		this.status = status;
		this.filePath = filePath;
		this.project = project;
    }
	public Guid Uuid { get => uuid; set => uuid = value; }
	public Guid ProjectUuid { get => projectUuid; set => projectUuid = value; }
	public string InvoiceNumber { get => invoiceNumber; set => invoiceNumber = value; }
	public DateTime InvoiceDate { get => invoiceDate; set => invoiceDate = value; }
	public DateTime DueDate { get => dueDate; set => dueDate = value; }
	public decimal SubTotal { get => subTotal; set => subTotal = value; }
	public decimal VatPercentage { get => vatPercentage; set => vatPercentage = value; }
	public decimal VatAmount { get => vatAmount; set => vatAmount = value; }
	public decimal TotalAmount { get => totalAmount; set => totalAmount = value; }
	public InvoiceStatus Status { get => status; set => status = value; }
	public string FilePath { get => filePath; set => filePath = value; }
	public Project Project { get => project; set => project = value; }

	public void calculateTotal()//parameter input)
	{
        // Implementation to calculate total amount
    }

	public void generatePdf()//parameter input)
	{
        // Implementation to generate PDF invoice
    }

	public void sendInvoice()//parameter input)
	{
        // Implementation to send invoice to client
    }
}
