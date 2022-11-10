// ============================================================================
// 
// バージョン情報ビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

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
        _window = window;
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
    }
    #endregion

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：ページがロードされた
    /// </summary>
    public void PageLoaded(Object _1, RoutedEventArgs _2)
    {
        InitializeIfNeeded();
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// ウィンドウ
    /// </summary>
    private readonly WindowEx _window;

    /// <summary>
    /// 初期化済フラグ
    /// </summary>
    private Boolean _initialized;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 必要に応じて初期化
    /// </summary>
    private void InitializeIfNeeded()
    {
        if (_initialized)
        {
            return;
        }

        Debug.WriteLine("AboutPageViewModel.InitializeIfNeeded()");

        // なぜか MainWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        // Depend: 効くようになればこのコードは不要
        _window.Width = 500;

        // Height は後で MainPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        // Depend: Window.SizeToContent が実装されればこのコードは不要
        _window.Height = 550;

        // 初期化完了
        _initialized = true;
    }

}
