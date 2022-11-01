using CFileMerge2.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CFileMerge2.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        ViewModel = App.GetService<MainPageViewModel>();
        InitializeComponent();
    }

    public MainPageViewModel ViewModel
    {
        get;
    }
}
