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
using CFileMerge2.Contracts.Services;
using CFileMerge2.Models.SharedMisc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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
    // View 通信用のプロパティー
    // --------------------------------------------------------------------

    // メイクファイル
    private String _makePath = String.Empty;
    public String MakePath
    {
        get => _makePath;
        set => SetProperty(ref _makePath, value);
    }


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
        fileOpenPicker.FileTypeFilter.Add(Cfm2Constants.FILE_EXT_CFM2_MAKE);
        fileOpenPicker.FileTypeFilter.Add("*");
        IntPtr hwnd = App.MainWindow.GetWindowHandle();
        InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

        StorageFile? file = await fileOpenPicker.PickSingleFileAsync();
        if (file == null)
        {
            return;
        }

        MakePath = file.Path;

#if false
        Debug.WriteLine("ButtonBrowseMakeClicked() " + file.Path);

        String? read = await App.GetService<ILocalSettingsService>().ReadSettingAsync<String>("TestLocalSettingsKey");
        Debug.WriteLine("ButtonBrowseMakeClicked() read: " + read);
        await App.GetService<ILocalSettingsService>().SaveSettingAsync("TestLocalSettingsKey", "hoge " + DateTime.Now.ToString());

        Debug.WriteLine("Path: " + ApplicationData.Current.LocalFolder.Path);
        Debug.WriteLine("Name: " + ApplicationData.Current.LocalSettings.Name);
#endif
    }
    #endregion
}
