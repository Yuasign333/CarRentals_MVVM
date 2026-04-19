using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class MyAccountViewModel : ObservableObject
    {
        private readonly string _userId;

        public string UserLabel { get; }
        public string FullName { get; }
        public string Username { get; }
        public string CustomerId { get; }

        private string _contact = "Loading...";
        public string Contact
        {
            get => _contact;
            set { _contact = value; OnPropertyChanged(); }
        }

        private string _license = "Loading...";
        public string License
        {
            get => _license;
            set { _license = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RentalModel> Rentals { get; } = new();

        private bool _hasRentals = false;
        public bool HasRentals
        {
            get => _hasRentals;
            set { _hasRentals = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand BrowseCarsCommand { get; }

        public MyAccountViewModel(string userId)
        {
            _userId = userId;
            CustomerId = userId;
            FullName = !string.IsNullOrEmpty(UserSession.FullName) ? UserSession.FullName : userId;
            Username = !string.IsNullOrEmpty(UserSession.Username) ? UserSession.Username : userId;
            UserLabel = $"Customer: {Username}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerDashboard(_userId)));

            BrowseCarsCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BrowseCarsWindow(_userId)));

            // Load profile + rentals async
            Task.Run(async () =>
            {
                string queryId = !string.IsNullOrEmpty(UserSession.UserId)
                    ? UserSession.UserId : userId;

                var customer = await CarDataService.GetCustomerByUsername(UserSession.Username ?? userId);
                var rentals = await CarDataService.GetRentalsByCustomer(queryId);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (customer != null)
                    {
                        Contact = customer.ContactNumber;
                        License = customer.LicenseNumber;
                    }

                    Rentals.Clear();
                    foreach (var r in rentals) Rentals.Add(r);
                    HasRentals = Rentals.Count > 0;
                });
            });
        }
    }
}