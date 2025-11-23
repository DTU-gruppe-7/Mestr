using System;

namespace Mestr.Core.Model
{
    public class CompanyProfile
    {
        // Vi bruger en fast ID eller bare tager den første, da der kun er én profil
        public Guid Uuid { get; private set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Cvr { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BankRegNumber { get; set; } = string.Empty; // Til faktura
        public string BankAccountNumber { get; set; } = string.Empty; // Til faktura

        // EF Core constructor
        private CompanyProfile() { }

        public CompanyProfile(string companyName, string email)
        {
            Uuid = Guid.NewGuid();
            CompanyName = companyName;
            Email = email;
        }
    }
}