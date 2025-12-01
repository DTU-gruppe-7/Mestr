using System;
using System.Collections.Generic;
using Mestr.Core.Constants;

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

    // Private constructor for factory method - ensures validation
    private Client(Guid uuid, string? companyName, string contactPerson, string email, 
                   string phoneNumber, string address, string postalAddress, string city, string? cvr,
                   bool skipValidation)
    {
        if (skipValidation)
        {
            // Used by EF Core - direct assignment without validation
            this._uuid = uuid;
            this.companyName = companyName ?? string.Empty;
            this.contactPerson = contactPerson;
            this.email = email;
            this.phoneNumber = phoneNumber;
            this.address = address;
            this.postalAddress = postalAddress;
            this.city = city;
            this.cvr = cvr;
        }
        else
        {
            // Use properties to trigger validation
            this._uuid = uuid;
            this.Name = companyName;
            this.ContactPerson = contactPerson;
            this.Email = email;  // Triggers validation
            this.PhoneNumber = phoneNumber;  // Triggers validation
            this.Address = address;
            this.PostalAddress = postalAddress;
            this.City = city;
            this.Cvr = cvr;
        }
    }

    /// <summary>
    /// Factory method to create a new Client with full validation.
    /// Use this method instead of the constructor to ensure all validation rules are applied.
    /// </summary>
    public static Client Create(Guid uuid, string? companyName, string contactPerson, 
                                string email, string phoneNumber, string address,
                                string postalAddress, string city, string? cvr = null)
    {
        // Validate required fields before creating
        if (string.IsNullOrWhiteSpace(contactPerson))
            throw new ArgumentException(AppConstants.ErrorMessages.ContactPersonRequired, nameof(contactPerson));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(AppConstants.ErrorMessages.EmailRequired, nameof(email));
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException(AppConstants.ErrorMessages.PhoneNumberRequired, nameof(phoneNumber));

        // Create instance - this will trigger property validation
        return new Client(uuid, companyName, contactPerson, email, phoneNumber, 
                         address, postalAddress, city, cvr, skipValidation: false);
    }

    /// <summary>
    /// Creates a Client without validation. For internal use (e.g., EF Core, testing).
    /// </summary>
    internal static Client CreateWithoutValidation(Guid uuid, string? companyName, string contactPerson,
                                                   string email, string phoneNumber, string address,
                                                   string postalAddress, string city, string? cvr = null)
    {
        return new Client(uuid, companyName, contactPerson, email, phoneNumber,
                         address, postalAddress, city, cvr, skipValidation: true);
    }

    // Properties 
    public Guid Uuid { get => _uuid; private set => _uuid = value; }
    
    public string Name
    {
        get => companyName ?? contactPerson;
        set => companyName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public string ContactPerson 
    { 
        get => contactPerson; 
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(AppConstants.ErrorMessages.ContactPersonRequired, nameof(ContactPerson));
            contactPerson = value;
        } 
    }

    public string Email 
    { 
        get => email; 
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(AppConstants.ErrorMessages.EmailRequired, nameof(Email));
            
            if (!IsValidEmail(value))
                throw new ArgumentException(AppConstants.ErrorMessages.EmailInvalid, nameof(Email));
            
            email = value;
        } 
    }
    
    public string PhoneNumber 
    { 
        get => phoneNumber; 
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(AppConstants.ErrorMessages.PhoneNumberRequired, nameof(PhoneNumber));
            
            if (!IsValidPhoneNumber(value))
                throw new ArgumentException(AppConstants.ErrorMessages.PhoneNumberInvalid, nameof(PhoneNumber));
            
            phoneNumber = value;
        } 
    }
    
    public string Address { get => address; set => address = value ?? string.Empty; }
    public string PostalAddress { get => postalAddress; set => postalAddress = value ?? string.Empty; }
    public string City { get => city; set => city = value ?? string.Empty; }
    public string? Cvr { get => cvr; set => cvr = value; }
    
    // Navigation property for related projects
    public ICollection<Project> Projects
    {
        get => projects; 
        set => projects = value ?? new List<Project>();
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
        bool isInternational = phoneNumber.StartsWith(AppConstants.PhoneNumber.InternationalPrefix);

        // If international, remove the + for digit check
        string numberToCheck = isInternational ? phoneNumber[1..] : phoneNumber;

        // Check if remaining characters are only digits
        if (!numberToCheck.All(char.IsDigit))
            return false;

        // Check length (E.164 standard)
        return numberToCheck.Length >= AppConstants.PhoneNumber.MinLength && 
               numberToCheck.Length <= AppConstants.PhoneNumber.MaxLength;
    }
}
