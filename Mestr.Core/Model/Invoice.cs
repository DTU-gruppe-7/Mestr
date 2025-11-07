using System;

public class Class1
{
	private Guid uuid;
	private Guid projectuuid;
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
	private List<InvoiceLine> Lines;

    public Class1(Guid? uuid = Guid.NewGuid(), Guid projectuuid, string invoiceNumber, DateTime invoiceDate, DateTime dueDate, decimal subTotal,
		decimal vatPercentage, decimal vatAmount, decimal totalAmount, InvoiceStatus status, string filePath, Project project)
	{
		this.uuid = uuid;
		this.projectuuid = projectuuid;
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
		this.Lines = new List<InvoiceLine>;
    }
	public int Uuid { get => uuid; set => uuid = value; }
	public int Projectuuid { get => projectuuid; set => projectuuid = value; }
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
	public List<InvoiceLine> Lines { get => Lines; set => Lines = value; }

	public void calculateTotal(//parameter input)
	{
        // Implementation to calculate total amount
    }

	public void generatePdf(//parameter input)
	{
        // Implementation to generate PDF invoice
    }

	public void sendInvoice(//parameter input)
	{
        // Implementation to send invoice to client
    }
}
