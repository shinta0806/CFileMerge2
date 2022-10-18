using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CFileMerge2.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
        ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClicked);
    }

    public ICommand ButtonBrowseMakeClickedCommand
    {
        get;
    }

    private void ButtonBrowseMakeClicked()
    {
        Debug.WriteLine("ButtonBrowseMakeClicked()");
    }

}
