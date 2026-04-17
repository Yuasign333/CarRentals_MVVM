// ─────────────────────────────────────────────────────────────────────────────
// Connected to: AddCarViewModel.cs
// Purpose: Admin car management screen.
//   Left side — table showing all cars in the fleet (CarList)
//   Right side — form to add a new car or view details of a selected car
// All Save, Delete, Clear, and Back interactions use Command bindings
// in AddCarWindow.xaml — zero click handlers here.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class AddCarWindow : Window
    {
        public AddCarWindow(string userId)
        {
            InitializeComponent();

            // Register this window as the current active window for NavigationService
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // ViewModel handles all car CRUD operations and form state
            this.DataContext = new AddCarViewModel(userId);
        }

    
    }
}