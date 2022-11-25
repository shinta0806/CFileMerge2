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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Serilog;
using Shinta;
using Windows.Graphics;
using Windows.Storage;
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
    /// ベール追加
    /// </summary>
    public async Task AddVeilAsync(String? childName = null, Object? dataContext = null)
    {
        Debug.WriteLine("AddVeilAsync()");
        if (_veiledElement != null)
        {
            throw new Exception("内部エラー：既にベールに覆われています。");
        }
        Frame frame = (Frame)Content;
        Page page = (Page)frame.Content;
        _veiledElement = page.Content;

        // いったん切り離し
        page.Content = null;

        // 再構築
        Grid veilGrid = (Grid)await LoadDynamicXamlAsync("VeilGrid");
        veilGrid.Children.Add(_veiledElement);
        if (!String.IsNullOrEmpty(childName))
        {
            FrameworkElement element = (FrameworkElement)await LoadDynamicXamlAsync(childName);
            element.DataContext = dataContext;
            veilGrid.Children.Add(element);
        }
        page.Content = veilGrid;
    }

    /// <summary>
    /// ベール除去
    /// </summary>
    public void RemoveVeil()
    {
        if (_veiledElement == null)
        {
            throw new Exception("内部エラー：ベールに覆われていません。");
        }

        Frame frame = (Frame)Content;
        Page page = (Page)frame.Content;
        Grid veilGrid = (Grid)page.Content;
        veilGrid.Children.Clear();
        page.Content = _veiledElement;
        _veiledElement = null;
    }

    /// <summary>
    /// ウィンドウをモーダルで表示
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public async Task ShowDialogAsync(WindowEx window)
    {
        if (_openingDialog != null)
        {
            throw new Exception("内部エラー：既にダイアログが開いています。");
        }
        _openingDialog = window;

        // ディスプレイサイズが不明なのでカスケードしない（はみ出し防止）
        await AddVeilAsync();
        window.Closed += DialogClosed;
        window.AppWindow.Move(new PointInt32(App.MainWindow.AppWindow.Position.X, App.MainWindow.AppWindow.Position.Y));
        window.Activate();

        await Task.Run(() =>
        {
            _dialogEvent.WaitOne();
        });
        RemoveVeil();
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

    /// <summary>
    /// ベールに覆われている UIElement
    /// </summary>
    private UIElement? _veiledElement;

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
        _openingDialog = null;
        _dialogEvent.Set();
    }

    /// <summary>
    /// 実行バイナリ内の XAML を読み込んでコントロールを作成
    /// </summary>
    /// <returns></returns>
    private async Task<Object> LoadDynamicXamlAsync(String name)
    {
        Uri uri = new("ms-appx:///Views/Dynamics/" + name + Common.FILE_EXT_XAML);
        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
        using StreamReader streamReader = new StreamReader(await file.OpenStreamForReadAsync());
        String xaml = await streamReader.ReadToEndAsync();
        return XamlReader.Load(xaml);
    }

    /// <summary>
    /// イベントハンドラー：メインウィンドウ Activated / Deactivated
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args"></param>
    private void WindowActivated(Object _, WindowActivatedEventArgs args)
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
