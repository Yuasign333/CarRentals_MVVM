using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class ProcessReturnDesignViewModel
    {
        public string UserLabel { get; } = "Agent: A001";

        public RentalModel? SelectedRental { get; set; } = new RentalModel
        {
            RentalId = "R0001",
            CarName = "Toyota Camry",
            CustomerId = "C001",
            DriverName = "Juan Dela Cruz",
            Hours = 3,
            TotalAmount = 156m,
            Status = "Active"
        };

        public ObservableCollection<RentalModel> ActiveRentals { get; } = new ObservableCollection<RentalModel>
        {
            new RentalModel
            {
                RentalId    = "R0001",
                CarName     = "Toyota Camry",
                CustomerId  = "C001",
                DriverName  = "Juan Dela Cruz",
                Hours       = 3,
                TotalAmount = 156m,
                Status      = "Active"
            },
            new RentalModel
            {
                RentalId    = "R0002",
                CarName     = "Ford Explorer",
                CustomerId  = "C001",
                DriverName  = "Maria Santos",
                Hours       = 5,
                TotalAmount = 487.5m,
                Status      = "Active"
            }
        };
    }
}