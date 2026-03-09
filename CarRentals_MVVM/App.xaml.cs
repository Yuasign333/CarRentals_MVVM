using System.Configuration;
using System.Data;
using System.Windows;

namespace CarRentals_MVVM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e) // Ensures the application fully exits when the main window is closed
        {
            base.OnExit(e);
            Environment.Exit(0);
        }
    }

}
