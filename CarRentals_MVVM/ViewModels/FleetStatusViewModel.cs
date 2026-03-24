using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for FleetStatusWindow.xaml.
    /// Displays the full fleet table for the admin.
    /// Connected to: FleetStatusWindow.xaml (View),
    /// FleetStatusWindow.xaml.cs (sets DataContext),
    /// CarDataService (reads all cars),
    /// AdminDashboard (navigates back here on Back).
    /// </summary>
    public class FleetStatusViewModel : ObservableObject
    {
        // The logged-in admin's user ID
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right badge (e.g. "Agent: A001").
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// Full list of all cars loaded from CarDataService.
        /// Bound to the fleet table ListView in FleetStatusWindow.xaml.
        /// </summary>
        public ObservableCollection<CarModel> Cars { get; } = new();

        /// <summary>
        /// Navigates back to AdminDashboard.
        /// Bound to the Back button in FleetStatusWindow.xaml.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the Fleet Status ViewModel for the given admin.
        /// </summary>
        /// <param name="userId">The logged-in admin's user ID.</param>
        public FleetStatusViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            // Navigate back to the admin dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.AdminDashboard(_userId));
            });

            // Load all cars from the data service into the observable collection
            foreach (var car in CarDataService.GetAll())
            {
                Cars.Add(car);
            }

        }
        }
    }
