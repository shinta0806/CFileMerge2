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

using Shinta;
using Shinta.WinUi3;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

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
        _newSubclassProc = new SUBCLASSPROC(SubclassProc);
        WinUi3Common.EnableContextHelp(this, _newSubclassProc);
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// ウィンドウプロシージャー
    /// </summary>
    private readonly SUBCLASSPROC _newSubclassProc;

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
    private LRESULT SubclassProc(HWND hWnd, UInt32 msg, WPARAM wPalam, LPARAM lParam, UIntPtr _1, UIntPtr _2)
    {
        switch (msg)
        {
            case PInvoke.WM_SYSCOMMAND:
                if ((UInt32)wPalam == PInvoke.SC_CONTEXTHELP)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Cfm2Common.ShowHelpAsync(this, "Kankyousettei");
                        }
                        catch (Exception ex)
                        {
                            await ShowExceptionLogMessageDialogAsync("ヘルプ表示時エラー", ex);
                        }
                    });
                    return (LRESULT)IntPtr.Zero;
                }

                // ヘルプボタン以外は次のハンドラーにお任せ
                return PInvoke.DefSubclassProc(hWnd, msg, wPalam, lParam);
            default:
                // WM_SYSCOMMAND 以外は次のハンドラーにお任せ
                return PInvoke.DefSubclassProc(hWnd, msg, wPalam, lParam);
        }
    }
}

