using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for ProcessReturnWindow.xaml.
    /// Placeholder — full return processing planned for a future release.
    /// Connected to: ProcessReturnWindow.xaml (View),
    /// ProcessReturnWindow.xaml.cs (sets DataContext),
    /// AdminDashboard (navigates back here on Back).
    /// </summary>
    public class ProcessReturnViewModel : ObservableObject
    {
        // The logged-in admin's user ID
        private readonly string _userId;

        /// <summary>
        /// Label shown in the top-right badge (e.g. "Agent: A001").
        /// </summary>
        public string UserLabel { get; }

        /// <summary>
        /// Navigates back to AdminDashboard.
        /// Bound to the Back button in ProcessReturnWindow.xaml.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Initializes the Process Return ViewModel for the given admin.
        /// </summary>
        /// <param name="userId">The logged-in admin's user ID.</param>
        public ProcessReturnViewModel(string userId)
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