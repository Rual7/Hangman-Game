using System.Windows.Input;

namespace Hangman_Game.ViewModels.Base;

public class RelayCommand : ICommand
{
    #region Fields

    private readonly Action<object?> _executeAction;
    private readonly Predicate<object?>? _canExecutePredicate;

    #endregion

    #region Events

    public event EventHandler? CanExecuteChanged;

    #endregion

    #region Constructors

    public RelayCommand(Action<object?> executeAction, Predicate<object?>? canExecutePredicate = null)
    {
        _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
        _canExecutePredicate = canExecutePredicate;
    }

    #endregion

    #region Public Command Methods

    public bool CanExecute(object? parameter)
    {
        return _canExecutePredicate == null || _canExecutePredicate(parameter);
    }

    public void Execute(object? parameter)
    {
        _executeAction(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}