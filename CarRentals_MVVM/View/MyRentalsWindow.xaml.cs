// ─────────────────────────────────────────────────────────────────────────────
// Connected to: MyRentalsViewModel.cs
// Purpose: Displays the logged-in customer's rental history.
//          The ViewModel loads rentals filtered by userId from CarDataService.
//          Navigated to after a successful booking in BrowseCarsViewModel.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class MyRentalsWindow : Window
    {
        public MyRentalsWindow(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // ViewModel loads only the rentals that belong to this customer
            this.DataContext = new MyRentalsViewModel(userId);
        }
    }
}