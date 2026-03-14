using System.Collections.ObjectModel;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class MyRentalsViewModel : ObservableObject
    {
        private readonly string _userId;
        public string UserLabel { get; }
        public ObservableCollection<RentalModel> Rentals { get; } = new();
        public bool HasRentals => Rentals.Count > 0;
        public ICommand BackCommand { get; }

        public MyRentalsViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Customer: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerDashboard(_userId)));

            foreach (var r in CarDataService.GetByCustomer(userId))
                Rentals.Add(r);
        }
    }
}