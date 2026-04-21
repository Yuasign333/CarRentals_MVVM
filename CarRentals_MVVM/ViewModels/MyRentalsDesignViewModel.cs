// ─────────────────────────────────────────────────────────────────────────────
// FILE: MyRentalsDesignViewModel.cs
// ─────────────────────────────────────────────────────────────────────────────
// Design-time only ViewModel for MyRentalsWindow.xaml.
// Provides fake rental history so the XAML designer shows a preview.
// NEVER used at runtime — only in the XAML designer via d:DataContext.
// Connected to: MyRentalsWindow.xaml (via d:DataContext).
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class MyRentalsDesignViewModel
    {
        /// <summary>Fake user label shown in the top-right badge in the designer.</summary>
        public string UserLabel { get; } = "Customer: testuser";

        /// <summary>Fake full name shown in the designer.</summary>
        public string FullName { get; } = "Juan Dela Cruz";

        /// <summary>Fake username shown in the designer.</summary>
        public string Username { get; } = "testuser";

        /// <summary>
        /// Always true so the rental table is visible in the designer.
        /// </summary>
        public bool HasRentals { get; } = true;

        /// <summary>
        /// Fake rental records shown in the table in the designer.
        /// Includes Active and Returned statuses so all badge colors can be previewed.
        /// </summary>
        public ObservableCollection<RentalModel> Rentals { get; } = new ObservableCollection<RentalModel>
        {
            new RentalModel
            {
                RentalId    = "R0001",
                CarName     = "Toyota Camry",
                CarId       = "C001",
                DriverName  = "Juan Dela Cruz",
                Color       = "White",
                Hours       = 3,
                TotalAmount = 156m,
                Status      = "Active",
                RentalDate  = new DateTime(2026, 4, 18)
            },
            new RentalModel
            {
                RentalId    = "R0002",
                CarName     = "Ford Explorer",
                CarId       = "C002",
                DriverName  = "Juan Dela Cruz",
                Color       = "Gray",
                Hours       = 5,
                TotalAmount = 487.5m,
                Status      = "Returned",
                RentalDate  = new DateTime(2026, 4, 15)
            }
        };
    }
}