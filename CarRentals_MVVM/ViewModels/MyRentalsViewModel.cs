using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;
using System.Windows;

namespace CarRentals_MVVM.ViewModels
{
    public class MyRentalsViewModel : ObservableObject
    {
        private readonly string _userId;

        public string UserLabel { get; }

        public ObservableCollection<RentalModel> Rentals { get; } = new();

        // ← Must be a full property with OnPropertyChanged, NOT computed
        private bool _hasRentals = false;
        public bool HasRentals
        {
            get => _hasRentals;
            set { _hasRentals = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }

        public MyRentalsViewModel(string userId)
        {
            _userId = userId;

            // Show username from session if available
            string displayName = !string.IsNullOrEmpty(UserSession.Username)
                ? UserSession.Username : userId;
            UserLabel = $"Customer: {displayName}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerDashboard(_userId)));

            Task.Run(async () =>
            {
                // Always query by the actual CustomerID (C001 etc), not username
                string queryId = !string.IsNullOrEmpty(UserSession.UserId)
                    ? UserSession.UserId : userId;

                var rentals = await CarDataService.GetRentalsByCustomer(queryId);

            
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Rentals.Clear();
                    foreach (var r in rentals) Rentals.Add(r);
                    HasRentals = Rentals.Count > 0; // ← triggers OnPropertyChanged
                });
            });
        }
    }
}