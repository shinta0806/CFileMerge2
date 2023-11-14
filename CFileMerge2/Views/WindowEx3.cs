// ============================================================================
// 
// ウィンドウの拡張
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Windows.Input;

using CFileMerge2.Models.SharedMisc;

using CommunityToolkit.Mvvm.Input;

using Serilog;
using Serilog.Events;

using Shinta.WinUi3.Views;

namespace CFileMerge2.Views;

public class WindowEx3 : WindowEx2
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public WindowEx3()
    {
        // コマンド
        HelpClickedCommand = new RelayCommand<String>(HelpClicked);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // コマンド
    // --------------------------------------------------------------------

    #region ヘルプリンクの制御
    public ICommand HelpClickedCommand
    {
        get;
    }

    private async void HelpClicked(String? parameter)
    {
        try
        {
            await Cfm2Common.ShowHelpAsync(this, parameter);
        }
        catch (Exception ex)
        {
            await ShowLogMessageDialogAsync(LogEventLevel.Error, "ヘルプ表示時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion
}
