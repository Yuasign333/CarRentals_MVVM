using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class SignUpWindow : Window
    {
        public SignUpWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new SignUpViewModel();
        }

        // PasswordBox cannot bind directly — extract here and push to ViewModel
        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SignUpViewModel vm)
            {
                vm.Password = PwBox.Password;
                vm.ConfirmPass = ConfirmPwBox.Password;
                vm.RegisterCommand.Execute(null);
            }
        }
    }
}