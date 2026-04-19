using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class BrowseCarsViewModel : ObservableObject
    {
        private readonly string _userId;
        public string UserLabel { get; }

        private string _selectedCategory = "All";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _selectedFuel = "All";
        public string SelectedFuel
        {
            get => _selectedFuel;
            set { _selectedFuel = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ObservableCollection<CarModel> FilteredCars { get; } = new();

        public ICommand BackCommand { get; }
        public ICommand SelectCarCommand { get; }

        public BrowseCarsViewModel(string userId)
        {
            _userId = userId;
            UserLabel = !string.IsNullOrEmpty(UserSession.Username)
       ? $"Customer: {UserSession.Username}"
       : $"Customer: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerDashboard(_userId)));

            // When customer clicks a car — open RentCarWindow
            SelectCarCommand = new RelayCommand(car =>
            {
                if (car is CarModel selected && selected.Status == "Available")
                {
                    NavigationService.Navigate(new View.RentCarWindow(_userId, selected));
                }
                else if (car is CarModel unavailable)
                {
                    MessageBox.Show(
                        $"This car is currently {unavailable.Status} and cannot be rented.",
                        "Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            ApplyFilter();
        }

        private async void ApplyFilter()
        {
            FilteredCars.Clear();
            var allCars = await CarDataService.GetAll();
            var query = allCars.Where(c => c.Status == "Available");

            if (SelectedCategory != "All")
                query = query.Where(c => c.Category == SelectedCategory);
            if (SelectedFuel != "All")
                query = query.Where(c => c.FuelType == SelectedFuel);

            foreach (var car in query)
                FilteredCars.Add(car);
        }

    }
}