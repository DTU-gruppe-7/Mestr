using Mestr.Core.Interface;
using System;
using System.Collections.Generic;

namespace Mestr.Core.Model;
public class Client : IClient
{
	private readonly Guid _uuid;
	private string name;
	private string email;
	private string phoneNumber;
    private string address;
	private string postalAddress;
	private string city;
	private string? cvr;
	private readonly DateTime initDate;
    private IList<IProject> projects;

    // Constructor
    public Client(Guid uuid, string name, string email, string phoneNumber, string address,
                  string postalAddress, string city, string? cvr = null)
    {
        this._uuid = uuid;
        this.name = name;
        this.email = email;
        this.phoneNumber = phoneNumber;
        this.address = address;
        this.postalAddress = postalAddress;
        this.city = city;
        this.cvr = cvr;
        this.initDate = DateTime.Now;
        this.projects = new List<IProject>();
    }


    // Properties 
    public Guid Uuid { get => _uuid; }
    public string Name {get => name; set => name = value;}
    
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
    public DateTime InitDate { get; }
    public IList<IProject> Projects { get => projects; set => projects = value; }

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
        // A simple phone number validation: check if it contains only digits and has a valid length
        // This is a basic validation, you can expand it based on your requirements
        return phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 7 && phoneNumber.Length <= 15;
    }
}
