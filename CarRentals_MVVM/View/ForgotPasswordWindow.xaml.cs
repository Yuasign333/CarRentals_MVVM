using System.Windows;
using System.Windows.Controls;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow(string role)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new ForgotPasswordViewModel(role);
        }

        // Extract PasswordBox values and push to ViewModel before executing command
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ForgotPasswordViewModel vm)
            {
                vm.NewPassword = NewPwBox.Password;
                vm.ConfirmPassword = ConfirmPwBox.Password;
                vm.ResetPasswordCommand.Execute(null);
            }
        }

      
    }
}