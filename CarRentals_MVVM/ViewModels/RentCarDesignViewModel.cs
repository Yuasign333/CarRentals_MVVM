// ─────────────────────────────────────────────────────────────────────────────
// Design-time only ViewModel for RentCarWindow.xaml.
// Provides a fake selected car and booking fields so the Visual Studio
// designer shows a preview of the rent car form.
// This class is NEVER used at runtime — only in the XAML designer.
// Connected to: RentCarWindow.xaml (via d:DataContext)
// ─────────────────────────────────────────────────────────────────────────────

using CarRentals_MVVM.Models;

namespace CarRentals_MVVM.ViewModels
{
    public class RentCarDesignViewModel
    {
        // ── Display labels ─────────────────────────────────────────────────────

        /// <summary>
        /// Fake user label shown in the top-right badge in the designer.
        /// </summary>
        public string UserLabel { get; } = "Customer: C001";

        // ── Step switching (for RentCarWindow if it uses steps) ────────────────

        /// <summary>
        /// Current step shown in the designer.
        ///   1 = Color picker step
        ///   2 = Booking form step
        /// </summary>
        public int Step { get; } = 1;

        /// <summary>True when Step 1 (color picker) is active.</summary>
        public bool IsStep1 => Step == 1;

        /// <summary>True when Step 2 (booking form) is active.</summary>
        public bool IsStep2 => Step == 2;

        // ── Selected car ───────────────────────────────────────────────────────

        /// <summary>
        /// Fake selected car shown in the rent car form in the designer.
        /// Provides all fields needed to preview the UI layout.
        /// </summary>
        public CarModel Car { get; } = new CarModel
        {
            CarId = "C001",
            Name = "Toyota Camry",
            Category = "Sedan",
            FuelType = "Standard Engine",
            PricePerHour = 40,
            ImageUrl = "https://i.imgur.com/gMFP5tP.jpeg",
            AvailableColors = ["White", "Silver", "Gray"]
        };

        // ── Booking form fields ────────────────────────────────────────────────

        /// <summary>
        /// Fake selected color shown in the color picker in the designer.
        /// </summary>
        public string SelectedColor { get; set; } = "White";

        /// <summary>
        /// Fake driver name shown in the booking form in the designer.
        /// </summary>
        public string DriverName { get; set; } = "Juan Dela Cruz";

        /// <summary>
        /// Fake rental hours shown in the booking form in the designer.
        /// </summary>
        public int Hours { get; set; } = 3;

        // ── Computed price fields ──────────────────────────────────────────────

        /// <summary>
        /// Fake base price shown in the Rental Estimate box in the designer.
        /// </summary>
        public decimal BasePrice { get; } = 120m;

        /// <summary>
        /// Fake deposit shown in the Rental Estimate box in the designer.
        /// </summary>
        public decimal Deposit { get; } = 36m;

        /// <summary>
        /// Fake total due shown in the Rental Estimate box in the designer.
        /// </summary>
        public decimal TotalDue { get; } = 156m;
    }
}