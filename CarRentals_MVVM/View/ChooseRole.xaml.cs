using System.Windows;
using System.Windows.Input;
using CarRentals_MVVM.Services;

namespace CarRentals_MVVM.View
{
    public partial class ChooseRole : Window
    {
        public ChooseRole()
        {
            InitializeComponent();
    
            this.Loaded += (s, e) => NavigationService.SetCurrent(this);
        }

        private void CustomerCard_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new CustomerLogin());
        }

        private void AdminCard_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new AdminLogin());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Exit?", "Exit",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                Application.Current.Shutdown(); // ✅ Explicit shutdown
        }
    }
}