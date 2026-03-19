using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class ChooseRole : Window
    {
        // Add a ChooseRoleViewModel with commands instead
        public ChooseRole()
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new ChooseRoleViewModel();
        }
        
    }

    
}