// ─────────────────────────────────────────────────────────────────────────────
// FILE: FleetStatusWindow.xaml.cs
// Connected to: FleetStatusViewModel.cs
// Purpose: Admin fleet table window. Sets DataContext to its own
//          dedicated FleetStatusViewModel instead of shared StubWindowViewModel.
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

            // Dedicated ViewModel for this window
            this.DataContext = new FleetStatusViewModel(userId);
        }
    }
}
