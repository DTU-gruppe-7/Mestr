using Mestr.Core.Model;
using Mestr.Data.Interface;
using Mestr.Services.Interface;
using Mestr.UI.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Mestr.UI.ViewModels
{
    public class AddCompanyInfoViewModel : ViewModelBase
    {
        private readonly ICompanyProfileService _companyProfileService;
        private CompanyProfile profile;

        private string _companyName = string.Empty;
        private string _contactPerson = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _address = string.Empty;
        private string _zipCode = string.Empty;
        private string _city = string.Empty;
        private string _cvr = string.Empty;
        private string _bankRegNumber = string.Empty;
        private string _bankAccountNumber = string.Empty;

        public AddCompanyInfoViewModel(ICompanyProfileService companyProfileService, CompanyProfile profile)
        {
            _companyProfileService = companyProfileService ?? throw new ArgumentNullException(nameof(companyProfileService));
            this.profile = profile ?? throw new ArgumentNullException(nameof(profile));

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            CompanyName = profile.CompanyName;
            ContactPerson = profile.ContactPerson;
            Email = profile.Email;
            PhoneNumber = profile.PhoneNumber;
            Address = profile.Address;
            ZipCode = profile.ZipCode;
            City = profile.City;
            CVR = profile.Cvr;
            BankRegNumber = profile.BankRegNumber;
            BankAccountNumber = profile.BankAccountNumber;

            
        }

        public string WindowTitle => "Tilføj virksomhedsinfo";

        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OnPropertyChanged(nameof(CompanyName));
                ValidateText(nameof(CompanyName), value, "Firmanavn");
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string ContactPerson
        {
            get => _contactPerson;
            set
            {
                _contactPerson = value;
                OnPropertyChanged(nameof(ContactPerson));
                ValidateText(nameof(ContactPerson), value, "Kontaktperson");
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ValidateEmail(nameof(Email), value);
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
                ValidatePhoneNumber(nameof(PhoneNumber), value);
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string ZipCode
        {
            get => _zipCode;
            set
            {
                _zipCode = value;
                OnPropertyChanged(nameof(ZipCode));
            }
        }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged(nameof(City));
            }
        }

        public string CVR
        {
            get => _cvr;
            set
            {
                _cvr = value;
                OnPropertyChanged(nameof(CVR));
            }
        }

        public string BankRegNumber
        {
            get => _bankRegNumber;
            set
            {
                _bankRegNumber = value;
                OnPropertyChanged(nameof(BankRegNumber));
            }
        }

        public string BankAccountNumber
        {
            get => _bankAccountNumber;
            set
            {
                _bankAccountNumber = value;
                OnPropertyChanged(nameof(BankAccountNumber));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private bool CanSave()
        {
            return !HasErrors
                   && !string.IsNullOrWhiteSpace(CompanyName)
                   && !string.IsNullOrWhiteSpace(ContactPerson)
                   && !string.IsNullOrWhiteSpace(Email)
                   && !string.IsNullOrWhiteSpace(PhoneNumber);
        }

        private void Save()
        {
            try
            {
                profile.CompanyName = CompanyName;
                profile.ContactPerson = ContactPerson;
                profile.Email = Email;
                profile.PhoneNumber = PhoneNumber;
                profile.Address = Address;
                profile.ZipCode = ZipCode;
                profile.City = City;
                profile.Cvr = CVR;
                profile.BankRegNumber = BankRegNumber;
                profile.BankAccountNumber = BankAccountNumber;

                _companyProfileService.UpdateProfile(profile);

                MessageBox.Show(
                    "Virksomhedsinfo blev gemt succesfuldt.",
                    "Gem succesfuldt",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kunne ikke gemme virksomhedsinfo. Fejl: {ex.Message}",
                    "Gem mislykkedes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult)
        {
            var window = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
            {
                window.DialogResult = dialogResult;
                window.Close();
            }
        }

        private void ValidateEmail(string propertyName, string email)
        {
            ClearErrors(propertyName);

            if (string.IsNullOrWhiteSpace(email))
            {
                AddError(propertyName, "Email må ikke være tom");
                return;
            }

            // Simple email validation
            if (!email.Contains("@") || !email.Contains("."))
            {
                AddError(propertyName, "Ugyldig email adresse");
            }
        }
        private void ValidatePhoneNumber(string propertyName, string phoneNumber)
        {
            ClearErrors(propertyName);

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                AddError(propertyName, "Telefonnummer må ikke være tomt");
                return;
            }

            // Check if phone number contains only digits
            if (!phoneNumber.All(char.IsDigit))
            {
                AddError(propertyName, "Telefonnummer må kun indeholde tal");
                return;
            }

            // Check if phone number has at least 8 digits
            if (phoneNumber.Length < 8)
            {
                AddError(propertyName, "Telefonnummer skal være mindst 8 cifre");
            }
        }
    }
}