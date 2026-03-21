// ─────────────────────────────────────────────────────────────────────────────
// Connected to: StubWindowViewModel.cs
// Purpose: Admin-only window showing a read-only table of the entire fleet.
//          Uses StubWindowViewModel which provides the Cars list
//          (loaded from CarDataService) and the BackCommand.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class FleetStatusWindow : Window
    {
        public FleetStatusWindow(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // StubWindowViewModel provides BackCommand and Cars list.
            // isAdmin: true — BackCommand navigates to AdminDashboard
            this.DataContext = new StubWindowViewModel(userId, isAdmin: true);
        }
    }
}