
// ─────────────────────────────────────────────────────────────────────────────
// FILE: MaintenanceWindow.xaml.cs
// Connected to: MaintenanceViewModel.cs
// Purpose: Admin maintenance placeholder window.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class MaintenanceWindow : Window
    {
        public MaintenanceWindow(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // Dedicated ViewModel for this window
            this.DataContext = new MaintenanceViewModel(userId);
        }
    }
}
