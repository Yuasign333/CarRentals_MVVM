using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CarRentals_MVVM.Commands
{
    /// <summary>
    /// A specialized ICommand for async database and network operations.
    /// Prevents double-clicking while the task is running and properly
    /// awaits background work without freezing the UI.
    /// Connected to: All ViewModels that call CarDataService async methods.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<object?, Task> executeAsync)
        {
            _executeAsync = executeAsync
                ?? throw new ArgumentNullException(nameof(executeAsync));
        }

        // Button is only clickable when no task is running
        public bool CanExecute(object? parameter) => !_isExecuting;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // ICommand requires void — we manage async safely here
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested(); // disable button

            try
            {
                await _executeAsync(parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested(); // re-enable button
            }
        }
    }
}