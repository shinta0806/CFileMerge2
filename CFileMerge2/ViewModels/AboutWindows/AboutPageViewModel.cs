// ============================================================================
// 
// バージョン情報ページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Reflection.Metadata;
using System.Windows.Input;

using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Serilog;
using Serilog.Events;
using Shinta;

namespace CFileMerge2.ViewModels.AboutWindows;

public class AboutPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public AboutPageViewModel(WindowEx3 window)
    {
        // 初期化
        _window = window;

        // コマンド
        ButtonCheckUpdateClickedCommand = new RelayCommand(ButtonCheckUpdateClicked);
        ButtonOkClickedCommand = new RelayCommand(ButtonOkClicked);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // コマンド
    // --------------------------------------------------------------------

    #region 更新プログラムの確認ボタンの制御
    public ICommand ButtonCheckUpdateClickedCommand
    {
        get;
    }

    private void ButtonCheckUpdateClicked()
    {
    }
    #endregion

    #region OK ボタンの制御
    public ICommand ButtonOkClickedCommand
    {
        get;
    }

    private async void ButtonOkClicked()
    {
        try
        {
            _window.Close();
        }
        catch (Exception ex)
        {
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "OK ボタンクリック時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：Escape キー押下
    /// </summary>
    public void KeyboardAcceleratorEscapeInvoked(KeyboardAccelerator _1, KeyboardAcceleratorInvokedEventArgs _2)
    {
        ButtonOkClicked();
    }

    /// <summary>
    /// イベントハンドラー：ページがロードされた
    /// </summary>
    public void PageLoaded(Object _1, RoutedEventArgs _2)
    {
        try
        {
            Initialize();

            // フォーカス
            FrameworkElement frameworkElement = (FrameworkElement)_window.Content;
            Button button = (Button)frameworkElement.FindName(Cfm2Constants.ELEMENT_NAME_BUTTON_OK);
            button.Focus(FocusState.Programmatic);
        }
        catch (Exception ex)
        {
            // ユーザー起因では発生しないイベントなのでログのみ
            Log.Error("ページロード時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// ウィンドウ
    /// </summary>
    private readonly WindowEx3 _window;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        _window.Title = Cfm2Constants.APP_NAME_J + "のバージョン情報";
    }
}
