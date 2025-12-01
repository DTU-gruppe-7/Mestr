using System;

namespace Mestr.Core.Constants
{
    /// <summary>
    /// Application-wide constants for the Mestr project management system.
    /// Centralizes magic strings and numbers to improve maintainability.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Date and time formatting constants
        /// </summary>
        public static class DateTimeFormats
        {
            /// <summary>
            /// Standard date-time format used throughout the application (yyyy-MM-dd HH:mm:ss)
            /// </summary>
            public const string Standard = "yyyy-MM-dd HH:mm:ss";
            
            /// <summary>
            /// Short date format for invoices and reports (dd-MM-yyyy)
            /// </summary>
            public const string ShortDate = "dd-MM-yyyy";
            
            /// <summary>
            /// Date format for file names (yyyyMMdd)
            /// </summary>
            public const string FileNameDate = "yyyyMMdd";
        }

        /// <summary>
        /// Culture and localization settings
        /// </summary>
        public static class Culture
        {
            /// <summary>
            /// Danish culture code
            /// </summary>
            public const string Danish = "da-DK";
            
            /// <summary>
            /// Currency code for Danish Krone
            /// </summary>
            public const string DanishCurrency = "DKK";
        }

        /// <summary>
        /// Database configuration constants
        /// </summary>
        public static class Database
        {
            /// <summary>
            /// Name of the default connection string in appsettings.json
            /// </summary>
            public const string DefaultConnectionStringName = "DefaultConnection";
            
            /// <summary>
            /// Precision for decimal amounts in database
            /// </summary>
            public const int DecimalPrecision = 18;
            
            /// <summary>
            /// Scale for decimal amounts in database
            /// </summary>
            public const int DecimalScale = 2;
        }

        /// <summary>
        /// Validation limits and constraints
        /// </summary>
        public static class Validation
        {
            /// <summary>
            /// Minimum length for text fields (names, descriptions, etc.)
            /// </summary>
            public const int MinTextLength = 3;
            
            /// <summary>
            /// Maximum length for standard text fields
            /// </summary>
            public const int MaxTextLength = 255;
            
            /// <summary>
            /// Maximum length for description fields
            /// </summary>
            public const int MaxDescriptionLength = 1000;
            
            /// <summary>
            /// Maximum decimal amount (1 million DKK)
            /// </summary>
            public const decimal MaxAmount = 1_000_000m;
            
            /// <summary>
            /// Minimum decimal amount (must be positive)
            /// </summary>
            public const decimal MinAmount = 0.01m;
        }

        /// <summary>
        /// Phone number validation constants
        /// </summary>
        public static class PhoneNumber
        {
            /// <summary>
            /// Minimum phone number length (E.164 standard)
            /// </summary>
            public const int MinLength = 8;
            
            /// <summary>
            /// Maximum phone number length (E.164 standard)
            /// </summary>
            public const int MaxLength = 15;
            
            /// <summary>
            /// International prefix
            /// </summary>
            public const string InternationalPrefix = "+";
            
            /// <summary>
            /// Danish country code
            /// </summary>
            public const string DanishCountryCode = "+45";
        }

        /// <summary>
        /// VAT (Moms) related constants
        /// </summary>
        public static class VAT
        {
            /// <summary>
            /// Standard Danish VAT rate (25%)
            /// </summary>
            public const decimal StandardRate = 0.25m;
            
            /// <summary>
            /// Standard Danish VAT percentage for display
            /// </summary>
            public const decimal StandardPercentage = 25m;
        }

        /// <summary>
        /// Invoice related constants
        /// </summary>
        public static class Invoice
        {
            /// <summary>
            /// Default payment terms in days
            /// </summary>
            public const int DefaultPaymentTermsDays = 14;
            
            /// <summary>
            /// Invoice number prefix length
            /// </summary>
            public const int InvoiceNumberPrefixLength = 8;
            
            /// <summary>
            /// Default file name prefix for generated invoices
            /// </summary>
            public const string FileNamePrefix = "Invoice";
        }

        /// <summary>
        /// UI related constants
        /// </summary>
        public static class UI
        {
            /// <summary>
            /// Transaction types
            /// </summary>
            public static class TransactionTypes
            {
                public const string Expense = "Udgift";
                public const string Earning = "Indtægt";
            }
            
            /// <summary>
            /// Result colors (hexadecimal)
            /// </summary>
            public static class Colors
            {
                /// <summary>
                /// Green color for positive results
                /// </summary>
                public const string Positive = "#22C55E";
                
                /// <summary>
                /// Red color for negative results
                /// </summary>
                public const string Negative = "#EF4444";
                
                /// <summary>
                /// Gray color for neutral/zero results
                /// </summary>
                public const string Neutral = "#6B7280";
            }
        }

        /// <summary>
        /// Error messages (Danish)
        /// </summary>
        public static class ErrorMessages
        {
            public const string ContactPersonRequired = "Kontaktperson kan ikke være tom.";
            public const string EmailRequired = "Email kan ikke være tom.";
            public const string EmailInvalid = "Ugyldig email format.";
            public const string PhoneNumberRequired = "Telefonnummer kan ikke være tom.";
            public const string PhoneNumberInvalid = "Ugyldigt telefonnummer format.";
            public const string PhoneNumberNoSpaces = "Telefonnummer må ikke indeholde mellemrum.";
            public const string PhoneNumberTooShort = "Telefonnummer skal være mindst {0} cifre.";
            public const string PhoneNumberTooLong = "Telefonnummer må maksimalt være {0} cifre.";
            public const string PhoneNumberOnlyDigits = "Telefonnummer må kun indeholde tal og + i starten.";
            
