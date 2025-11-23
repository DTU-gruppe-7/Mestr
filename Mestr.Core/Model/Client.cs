using System;
using System.Collections.Generic;

namespace Mestr.Core.Model;
public class Client
{
	private Guid _uuid;
	private string companyName = string.Empty;
	private string contactPerson = string.Empty;
	private string email = string.Empty;
	private string phoneNumber = string.Empty;
	private string address = string.Empty;
	private string postalAddress = string.Empty;
	private string city = string.Empty;

	private string? cvr;
    private ICollection<Project> projects = new List<Project>();

    // EF Core requires a parameterless constructor
    private Client()
    {
    }

    // Constructor
    public Client(Guid uuid, string? companyName, string contactPerson, string email, string phoneNumber, string address,
                  string postalAddress, string city, string? cvr)
    {
        this._uuid = uuid;
        this.companyName = companyName;
        this.contactPerson = contactPerson;
        this.email = email;
        this.phoneNumber = phoneNumber;
        this.address = address;
        this.postalAddress = postalAddress;
        this.city = city;
        this.cvr = cvr;
    }


    // Properties 
    public Guid Uuid { get => _uuid; private set => _uuid = value; }
    public string Name
    {
        get => companyName ?? contactPerson;
        set => companyName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public string ContactPerson { get => contactPerson; set => contactPerson = value; }

    public string Email 
    { 
        get => email; 
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.", nameof(Email));
            
            if (!IsValidEmail(value))
                throw new ArgumentException("Invalid email format.", nameof(Email));
            
            email = value;
        } 
    }
    public string PhoneNumber 
    { 
        get => phoneNumber; 
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be empty.", nameof(PhoneNumber));
            
            if (!IsValidPhoneNumber(value))
                throw new ArgumentException("Invalid phone number format.", nameof(PhoneNumber));
            
            phoneNumber = value;
        } 
    }
    public string Address { get => address; set => address = value; }
    public string PostalAddress { get => postalAddress; set => postalAddress = value; }
    public string City { get => city; set => city = value; }
    public string? Cvr { get => cvr; set => cvr = value; }
    // Navigation property for related projects
    public ICollection<Project> Projects
    {
        get => projects; set => projects = value;
    }

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

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Check if phone number contains spaces - not allowed
        if (phoneNumber.Contains(" "))
            return false;

        // Check if phone number starts with + (international format)
        bool isInternational = phoneNumber.StartsWith("+");

        // If international, remove the + for digit check
        string numberToCheck = isInternational ? phoneNumber[1..] : phoneNumber;

        // Check if remaining characters are only digits
        if (!numberToCheck.All(char.IsDigit))
            return false;

        // Check length (E.164 standard: 8-15 digits, excluding country code prefix)
        return numberToCheck.Length >= 8 && numberToCheck.Length <= 15;
    }
}
