using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Commands;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.ViewModels
{
    public class RevenueViewModel : ObservableObject
    {
        private readonly string _userId;
        public string UserLabel { get; }

        public ObservableCollection<RentalModel> AllRentals { get; } = new();

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        private decimal _avgPerRental;
        public decimal AvgPerRental
        {
            get => _avgPerRental;
            set { _avgPerRental = value; OnPropertyChanged(); }
        }

        private int _totalRentals;
        public int TotalRentals
        {
            get => _totalRentals;
            set { _totalRentals = value; OnPropertyChanged(); }
        }

        private int _activeRentals;
        public int ActiveRentals
        {
            get => _activeRentals;
            set { _activeRentals = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand ExportReportCommand { get; }

        public RevenueViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_userId)));

            ExportReportCommand = new RelayCommand(_ =>
            {
                CarDataService.GenerateRevenueReport(
                    AllRentals.ToList(), TotalRevenue, AvgPerRental);
                MessageBox.Show("Revenue report saved to Admin/RevenueReports folder.",
                    "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            Task.Run(async () =>
            {
                var rentals = await CarDataService.GetAllRentals();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllRentals.Clear();
                    foreach (var r in rentals) AllRentals.Add(r);

                    TotalRentals = AllRentals.Count;
                    ActiveRentals = AllRentals.Count(r => r.Status == "Active");
                    TotalRevenue = AllRentals.Sum(r => r.TotalAmount);
                    AvgPerRental = TotalRentals > 0
                        ? TotalRevenue / TotalRentals : 0;
                });
            });
        }
    }
}