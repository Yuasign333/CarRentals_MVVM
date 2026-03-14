using System.Windows;
using CarRentals_MVVM.Models;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class RentCarWindow : Window
    {
        // Only one constructor now — always called with a selected car
        public RentCarWindow(string userId, CarModel car)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new RentCarViewModel(userId, car);
        }
    }
}