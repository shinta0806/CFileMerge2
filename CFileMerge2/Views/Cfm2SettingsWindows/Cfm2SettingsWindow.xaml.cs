// ============================================================================
// 
// 環境設定ウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

using CFileMerge2.Models.SharedMisc;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using PInvoke;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Serilog.Events;
using Serilog;
using System.Reflection.Metadata;
using Microsoft.UI.Xaml;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public delegate IntPtr Subclassproc(IntPtr hwnd, UInt32 msg, IntPtr wpalam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);


public sealed partial class Cfm2SettingsWindow : WindowEx2
{
    [DllImport("Comctl32.dll")]
    internal static extern Boolean SetWindowSubclass(IntPtr hwnd, Subclassproc subclassproc, IntPtr uIdSubclass, IntPtr dwRefData);

    [DllImport("Comctl32.dll")]
    internal static extern IntPtr DefSubclassProc(IntPtr hwnd, UInt32 msg, IntPtr wpalam, IntPtr lParam);


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
        IntPtr handle = WindowNative.GetWindowHandle(this);
        User32.SetWindowLongFlags exStyle = (User32.SetWindowLongFlags)User32.GetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE);
        User32.SetWindowLong(handle, User32.WindowLongIndexFlags.GWL_EXSTYLE, exStyle | User32.SetWindowLongFlags.WS_EX_CONTEXTHELP);
        SetWindowSubclass(handle, MySubclassproc, IntPtr.Zero, IntPtr.Zero);
#if false
        var a = PresenterKind;
        var b = (OverlappedPresenter)Presenter;
        b.IsModal = true;
#endif
    }

    private IntPtr MySubclassproc(IntPtr hwnd, UInt32 msg, IntPtr wpalam, IntPtr lparam, IntPtr uIdSubclass, IntPtr dwRefData)
    {
        Debug.WriteLine("MySubclassproc()"+Environment.TickCount.ToString("#,0"));
        switch (msg)
        {
            case 0x0112:
                Debug.WriteLine("MySubclassproc() WM_SYSCOMMAND");
                if((UInt32)wpalam== 0xF180)
                {
                    Debug.WriteLine("MySubclassproc() WM_SYSCOMMAND - SC_CONTEXTHELP");
                    _ = Task.Run(async () => {
                        try
                        {
                            await Cfm2Common.ShowHelpAsync();
                        }
                        catch (Exception ex)
                        {
                            await ShowLogMessageDialogAsync(LogEventLevel.Error, "ヘルプ表示時エラー：\n" + ex.Message);
                            Log.Information("スタックトレース：\n" + ex.StackTrace);
                        }
                    });
                    return IntPtr.Zero;
                }
                return DefSubclassProc(hwnd, msg, wpalam, lparam);
            default:
                return DefSubclassProc(hwnd, msg, wpalam, lparam);
        }
    }
}
