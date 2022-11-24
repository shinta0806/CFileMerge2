// ============================================================================
// 
// ウィンドウの拡張
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Serilog;
using Windows.Graphics;
using Windows.UI.Popups;
using WinUIEx;

namespace CFileMerge2.Views;

public class WindowEx2 : WindowEx
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public WindowEx2()
    {
        // イベントハンドラー
        Activated += WindowActivated;
        AppWindow.Closing += AppWindowClosing;
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // 一般のプロパティー
    // --------------------------------------------------------------------

    /// <summary>
    /// 内容に合わせてサイズ調整
    /// 関連する操作は PageEx2 にやってもらう
    /// ToDo: Window.SizeToContent が実装されれば不要となるコード
    /// </summary>
    public SizeToContent SizeToContent
    {
        get;
        set;
    }

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// ウィンドウをモーダルで表示
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public Task ShowDialogAsync(WindowEx window)
    {
        if (_openingDialog != null)
        {
            throw new Exception("内部エラー：既にダイアログが開いています。");
        }
        _openingDialog = window;

        // ディスプレイサイズが不明なのでカスケードしない（はみ出し防止）
        //ShowOverlapArea();
        window.Closed += DialogClosed;
        window.AppWindow.Move(new PointInt32(App.MainWindow.AppWindow.Position.X, App.MainWindow.AppWindow.Position.Y));
        window.Activate();

        return Task.Run(() =>
        {
            _dialogEvent.WaitOne();
            _openingDialog = null;
            //HideOverlapArea();
        });
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// 開いているダイアログウィンドウ
    /// </summary>
    private WindowEx? _openingDialog;

    /// <summary>
    /// ダイアログ制御用
    /// </summary>
    private readonly AutoResetEvent _dialogEvent = new(false);

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：ウィンドウが閉じられようとしている
    /// </summary>
    private void AppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        // 開いているダイアログがある場合は閉じる（タスクバーから閉じられた場合などは可能性がある）
        _openingDialog?.Close();
    }

    /// <summary>
    /// イベントハンドラー：ダイアログが閉じられた
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void DialogClosed(object sender, WindowEventArgs args)
    {
        Debug.WriteLine("DialogClosed()");
        _dialogEvent.Set();
    }

    /// <summary>
    /// イベントハンドラー：メインウィンドウ Activated / Deactivated
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args"></param>
    public void WindowActivated(Object _, WindowActivatedEventArgs args)
    {
#if DEBUG
        if (args.WindowActivationState == WindowActivationState.CodeActivated || args.WindowActivationState == WindowActivationState.PointerActivated)
        {
            Debug.WriteLine("WindowActivated() " + App.MainWindow.Content.ActualSize.Y);
        }
#endif
        if ((args.WindowActivationState == WindowActivationState.PointerActivated || args.WindowActivationState == WindowActivationState.CodeActivated) && _openingDialog != null)
        {
            _openingDialog.Activate();
        }
    }


}
