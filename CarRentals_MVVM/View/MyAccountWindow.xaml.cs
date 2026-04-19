using System.Windows;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;


namespace CarRentals_MVVM.View
{
    public partial class MyAccountWindow : Window
    {
        public MyAccountWindow(string userId)
        {
            InitializeComponent();
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new MyAccountViewModel(userId);
        }
    }
}