// ─────────────────────────────────────────────────────────────────────────────
// Connected to: ChooseRoleViewModel.cs
// Purpose: Entry point of the application. Lets the user pick a role
//          before proceeding to the login screen.
// All button interactions (Customer, Admin, Exit) are handled via
// Command bindings in ChooseRole.xaml — zero click handlers here.
// ─────────────────────────────────────────────────────────────────────────────

using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class ChooseRole : Window
    {
        public ChooseRole()
        {
            InitializeComponent();

            // Register this window with NavigationService so it knows
            // which window is currently active before any navigation occurs.
            // Must be in Loaded (not constructor) so the window is fully ready.
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);

            // Bind the ViewModel — all role card and exit button logic lives there
            this.DataContext = new ChooseRoleViewModel();
        }
    }
}