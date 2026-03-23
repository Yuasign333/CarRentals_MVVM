using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for MaintenanceWindow.xaml.
    /// Placeholder — full maintenance features planned for a future release.
    /// Connected to: MaintenanceWindow.xaml (View),
    /// MaintenanceWindow.xaml.cs (sets DataContext),
    /// AdminDashboard (navigates back here on Back).
    /// </summary>
    public class MaintenanceViewModel : ObservableObject
    {
        // The logged-in admin's user ID
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right badge (e.g. "Agent: A001").
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// Navigates back to AdminDashboard.
        /// Bound to the Back button in MaintenanceWindow.xaml.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the Maintenance ViewModel for the given admin.
        /// </summary>
        /// <param name="userId">The logged-in admin's user ID.</param>
        public MaintenanceViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            // Navigate back to the admin dashboard
            BackCommand = new RelayCommand(_ =>
            {
                NavigationService.Navigate(new View.AdminDashboard(_userId));
            });
        }
    }
}