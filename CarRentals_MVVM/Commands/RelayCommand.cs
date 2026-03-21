using System;
using System.Windows.Input;

namespace CarRentals_MVVM.Commands
{
    /// <summary>
    /// A reusable ICommand implementation that delegates execution logic
    /// to Action and Func delegates passed in from the ViewModel.
    /// Used by all ViewModels to bind UI buttons and interactions to backend logic.
    /// Connected to: All ViewModels (AdminDashboardViewModel, BrowseCarsViewModel, etc.)
    /// </summary>
    public class RelayCommand : ICommand
    {
        // The action to run when the command executes
        private readonly Action<object?> _execute;

        // Optional condition that determines if the command can run
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Fires when the ability to execute this command may have changed.
        /// Hooks into WPF's built-in CommandManager to auto-refresh button states.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// Creates a new RelayCommand.
        /// </summary>
        /// <param name="execute">The action to run on execution. Cannot be null.</param>
        /// <param name="canExecute">Optional condition. If null, command is always enabled.</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            // Throw immediately if no action is provided — prevents silent failures
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Checks whether the command is allowed to run.
        /// Returns true if no canExecute condition was provided.
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        /// <summary>
        /// Runs the command's action with the given parameter.
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}