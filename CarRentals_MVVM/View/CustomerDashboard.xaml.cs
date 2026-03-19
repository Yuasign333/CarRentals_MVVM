using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CarRentals_MVVM.Services;
using CarRentals_MVVM.ViewModels;

namespace CarRentals_MVVM.View
{
    public partial class CustomerDashboard : Window
    {
        
        public CustomerDashboard(string userId)
        {
            InitializeComponent();
           
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
            this.DataContext = new CustomerDashboardViewModel(
                new Models.UserModel { UserID = userId, Role = "Customer" });
        }
        
        
    }
}