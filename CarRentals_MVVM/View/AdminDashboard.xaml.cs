// ─────────────────────────────────────────────────────────────────────────────
// Connected to: AdminDashboardViewModel.cs
// Purpose: Admin main hub. Constructs a UserModel with the Admin role
//          and passes it to AdminDashboardViewModel which handles
//          sidebar toggle, welcome text, and all navigation commands.
// All card and sidebar interactions use Command bindings in the XAML —
// zero click handlers here.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // Build a UserModel and pass it to the ViewModel.
            // The ViewModel uses UserID for display labels and navigation,
            // and Role to identify this session as an Admin session.
            this.DataContext = new AdminDashboardViewModel(
                new Models.UserModel { UserID = userId, Role = "Admin" }
            );
        }
    }
}