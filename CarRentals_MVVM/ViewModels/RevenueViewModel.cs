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
    /// <summary>
    /// ViewModel for the Admin Revenue Dashboard.
    /// Calculates business performance metrics by aggregating rental income 
    /// and deducting maintenance costs to find the true Net Revenue.
    /// </summary>
    public class RevenueViewModel : ObservableObject
    {
        private readonly string _userId;

        /// <summary>Displays the Admin/Agent ID in the dashboard header.</summary>
        public string UserLabel { get; }

        /// <summary>Full history of all rentals (Active, Returned, Late, etc.) fetched from the DB.</summary>
        public ObservableCollection<RentalModel> AllRentals { get; } = new();

        private decimal _totalRevenue;
        /// <summary>The Net Revenue (Rental Income minus Maintenance Expenses).</summary>
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        private decimal _avgPerRental;
        /// <summary>The average profit generated per rental transaction.</summary>
        public decimal AvgPerRental
        {
            get => _avgPerRental;
            set { _avgPerRental = value; OnPropertyChanged(); }
        }

        private int _totalRentals;
        /// <summary>Count of all rental records in the database.</summary>
        public int TotalRentals
        {
            get => _totalRentals;
            set { _totalRentals = value; OnPropertyChanged(); }
        }

        private int _activeRentals;
        /// <summary>Count of cars currently out with customers (Status = 'Active').</summary>
        public int ActiveRentals
        {
            get => _activeRentals;
            set { _activeRentals = value; OnPropertyChanged(); }
        }

        /// <summary>Command to return to the Admin Dashboard main menu.</summary>
        public ICommand BackCommand { get; }

        /// <summary>Command to trigger the generation of a physical text/PDF report of the revenue data.</summary>
        public ICommand ExportReportCommand { get; }

        public RevenueViewModel(string userId)
        {
            _userId = userId;
            UserLabel = $"Agent: {userId}";

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.AdminDashboard(_userId)));

            ExportReportCommand = new RelayCommand(_ =>
            {
                // Calls Service to save data to a local folder
                CarDataService.GenerateRevenueReport(
                    AllRentals.ToList(), TotalRevenue, AvgPerRental);

                MessageBox.Show("Revenue report saved to Admin/RevenueReports folder.",
                    "Exported", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            // Initialize data loading in a background thread to prevent UI freezing
            Task.Run(async () =>
            {
                // 1. Fetch both Income (Rentals) and Expenses (Maintenance)
                var rentals = await CarDataService.GetAllRentals();
                var maintenanceRecords = await CarDataService.GetAllMaintenance();

                // 2. Switch back to UI thread to update ObservableCollections and Properties
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllRentals.Clear();
                    foreach (var r in rentals) AllRentals.Add(r);

                    // 3. Calculate basic counts
                    TotalRentals = AllRentals.Count;
                    ActiveRentals = AllRentals.Count(r => r.Status == "Active");

                    // 4. FINANCIAL CALCULATIONS:
                    // Gross Revenue: Total money collected from customers
                    decimal grossRevenue = AllRentals.Sum(r => r.TotalAmount);

                    // Total Maintenance: Total money paid to technicians/parts
                    decimal totalMaintenanceCost = maintenanceRecords.Sum(m => m.Cost);

                    // Net Revenue: Gross income minus the cost of keeping cars running
                    // Note: If maintenance were billed to customers, this would be a '+' sign instead.
                    TotalRevenue = grossRevenue - totalMaintenanceCost;

                    // 5. Calculate Average Profitability
                    AvgPerRental = TotalRentals > 0
                        ? TotalRevenue / TotalRentals : 0;
                });
            });
        }
    }
}