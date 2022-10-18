using CFileMerge2.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CFileMerge2.Views;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainPageViewModel>();
        InitializeComponent();
    }
}
