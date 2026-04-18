// ─────────────────────────────────────────────────────────────────────────────
// Connected to: LoginViewModel.cs
// Purpose: Customer login screen. Passes "Customer" as the role to the
//          shared LoginViewModel so it validates against customer credentials
//          and navigates to CustomerDashboard on success.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class CustomerLogin : Window
    {
        public CustomerLogin()
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // LoginViewModel is shared between Admin and Customer login.
            // Passing "Customer" tells it which credentials to check
            // and which dashboard to navigate to on success.
            this.DataContext = new LoginViewModel("Customer");
        }

    
    }
}