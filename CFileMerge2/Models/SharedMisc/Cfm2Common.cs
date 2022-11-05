// ============================================================================
// 
// ちょちょいとファイル合併 2 共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Helpers;
using Serilog;
using Shinta;

namespace CFileMerge2.Models.SharedMisc;

internal class Cfm2Common
{
    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// ヘルプの表示
    /// </summary>
    /// <param name="anchor"></param>
    /// <returns></returns>
    public static async Task ShowHelpAsync(String? anchor = null)
    {
        String? helpPath = null;

        try
        {
            // アンカーが指定されている場合は状況依存型ヘルプを表示
            if (!String.IsNullOrEmpty(anchor))
            {
                helpPath = Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + Cfm2Constants.FOLDER_NAME_HELP_PARTS
                        + FILE_NAME_HELP_PREFIX + "_" + anchor + Common.FILE_EXT_HTML;
                try
                {
                    Common.ShellExecute(helpPath);
                    return;
                }
                catch (Exception ex)
                {
                    await App.MainWindow.CreateMessageDialog("状況に応じたヘルプを表示できませんでした：\n" + ex.Message + "\n" + helpPath
                            + "\n通常のヘルプを表示します。", Cfm2Constants.LABEL_ERROR).ShowAsync();
                }
            }

            // アンカーが指定されていない場合・状況依存型ヘルプを表示できなかった場合は通常のヘルプを表示
            helpPath = Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
            Common.ShellExecute(helpPath);
        }
        catch (Exception ex)
        {
            await App.MainWindow.CreateMessageDialog("ヘルプを表示できませんでした。\n" + ex.Message + "\n" + helpPath, Cfm2Constants.LABEL_ERROR).ShowAsync();
        }
    }

    /// <summary>
    /// ログの記録と表示
    /// </summary>
    /// <param name="logEventLevel"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task ShowLogMessageDialogAsync(Serilog.Events.LogEventLevel logEventLevel, String message)
    {
        Log.Write(logEventLevel, message);
        await App.MainWindow.CreateMessageDialog(message, logEventLevel.ToString().GetLocalized()).ShowAsync();
    }

    // ====================================================================
    // private 定数
    // ====================================================================

    /// <summary>
    /// ヘルプファイル名
    /// </summary>
    private const String FILE_NAME_HELP_PREFIX = Cfm2Constants.APP_ID + "_JPN";
}
