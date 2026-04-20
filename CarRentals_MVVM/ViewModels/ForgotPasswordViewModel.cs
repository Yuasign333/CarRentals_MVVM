using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class ForgotPasswordViewModel : ObservableObject
    {
        private readonly string _role;

        private string _username = string.Empty;
        private string _securityQuestion = string.Empty;
        private string _securityAnswer = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _errorVisible = false;
        private bool _isStep2 = false;
        private string _customerId = string.Empty;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
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
        public string NewPassword
        {
            get => _newPassword;
            set 
            {

                    _newPassword = value; OnPropertyChanged(); 
            }
        }
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set 
            {
               
               
                _confirmPassword = value; OnPropertyChanged(); 
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
        public bool IsStep2
        {
            get => _isStep2;
            set
            {
                _isStep2 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStep1));
            }
        }
        public bool IsStep1 => !IsStep2;

        public ICommand FindAccountCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand BackCommand { get; }

        public ForgotPasswordViewModel(string role)
        {
            _role = role;

            BackCommand = new RelayCommand(_ =>
            {
                if (_role == "Admin")
                    NavigationService.Navigate(new View.AdminLogin());
                else
                    NavigationService.Navigate(new View.CustomerLogin());
            });

            FindAccountCommand = new RelayCommand(async _ =>
            {
                ErrorVisible = false;

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Please enter your username.";
                    ErrorVisible = true;
                    return;
                }

                var (found, question, customerId) =
                    await CarDataService.GetSecurityQuestion(Username);

                if (!found)
                {
                    ErrorMessage = "Username not found.";
                    ErrorVisible = true;
                    return;
                }

                _customerId = customerId;
                SecurityQuestion = question;
                IsStep2 = true;
            });

            ResetPasswordCommand = new RelayCommand(async _ =>
            {
                ErrorVisible = false;

                if (string.IsNullOrWhiteSpace(SecurityAnswer))
                {
                    ErrorMessage = "Please enter your security answer.";
                    ErrorVisible = true;
                    return;
                }
                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    ErrorMessage = "Please enter a new password.";
                    ErrorVisible = true;
                    return;
                }
                if (NewPassword.Length < 6)
                {
                    ErrorMessage = $"Password too short ({NewPassword.Length} chars). Minimum is 6 characters.";
                    ErrorVisible = true;
                    return;
                }
                if (NewPassword.Contains(" "))
                {
                    ErrorMessage = "Password cannot contain spaces.";
                    ErrorVisible = true;
                    return;
                }
                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    ErrorMessage = "Please confirm your new password.";
                    ErrorVisible = true;
                    return;
                }
                if (NewPassword != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    ErrorVisible = true;
                    return;
                }

                var (success, message) = await CarDataService.ResetPassword(
                    Username, SecurityAnswer, NewPassword);

                if (success)
                {
                    MessageBox.Show(
                        "Password reset successfully! You can now log in.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (_role == "Admin")
                        NavigationService.Navigate(new View.AdminLogin());
                    else
                        NavigationService.Navigate(new View.CustomerLogin());
                }
                else
                {
                    ErrorMessage = message;
                    ErrorVisible = true;
                }
            });
        }
    }
}