using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// Design-time only ViewModel for RevenueWindow.xaml (or RevenueView).
    /// This provides static, hardcoded financial data so that the Visual Studio/Rider 
    /// XAML designer can render charts, labels, and tables for preview purposes.
    /// NEVER used during actual application execution.
    /// </summary>
    public class RevenueDesignViewModel
    {
        /// <summary>Sample agent ID shown in the header of the design preview.</summary>
        public string UserLabel { get; } = "Agent: A001";

        /// <summary>Total net money earned, shown in the main 'Total Revenue' KPI card.</summary>
        public decimal TotalRevenue { get; } = 12450.00m;

        /// <summary>Total count of all rental transactions in the system history.</summary>
        public int TotalRentals { get; } = 24;

        /// <summary>Count of cars currently out on the road (not yet returned).</summary>
        public int ActiveRentals { get; } = 6;

        /// <summary>Calculated metric showing the average ticket price per rental session.</summary>
        public decimal AvgPerRental { get; } = 518.75m;

        /// <summary>
        /// A collection of sample rental data. 
        /// Populates the DataGrid or list in the designer to test layout, 
        /// column widths, and currency formatting.
        /// </summary>
        public ObservableCollection<RentalModel> AllRentals { get; } = new ObservableCollection<RentalModel>
        {
            new RentalModel
            {
                RentalId    = "R0001",
                CarName     = "Toyota Camry",
                CustomerId  = "C001",
                Hours       = 3,
                TotalAmount = 156m,
                Status      = "Active",
                RentalDate  = new DateTime(2026, 4, 18)
            },
            new RentalModel
            {
                RentalId    = "R0002",
                CarName     = "Ford Explorer",
                CustomerId  = "C002",
                Hours       = 5,
                TotalAmount = 487.5m,
                Status      = "Returned",
                RentalDate  = new DateTime(2026, 4, 15)
            }
        };
    }
}