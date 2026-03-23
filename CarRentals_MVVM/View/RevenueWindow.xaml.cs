// ─────────────────────────────────────────────────────────────────────────────
// FILE: RevenueWindow.xaml.cs
// Connected to: RevenueViewModel.cs
// Purpose: Admin revenue placeholder window.
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

            // Dedicated ViewModel for this window
            this.DataContext = new RevenueViewModel(userId);
        }
    }
}