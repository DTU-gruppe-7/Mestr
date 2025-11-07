using System;
using System.Collections.Generic;

public class Client
{
	private Guid uuid;
	private string name;
	private string email;
	private string phoneNumber;
    private string address;
	private string postalAddress;
	private string city;
	private string? cvr;
	private DateTime initDate;
    private List<Project> projects;

    // Constructor
    public Client(Guid uuid, string name, string email, string phoneNumber, string address,
                  string postalAddress, string city, List<Project>? projects = null,
                  string? cvr = null)
    {
        this.uuid = uuid;
        this.name = name;
        this.email = email;
        this.phoneNumber = phoneNumber;
        this.address = address;
        this.postalAddress = postalAddress;
        this.city = city;
        this.cvr = cvr;
        this.initDate = DateTime.Now;
        this.projects = projects ?? new List<Project>();
    }


    // Properties 
    public Guid UUId { get => uuid; }
    public string Name { get => name; set => name = value; }
    public string Email { get => email; set => email = value; }
    public string PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
    public string Address { get => address; set => address = value; }
    public string PostalAddress { get => postalAddress; set => postalAddress = value; }
    public string City { get => city; set => city = value; }
    public string? Cvr { get => cvr; set => cvr = value; }
    public DateTime InitDate { get => initDate; }
    public List<Project> Projects { get => projects; set => projects = value; }

    // Get entire address
    public string GetFullAddress()
    {
        return $"{address}, {postalAddress} {city}";
    }

    // Check if client is B2B
    public bool IsBusinessClient()
    {
        return !string.IsNullOrEmpty(cvr);
    }

}
