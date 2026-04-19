using System;
using System.Linq;
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
        private string _securityQuestion = "What is your pet's name?";
        private string _securityAnswer = string.Empty;

        public string FullName
        {
            get => _fullName;
            set
            {
                // Names must be letters and spaces only
                if (!string.IsNullOrEmpty(value) &&
                    value.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
                {
                    MessageBox.Show(
                        "Full Name must contain letters and spaces only.\n\nExample: 'Juan Dela Cruz'\nNumbers and symbols are not allowed.",
                        "Invalid Full Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _fullName = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Contains(" "))
                {
                    MessageBox.Show(
                        "Username cannot contain spaces.\n\nExample: 'juandc' or 'juan123'\nUse letters and numbers only, no spaces.",
                        "Invalid Username", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
               
                if (!string.IsNullOrEmpty(value) && value.Length < 6)
                {
                    ErrorMessage = $"Password too short ({value.Length}/6 chars minimum).";
                    ErrorVisible = true;
                }
                else
                {
                    ErrorVisible = false;
                }
            }
        }
        public string ConfirmPass
        {
            get => _confirmPass;
            set
            {
                _confirmPass = value;
                OnPropertyChanged();
                // Live check: show inline error if they don't match
                if (!string.IsNullOrEmpty(value) && value != _password)
                {
                    ErrorMessage = "Passwords do not match yet.";
                    ErrorVisible = true;
                }
                else if (!string.IsNullOrEmpty(value) && value == _password)
                {
                    ErrorVisible = false;
                }
            }
        }

        public string Contact
        {
            get => _contact;
            set
            {
                // 1. If it's empty, just accept it (so they can delete/clear the box)
                if (string.IsNullOrEmpty(value))
                {
                    _contact = value;
                    OnPropertyChanged();
                    return;
                }

                // 2. Check ONLY for invalid characters while they type
                if (value.Any(c => !char.IsDigit(c) && c != '+' && c != '-' && c != ' ') || value.Length == 12)
                {
                    MessageBox.Show(
                        "Contact Number should contain 11 digits only.\n\nExample: '09171234567' or '+639171234567'",
                        "Invalid Character", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Reject the invalid character, but let them keep typing valid ones
                }

                // 3. Accept the valid keystroke
                _contact = value;
                OnPropertyChanged();
            }
        }

        public string License
        {
            get => _license;
            set
            {
                _license = value;
                OnPropertyChanged();
                // Only show inline hint, never block or MessageBox on every keystroke
                if (!string.IsNullOrEmpty(value) && value.Length < 4)
                {
                    ErrorMessage = "License Number: e.g. 'N01-23-456789' or 'LIC-0001' (min 4 chars)";
                    ErrorVisible = true;
                }
                else if (!string.IsNullOrEmpty(value) && value.Length >= 4)
                {
                    ErrorVisible = false;
                }
            }
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

        public string SecurityQuestion
        {
            get => _securityQuestion;
            set { _securityQuestion = value; OnPropertyChanged(); }
        }

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

                // Final validation before submit
                if (string.IsNullOrWhiteSpace(FullName) ||
                    string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(Contact) ||
                    string.IsNullOrWhiteSpace(License))
                {
                    ErrorMessage = "All fields are required. Please fill in every field.";
                    ErrorVisible = true;
                    return;
                }
                if (Password.Length < 6)
                {
                    ErrorMessage = $"Password must be at least 6 characters (you entered {Password.Length}).";
                    ErrorVisible = true;
                    return;
                }
                if (Password != ConfirmPass)
                {
                    ErrorMessage = "Passwords do not match. Please re-enter them.";
                    ErrorVisible = true;
                    return;
                }
                if (string.IsNullOrWhiteSpace(SecurityAnswer))
                {
                    ErrorMessage = "Please provide a security answer for account recovery.";
                    ErrorVisible = true;
                    return;
                }
                if (await CarDataService.UsernameExists(Username))
                {
                    ErrorMessage = $"The username '{Username}' is already taken. Please choose another.";
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
                        $"Account created successfully!\n\nYour Customer ID: {newId}\nYour Username: {Username}\n\nUse your username to log in.",
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