using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class RevenueDesignViewModel
    {
        public string UserLabel { get; } = "Agent: A001";
        public decimal TotalRevenue { get; } = 12450.00m;
        public int TotalRentals { get; } = 24;
        public int ActiveRentals { get; } = 6;
        public decimal AvgPerRental { get; } = 518.75m;

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