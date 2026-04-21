// ─────────────────────────────────────────────────────────────────────────────
// FILE: MyAccountDesignViewModel.cs
// ─────────────────────────────────────────────────────────────────────────────
// Design-time only ViewModel for MyAccountWindow.xaml.
// Provides fake customer data and rental history so the XAML designer
// shows a preview without running the application.
// NEVER used at runtime — only in the XAML designer via d:DataContext.
// Connected to: MyAccountWindow.xaml (via d:DataContext).
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.ObjectModel;
using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class MyAccountDesignViewModel
    {
        /// <summary>Fake user label shown in the top-right badge in the designer.</summary>
        public string UserLabel { get; } = "Customer: testuser";

        /// <summary>Fake full name shown in the profile card in the designer.</summary>
        public string FullName { get; } = "Juan Dela Cruz";

        /// <summary>Fake username shown in the profile card in the designer.</summary>
        public string Username { get; } = "testuser";

        /// <summary>Fake customer ID shown in the profile card in the designer.</summary>
        public string CustomerId { get; } = "C001";

        /// <summary>Fake contact number shown in the profile card in the designer.</summary>
        public string Contact { get; } = "09171234567";

        /// <summary>Fake license number shown in the profile card in the designer.</summary>
        public string License { get; } = "N01-23-456789";

        /// <summary>
        /// Always true in the designer so the rental history table is always visible.
        /// </summary>
        public bool HasRentals { get; } = true;

        /// <summary>
        /// Fake rental history shown in the right panel of MyAccountWindow in the designer.
        /// Includes both Active and Returned statuses so both badge colors can be previewed.
        /// </summary>
        public ObservableCollection<RentalModel> Rentals { get; } = new ObservableCollection<RentalModel>
        {
            new RentalModel
            {
                RentalId    = "R0001",
                CarName     = "Toyota Camry",
                DriverName  = "Juan Dela Cruz",
                Hours       = 3,
                TotalAmount = 156m,
                Status      = "Active",
                RentalDate  = new DateTime(2026, 4, 18)
            },
            new RentalModel
            {
                RentalId    = "R0002",
                CarName     = "Ford Explorer",
                DriverName  = "Juan Dela Cruz",
                Hours       = 5,
                TotalAmount = 487.5m,
                Status      = "Returned",
                RentalDate  = new DateTime(2026, 4, 15)
            }
        };
    }
}