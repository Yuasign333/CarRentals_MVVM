// ─────────────────────────────────────────────────────────────────────────────
// Connected to: StubWindowViewModel.cs
// Purpose: Placeholder for the Revenue and Analytics feature.
//          Full implementation (earnings reports, charts) is planned
//          for a future release. Currently shows a "coming soon" card.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class RevenueWindow : Window
    {
        public RevenueWindow(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // StubWindowViewModel provides BackCommand.
            // isAdmin: true — BackCommand navigates to AdminDashboard
            this.DataContext = new StubWindowViewModel(userId, isAdmin: true);
        }
    }
}