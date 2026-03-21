// ─────────────────────────────────────────────────────────────────────────────
// Connected to: BrowseCarsViewModel.cs
// Purpose: Hosts the 3-page customer rental flow:
//   Page 1 — Browse and filter available cars
//   Page 2 — Choose a color for the selected car
//   Page 3 — Fill in booking details and confirm the rental
// Page switching, filtering, and booking logic all live in BrowseCarsViewModel.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class BrowseCarsWindow : Window
    {
        public BrowseCarsWindow(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // ViewModel handles all 3 pages of the rental flow
            this.DataContext = new BrowseCarsViewModel(userId);
        }

       
    }
}