            public const string DescriptionRequired = "Beskrivelse kan ikke være tom.";
            public const string DescriptionTooShort = "Beskrivelse skal være mindst {0} tegn langt.";
            
            public const string AmountMustBePositive = "Beløb skal være større end 0.";
            public const string AmountTooLarge = "Beløb må ikke overstige {0:N0}.";
            
            public const string DateMustBeFuture = "Deadline skal være en fremtidig dato.";
            
            public const string ClientNotFound = "Klient ikke fundet.";
            public const string ProjectNotFound = "Projekt ikke fundet.";
            
            public const string CompanyProfileRequired = "Firmaprofil ikke fundet. Opret venligst en firmaprofil før generering af faktura.";
            public const string CompanyProfileLoadError = "Firmaprofil kunne ikke indlæses. Programmet kan ikke fortsætte.";
            
            public const string DatabaseInitError = "Databasen kunne ikke initialiseres.\n\nFejl: {0}";
            public const string ConnectionStringMissing = "Database connection string '{0}' is not configured. Please check appsettings.json";
        }

        /// <summary>
        /// Success messages (Danish)
        /// </summary>
        public static class SuccessMessages
        {
            public const string ClientCreated = "Klienten blev oprettet.";
            public const string ClientUpdated = "Klienten blev opdateret.";
            public const string ProjectCreated = "Projektet blev oprettet.";
            public const string ProjectUpdated = "Projektet blev opdateret.";
            public const string TransactionCreated = "Transaktionen blev oprettet.";
            public const string TransactionUpdated = "Transaktionen blev opdateret.";
            public const string CompanyInfoSaved = "Virksomhedsoplysningerne blev gemt.";
        }

        /// <summary>
        /// Confirmation messages (Danish)
        /// </summary>
        public static class ConfirmationMessages
        {
            public const string DeleteClient = "Er du sikker på, at du vil slette klienten '{0}'?\n\nDenne handling kan ikke fortrydes.";
            public const string DeleteProject = "Er du sikker på, at du vil slette projektet '{0}'?\n\nDenne handling kan ikke fortrydes.";
            public const string DeleteTransaction = "Er du sikker på, at du vil slette denne {0}?\n\nDenne handling kan ikke fortrydes.";
            public const string UnsavedChanges = "Du har ugemte ændringer. Vil du forlade siden uden at gemme?";
        }

        /// <summary>
        /// Window titles (Danish)
        /// </summary>
        public static class WindowTitles
        {
            public const string AddClient = "Tilføj ny klient";
            public const string EditClient = "Rediger klient";
            public const string AddTransaction = "Tilføj ny transaktion";
            public const string EditTransaction = "Rediger transaktion";
            public const string AddCompanyInfo = "Tilføj virksomhedsinfo";
            public const string Error = "Fejl";
            public const string Warning = "Advarsel";
            public const string CriticalError = "Kritisk fejl";
            public const string ConfirmAction = "Bekræft handling";
            public const string ConfirmDelete = "Bekræft sletning";
            public const string UnsavedChanges = "Ugemte ændringer";
            public const string Success = "Handling gennemført";
        }

        /// <summary>
        /// Invoice text templates (Danish)
        /// </summary>
        public static class InvoiceText
        {
            public const string Title = "FAKTURA";
            public const string InvoiceDate = "Fakturadato: {0}";
            public const string InvoiceNumber = "Faktura nr: {0}";
            public const string ClientTypeB2B = "Type: Erhvervskunde (B2B)";
            public const string ClientTypeB2C = "Type: Privatkunde (B2C)";
            public const string From = "Fra:";
            public const string InvoiceTo = "Faktureres til:";
            public const string Attention = "Att: {0}";
            public const string Project = "Projekt: {0}";
            public const string PaymentTerms = "Betalingsbetingelser: Netto {0} dage";
            public const string PaymentTermsDefault = "Betalingsbetingelser: Netto 14 dage";
            public const string SubtotalExclVAT = "Subtotal (ekskl. moms):";
            public const string VATAmount = "Moms ({0}%):";
            public const string TotalInclVAT = "I alt at betale (inkl. moms):";
            public const string Total = "Total:";
            public const string TotalToPay = "I alt at betale:";
            public const string PricesInclVAT = "Alle priser er inkl. moms";
            public const string PaymentInformation = "Betalingsinformation:";
            public const string BankRegNumber = "Reg.nr: {0}";
            public const string BankAccountNumber = "Konto: {0}";
            public const string ThankYou = "Tak for samarbejdet!";
            
            /// <summary>
            /// Table headers
            /// </summary>
            public static class TableHeaders
            {
                public const string Number = "Nr.";
                public const string Description = "Beskrivelse";
                public const string Quantity = "Antal";
                public const string Amount = "Beløb (DKK)";
            }
        }

        /// <summary>
        /// Test data constants
        /// </summary>
        public static class TestData
        {
            public const string TestEmail = "test@something.com";
            public const string TestPhoneNumber = "12345678";
            public const string TestAddress = "123 Test St";
            public const string TestPostalCode = "12345";
            public const string TestCity = "Test City";
            public const string TestCVR = "88888888";
            public const string TestClientName = "Test Client";
            public const string TestContactPerson = "John Doe";
            public const string TestProjectName = "Test Project";
            public const string TestProjectDescription = "This is a testproject";
        }
    }
}
