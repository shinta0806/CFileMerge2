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
using WinRT.Interop;
using WinUIEx;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsWindow : WindowEx2
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
        Content = new Cfm2SettingsPage(this);
        WinUi3Common.EnableContextHelp(this, SubclassProc);
#if false
        var a = PresenterKind;
        var b = (OverlappedPresenter)Presenter;
        b.IsModal = true;
#endif
    }

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
    private IntPtr SubclassProc(IntPtr hwnd, UInt32 msg, IntPtr wPalam, IntPtr lParam, IntPtr _1, IntPtr _2)
    {
        switch ((User32.WindowMessage)msg)
        {
            case User32.WindowMessage.WM_SYSCOMMAND:
                if ((User32.SysCommands)wPalam == User32.SysCommands.SC_CONTEXTHELP)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Cfm2Common.ShowHelpAsync(this);
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
