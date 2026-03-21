using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CarRentals_MVVM.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels and Models that need to notify the UI
    /// when a property value changes. Implements INotifyPropertyChanged.
    /// Connected to: All ViewModels and Models that inherit from this class.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
     
        // Event raised when a property value changes.
        // WPF data bindings listen to this event to refresh the UI automatically.
        public event PropertyChangedEventHandler? PropertyChanged;

     
        /// Raises the PropertyChanged event for the given property name.
        /// The [CallerMemberName] attribute automatically fills in the calling
        /// property's name so you don't have to type it manually.
     
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
        }
    }
}