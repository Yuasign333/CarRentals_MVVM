// ─────────────────────────────────────────────────────────────────────────────
// Connected to: CustomerDashboardViewModel.cs
// Purpose: Customer main hub. Constructs a UserModel with the Customer role
//          and passes it to CustomerDashboardViewModel which handles
//          sidebar toggle, welcome text, and all navigation commands.
// All card and sidebar interactions use Command bindings in the XAML —
// zero click handlers here.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class CustomerDashboard : Window
    {
        public CustomerDashboard(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // Build a UserModel and pass it to the ViewModel.
            // The ViewModel uses UserID for display labels and navigation,
            // and Role to identify this session as a Customer session.
            this.DataContext = new CustomerDashboardViewModel(
                new Models.UserModel { UserID = userId, Role = "Customer" }
            );
        }
    }
}