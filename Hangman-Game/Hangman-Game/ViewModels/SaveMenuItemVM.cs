using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class SaveMenuItemVM
{
    #region Public Properties

    public string Header { get; set; } = string.Empty;

    public ICommand Command { get; set; } = null!;

    public object? CommandParameter { get; set; }

    #endregion
}