// ============================================================================
// 
// 環境設定ウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

using System.Diagnostics;

using CFileMerge2.Models.SharedMisc;

using Microsoft.UI.Windowing;

using PInvoke;

using Serilog;
using Serilog.Events;

using Shinta;
using Shinta.WinUi3;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsWindow : WindowEx3
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsWindow()
    {
        InitializeComponent();

        // 初期化
        SizeToContent = SizeToContent.Height;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Title = "Cfm2SettingsWindow_Title".ToLocalized();
        Content = new Cfm2SettingsPage(this);
        _newSubclassProc = new WindowsApi.SubclassProc(SubclassProc);
        WinUi3Common.EnableContextHelp(this, _newSubclassProc);
#if false
        var a = PresenterKind;
        var b = (OverlappedPresenter)Presenter;
        b.IsModal = true;
#endif
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// ウィンドウプロシージャー
    /// </summary>
    private readonly WindowsApi.SubclassProc _newSubclassProc;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// ウィンドウメッセージ処理
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="msg"></param>
    /// <param name="wPalam"></param>
    /// <param name="lParam"></param>
    /// <param name="_1"></param>
    /// <param name="_2"></param>
    /// <returns></returns>
    private IntPtr SubclassProc(IntPtr hwnd, User32.WindowMessage msg, IntPtr wPalam, IntPtr lParam, IntPtr _1, IntPtr _2)
    {
        Debug.WriteLine("SubclassProc()" + Environment.TickCount.ToString("#,0"));
        switch (msg)
        {
            case User32.WindowMessage.WM_SYSCOMMAND:
                if ((User32.SysCommands)wPalam == User32.SysCommands.SC_CONTEXTHELP)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Cfm2Common.ShowHelpAsync(this, "Kankyousettei");
                        }
                        catch (Exception ex)
                        {
                            await ShowLogMessageDialogAsync(LogEventLevel.Error, "ヘルプ表示時エラー：\n" + ex.Message);
                            Log.Information("スタックトレース：\n" + ex.StackTrace);
                        }
                    });
                    return IntPtr.Zero;
                }
                return WindowsApi.DefSubclassProc(hwnd, msg, wPalam, lParam);
            default:
                return WindowsApi.DefSubclassProc(hwnd, msg, wPalam, lParam);
        }
    }
}

