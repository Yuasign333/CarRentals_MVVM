using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class MyAccountDesignViewModel
    {
        public string UserLabel { get; } = "Customer: testuser";
        public string FullName { get; } = "Juan Dela Cruz";
        public string Username { get; } = "testuser";
        public string CustomerId { get; } = "C001";
        public string Contact { get; } = "09171234567";
        public string License { get; } = "N01-23-456789";
        public bool HasRentals { get; } = true;

        public ObservableCollection<RentalModel> Rentals { get; } = new ObservableCollection<RentalModel>
        {
            new RentalModel
            {
                RentalId   = "R0001",
                CarName    = "Toyota Camry",
                DriverName = "Juan Dela Cruz",
                Hours      = 3,
                TotalAmount= 156m,
                Status     = "Active",
                RentalDate = new DateTime(2026, 4, 18)
            },
            new RentalModel
            {
                RentalId   = "R0002",
                CarName    = "Ford Explorer",
                DriverName = "Juan Dela Cruz",
                Hours      = 5,
                TotalAmount= 487.5m,
                Status     = "Returned",
                RentalDate = new DateTime(2026, 4, 15)
            }
        };
    }
}