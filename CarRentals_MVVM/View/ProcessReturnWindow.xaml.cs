// ─────────────────────────────────────────────────────────────────────────────
// FILE: ProcessReturnWindow.xaml.cs
// Connected to: ProcessReturnViewModel.cs
// Purpose: Admin process return placeholder window.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class ProcessReturnWindow : Window
    {
        public ProcessReturnWindow(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // Dedicated ViewModel for this window
            this.DataContext = new ProcessReturnViewModel(userId);
        }
    }
}
