using System;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class SignUpViewModel : ObservableObject
    {
        private string _fullName = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPass = string.Empty;
        private string _contact = string.Empty;
        private string _license = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _errorVisible = false;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }
        public string ConfirmPass
        {
            get => _confirmPass;
            set { _confirmPass = value; OnPropertyChanged(); }
        }
        public string Contact
        {
            get => _contact;
            set { _contact = value; OnPropertyChanged(); }
        }
        public string License
        {
            get => _license;
            set { _license = value; OnPropertyChanged(); }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { _errorVisible = value; OnPropertyChanged(); }
        }

        private string _securityQuestion = "What is your pet name?";
        public string SecurityQuestion
        {
            get => _securityQuestion;
            set { _securityQuestion = value; OnPropertyChanged(); }
        }

        private string _securityAnswer = string.Empty;
        public string SecurityAnswer
        {
            get => _securityAnswer;
            set { _securityAnswer = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        public SignUpViewModel()
        {
            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerLogin()));

            RegisterCommand = new RelayCommand(async _ =>
            {
                ErrorVisible = false;

                // Validation
                if (string.IsNullOrWhiteSpace(FullName) ||
                    string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(Contact) ||
                    string.IsNullOrWhiteSpace(License))
                {
                    ErrorMessage = "All fields are required.";
                    ErrorVisible = true;
                    return;
                }
                if (Password != ConfirmPass)
                {
                    ErrorMessage = "Passwords do not match.";
                    ErrorVisible = true;
                    return;
                }
                if (Password.Contains(" ") || Username.Contains(" "))
                {
                    ErrorMessage = "Username and password cannot contain spaces.";
                    ErrorVisible = true;
                    return;
                }
                if (await CarDataService.UsernameExists(Username))
                {
                    ErrorMessage = "That username is already taken.";
                    ErrorVisible = true;
                    return;
                }
                if (string.IsNullOrWhiteSpace(SecurityAnswer))
                {
                    ErrorMessage = "Please provide a security answer.";
                    ErrorVisible = true;
                    return;
                }

                try
                {
                    string newId = await CarDataService.GetNextCustomerId();
                    var customer = new CustomerModel
                    {
                        CustomerId = newId,
                        FullName = FullName,
                        Username = Username,
                        Password = Password,
                        ContactNumber = Contact,
                        LicenseNumber = License,
                        SecurityQuestion = SecurityQuestion,
                        SecurityAnswer = SecurityAnswer
                    };
                    await CarDataService.RegisterCustomer(customer);

                    MessageBox.Show(
                        $"Account created!\nYour Customer ID is: {newId}\nUse it to log in.",
                        "Registration Successful",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService.Navigate(new View.CustomerLogin());
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Registration failed: " + ex.Message;
                    ErrorVisible = true;
                }
            });
        }
    }
}