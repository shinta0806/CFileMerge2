// ============================================================================
// 
// メインビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;

namespace CFileMerge2.ViewModels;

public class MainPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    // --------------------------------------------------------------------
    // メインコンストラクター
    // --------------------------------------------------------------------
    public MainPageViewModel()
    {
        ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClickedAsync);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // コマンド
    // --------------------------------------------------------------------

    #region 参照ボタンの制御
    public ICommand ButtonBrowseMakeClickedCommand
    {
        get;
    }

    private async void ButtonBrowseMakeClickedAsync()
    {
        FileOpenPicker fileOpenPicker = new();
        fileOpenPicker.FileTypeFilter.Add("*");
        fileOpenPicker.FileTypeFilter.Add(".hoge");
        fileOpenPicker.FileTypeFilter.Add(".abc|あいう");
        fileOpenPicker.FileTypeFilter.Add(".def;かきく");
        var hwnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(fileOpenPicker, hwnd);
        Windows.Storage.StorageFile file = await fileOpenPicker.PickSingleFileAsync();
        Debug.WriteLine("ButtonBrowseMakeClicked() " + file.Path);
    }
    #endregion
}
