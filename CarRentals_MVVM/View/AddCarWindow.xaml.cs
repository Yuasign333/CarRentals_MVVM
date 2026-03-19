using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class AddCarWindow : Window
    {
        public AddCarWindow(string userId)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new AddCarViewModel(userId);
        }

       
       
    }
}