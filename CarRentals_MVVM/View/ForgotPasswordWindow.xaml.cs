using System.Windows;
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
    }
}