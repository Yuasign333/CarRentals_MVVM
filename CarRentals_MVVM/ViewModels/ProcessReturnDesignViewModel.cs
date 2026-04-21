using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// Design-time only ViewModel for ProcessReturnWindow.xaml.
    /// Provides fake active rental data so the XAML designer shows a preview
    /// of the rental table and selected rental panel.
    /// NEVER used at runtime — only in the XAML designer via d:DataContext.
    /// Connected to: ProcessReturnWindow.xaml (via d:DataContext).
    /// </summary>
    public class ProcessReturnDesignViewModel
    {
        /// <summary>Fake user label shown in the top-right badge in the designer.</summary>
        public string UserLabel { get; } = "Agent: A001";

        /// <summary>
        /// Fake selected rental shown in the return detail panel in the designer.
        /// </summary>
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

        /// <summary>
        /// Fake list of active rentals shown in the table in the designer.
        /// </summary>
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