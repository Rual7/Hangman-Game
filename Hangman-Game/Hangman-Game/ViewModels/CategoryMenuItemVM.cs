using Hangman_Game.ViewModels.Base;
using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class CategoryMenuItemVM : BaseVM
{
    #region Fields

    private bool _isChecked;

    #endregion

    #region Public Properties

    public string Header { get; set; } = string.Empty;

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    public ICommand Command { get; set; } = null!;

    public object? CommandParameter { get; set; }

    #endregion
}