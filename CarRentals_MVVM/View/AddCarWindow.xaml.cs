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

        private void BackBtn_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AddCarViewModel vm)
                vm.BackCommand.Execute(null);
        }

        private void Button_Click(object sender, RoutedEventArgs e) { }
    }
}