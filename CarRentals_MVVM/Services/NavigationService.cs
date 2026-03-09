using System.Windows;
using System.Windows.Threading;

namespace CarRentals_MVVM.Services
{
    public static class NavigationService
    {
        private static Window? _current;

        public static void SetCurrent(Window window)
        {
            _current = window;
        }

        public static void Navigate(Window next)
        {
            var previous = _current;

            if (previous != null)
            {
                next.WindowState = previous.WindowState;
                next.WindowStartupLocation = WindowStartupLocation.Manual;
                next.Top = previous.Top;
                next.Left = previous.Left;
                next.Width = previous.Width;
                next.Height = previous.Height;
            }

            Application.Current.MainWindow = next;
            _current = next;

            next.Show();

            if (previous != null)
            {
                previous.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                    new System.Action(() => previous.Close()));
            }
        }
    }
}