using System;
using System.Windows;
using System.Windows.Threading;

namespace CarRentals_MVVM.Services
{
    /// <summary>
    /// Static service that handles window-to-window navigation throughout the app.
    /// Keeps track of the currently active window and replaces it with the next one,
    /// copying position and size so the transition feels seamless.
    /// Connected to: All ViewModels that navigate between windows.
    /// Called via NavigationService.Navigate(new SomeWindow(...)) in each ViewModel.
    /// </summary>
    public static class NavigationService
    {
        // The window currently displayed to the user
        private static Window? _current;

        /// <summary>
        /// Registers the given window as the currently active window.
        /// Must be called in each window's Loaded event (not the constructor)
        /// so the window is fully initialized before it is tracked.
        /// Called from: all .xaml.cs code-behind files via:
        ///   this.Loaded += (s, e) => NavigationService.SetCurrent(this);
        /// </summary>
        /// <param name="window">The window that just finished loading.</param>
        public static void SetCurrent(Window window)
        {
            _current = window;
        }

        /// <summary>
        /// Navigates from the current window to the next window.
        /// Copies the current window's position and size to the new window
        /// so it opens in the same location. The previous window is then closed.
        /// </summary>
        /// <param name="next">The new window to display.</param>
        public static void Navigate(Window next)
        {
            // Store a reference to the window we are leaving
            var previous = _current;

            // Copy position and size from the previous window to the next
            // This keeps the app feeling stable — no jumping around the screen
            if (previous != null)
            {
                next.WindowState = previous.WindowState;
                next.WindowStartupLocation = WindowStartupLocation.Manual;
                next.Top = previous.Top;
                next.Left = previous.Left;
                next.Width = previous.Width;
                next.Height = previous.Height;
            }

            // Register the new window as the main window and show it
            Application.Current.MainWindow = next;
            _current = next;
            next.Show();

            // Close the previous window after the new one has loaded
            // Using BeginInvoke with DispatcherPriority.Loaded ensures the new
            // window is fully visible before the old one disappears
            if (previous != null)
            {
                previous.Dispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(() => previous.Close())
                );
            }
        }
    }
}