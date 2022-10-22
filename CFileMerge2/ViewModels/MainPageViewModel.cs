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
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Composition.Desktop;
using Windows.UI.Popups;
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
        ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClicked);
        ButtonGoClickedCommand = new RelayCommand(ButtonGoClicked);
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

    private async void ButtonBrowseMakeClicked()
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

    #region スタートボタンの制御
    public ICommand ButtonGoClickedCommand
    {
        get;
    }

    private async void ButtonGoClicked() => await MergeAsync();
    #endregion

    // ====================================================================
    // private 関数
    // ====================================================================

    private void DisableControls()
    {
        
    }


    // --------------------------------------------------------------------
    // 合併メインルーチン
    // --------------------------------------------------------------------
    private async Task MergeAsync()
    {
        await Task.Run(async () =>
        {
            try
            {
                DisableControls();
                throw new Exception("hoge");
            }
            catch (Exception ex)
            {
                await App.MainWindow.CreateMessageDialog(ex.Message, "エラー").ShowAsync();
            }
        });
    }

}
