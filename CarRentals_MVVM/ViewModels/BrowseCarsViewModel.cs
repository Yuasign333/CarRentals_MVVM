using System.Collections.ObjectModel;
using System.Linq;
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
            UserLabel = $"Customer: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.CustomerDashboard(_userId)));

            SelectCarCommand = new RelayCommand(car =>
            {
                if (car is CarModel selected && selected.Status == "Available")
                    NavigationService.Navigate(new View.RentCarWindow(_userId, selected));
            });

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredCars.Clear();
            var q = CarDataService.GetAll().AsEnumerable();
            if (SelectedCategory != "All") q = q.Where(c => c.Category == SelectedCategory);
            if (SelectedFuel != "All") q = q.Where(c => c.FuelType == SelectedFuel);
            foreach (var c in q) FilteredCars.Add(c);
        }
    }
}