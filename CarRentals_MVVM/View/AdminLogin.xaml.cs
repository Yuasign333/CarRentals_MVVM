// ─────────────────────────────────────────────────────────────────────────────
// Connected to: LoginViewModel.cs
// Purpose: Admin login screen. Passes "Admin" as the role to the
//          shared LoginViewModel so it validates against admin credentials
//          and navigates to AdminDashboard on success.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class AdminLogin : Window
    {
        public AdminLogin()
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // LoginViewModel is shared between Admin and Customer login.
            // Passing "Admin" tells it which credentials to check
            // and which dashboard to navigate to on success.
            this.DataContext = new LoginViewModel("Admin");
        }
    }
}