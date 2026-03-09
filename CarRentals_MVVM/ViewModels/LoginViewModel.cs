using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        public UserModel CurrentUser { get; set; }
        public ICommand LoginCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        private bool _errorVisible = false;
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { _errorVisible = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public LoginViewModel(string role)
        {
            CurrentUser = new UserModel { Role = role };
            LoginCommand = new RelayCommand(ExecuteLogin);
            BackCommand = new RelayCommand(ExecuteBack);
            ForgotPasswordCommand = new RelayCommand(ExecuteForgotPassword);
        }

        private void ExecuteLogin(object? parameter)
        {
            if (parameter is PasswordBox pb)
                CurrentUser.Password = pb.Password;

            bool valid = CurrentUser.Role == "Admin"
                ? CurrentUser.UserID.Trim() == "A001" && CurrentUser.Password.Trim() == "admin123"
                : CurrentUser.UserID.Trim() == "C001" && CurrentUser.Password.Trim() == "customer123";

            if (valid)
            {
                ErrorVisible = false;
                if (CurrentUser.Role == "Admin")
                {
                    var w = new View.AdminDashboard(CurrentUser.UserID);
                    NavigationService.Navigate(w);
                }
                else
                {
                    var w = new View.CustomerDashboard(CurrentUser.UserID);
                    NavigationService.Navigate(w);
                }
            }
            else
            {
                ErrorMessage = CurrentUser.Role == "Admin"
                    ? "Invalid Agent ID or password. Hint: A001 / admin123"
                    : "Invalid Customer ID or password. Hint: C001 / customer123";
                ErrorVisible = true;
            }
        }

        private void ExecuteBack(object? parameter)
            => NavigationService.Navigate(new View.ChooseRole());

        private void ExecuteForgotPassword(object? parameter)
            => MessageBox.Show("Instructions sent to your campus email.",
                               "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}