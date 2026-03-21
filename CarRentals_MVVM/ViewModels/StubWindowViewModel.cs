using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// Shared ViewModel used by multiple admin stub windows:
    /// FleetStatusWindow, MaintenanceWindow, ProcessReturnWindow, RevenueWindow.
    /// Provides the user label, back navigation, and the full car list
    /// for windows that display fleet data.
    /// Connected to: FleetStatusWindow.xaml.cs, MaintenanceWindow.xaml.cs,
    /// ProcessReturnWindow.xaml.cs, RevenueWindow.xaml.cs (all set DataContext to this),
    /// CarDataService (reads all cars for FleetStatusWindow display).
    /// </summary>
    public class StubWindowViewModel : ObservableObject
    {
        // The logged-in user's ID — used when navigating back
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right of the window.
        /// Displays "Agent: {userId}" for admin windows.
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// Navigates back to the appropriate dashboard based on role.
        /// Bound to the Back button in each stub window's XAML.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// The full list of cars from CarDataService.
        /// Used by FleetStatusWindow.xaml to display the fleet table.
        /// Loaded once on initialization — reflects the live data at time of opening.
        /// </summary>
        public ObservableCollection<CarModel> Cars { get; } = new();

        /// <summary>
        /// Initializes the stub ViewModel for the given user and role.
        /// </summary>
        /// <param name="userId">The ID of the currently logged-in user.</param>
        /// <param name="isAdmin">
        /// True if the user is an Admin — navigates back to AdminDashboard.
        /// False if Customer — navigates back to CustomerDashboard.
        /// </param>
        public StubWindowViewModel(string userId, bool isAdmin)
        {
            _userId = userId;
            UserLabel = isAdmin
                ? $"Agent: {userId}"
                : $"Customer: {userId}";

            // Navigate back to the correct dashboard based on user role
            BackCommand = new RelayCommand(_ =>
            {
                if (isAdmin)
                {
                    NavigationService.Navigate(new View.AdminDashboard(_userId));
                }
                else
                {
                    NavigationService.Navigate(new View.CustomerDashboard(_userId));
                }
            });

            // Load all cars from the data service into the observable collection
            // This powers the fleet table in FleetStatusWindow.xaml
            foreach (var car in CarDataService.GetAll())
            {
                Cars.Add(car);
            }
        }
    }
}