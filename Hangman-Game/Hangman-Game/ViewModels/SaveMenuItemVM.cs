using System.Windows.Input;

namespace Hangman_Game.ViewModels;

public class SaveMenuItemVM
{
    public string Header { get; set; } = string.Empty;
    public ICommand Command { get; set; } = null!;
    public object? CommandParameter { get; set; }
}
