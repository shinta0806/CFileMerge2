// ============================================================================
// 
// バージョン情報ビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Windows.Input;

using CFileMerge2.Models.SharedMisc;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Serilog.Events;
using Serilog;

namespace CFileMerge2.ViewModels;

public class AboutPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public AboutPageViewModel(WindowEx window)
    {
        // 初期化
        _window = window;

        // コマンド
        ButtonOkClickedCommand = new RelayCommand(ButtonOkClicked);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // コマンド
    // --------------------------------------------------------------------

    #region OK ボタンの制御
    public ICommand ButtonOkClickedCommand
    {
        get;
    }

    private void ButtonOkClicked()
    {
        Debug.WriteLine("ButtonOkClicked()");
        _window.Close();
    }
    #endregion

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：Escape キー押下
    /// </summary>
    public async void KeyboardAcceleratorEscapeInvoked(KeyboardAccelerator _1, KeyboardAcceleratorInvokedEventArgs _2)
    {
        try
        {
            _window.Close();
        }
        catch (Exception ex)
        {
            await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, "Escape キー押下時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }

    /// <summary>
    /// イベントハンドラー：メインパネルのサイズが変更された
    /// Depend: Window.SizeToContent が実装されればこのコードは不要
    /// </summary>
    public void MainPanelSizeChanged(Object sender, SizeChangedEventArgs _)
    {
        try
        {
            Double mainUiHeight = ((FrameworkElement)sender).ActualHeight + Cfm2Constants.MARGIN_DEFAULT * 2;
            Debug.WriteLine("AboutPageViewModel.MainPanelSizeChanged() mainUiHeight: " + mainUiHeight);
            if (mainUiHeight < _prevMainUiHeight)
            {
                return;
            }

            _window.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(_window.AppWindow.ClientSize.Width, (Int32)mainUiHeight));
            _prevMainUiHeight = mainUiHeight;
        }
        catch (Exception ex)
        {
            // ユーザー起因では発生しないイベントなのでログのみ
            Log.Error("メインパネルサイズ変更時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
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
    private readonly WindowEx _window;

    /// <summary>
    /// 前回のメイン UI の高さ
    /// </summary>
    private Double _prevMainUiHeight;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        Debug.WriteLine("AboutPageViewModel.Initialize()");
        _window.Title = Cfm2Constants.APP_NAME_J + "のバージョン情報";

        // なぜか MainWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        // Depend: 効くようになればこのコードは不要
        _window.Width = 600;

        // Height は後で MainPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        // Depend: Window.SizeToContent が実装されればこのコードは不要
        _window.Height = 600;
    }
}
