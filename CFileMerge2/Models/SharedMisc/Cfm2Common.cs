// ============================================================================
// 
// ちょちょいとファイル合併 2 共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using CFileMerge2.Contracts.Services;
using CFileMerge2.Helpers;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Services;
using Serilog;
using Serilog.Events;

using Shinta;
using Shinta.WinUi3;
using Windows.Foundation;
using Windows.UI.Popups;

namespace CFileMerge2.Models.SharedMisc;

internal class Cfm2Common
{
    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 最新情報の確認
    /// </summary>
    /// <param name="forceShow"></param>
    /// <returns></returns>
    public static async Task CheckLatestInfoAsync(Boolean forceShow)
    {
        LatestInfoManager latestInfoManager = Cfm2Common.CreateLatestInfoManager(forceShow);
        if (await latestInfoManager.CheckAsync())
        {
            Cfm2Model.Instance.EnvModel.Cfm2Settings.RssCheckDate = DateTime.Now.Date;
            await Cfm2Model.Instance.EnvModel.SaveCfm2SettingsAsync();
        }
    }

    // --------------------------------------------------------------------
    // 最新情報管理者を作成
    // --------------------------------------------------------------------
    public static LatestInfoManager CreateLatestInfoManager(Boolean forceShow)
    {
        return new LatestInfoManager("http://shinta.coresv.com/soft/CFileMerge2_JPN.xml", forceShow, 3, Cfm2Constants.APP_VER,
                Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Token, App.MainWindow,
                ((LocalSettingsService)App.GetService<ILocalSettingsService>()).Folder() + "LatestInfo" + Common.FILE_EXT_CONFIG);
    }

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
                    await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, "状況に応じたヘルプを表示できませんでした：\n" + ex.Message + "\n" + helpPath
                            + "\n通常のヘルプを表示します。");
                }
            }

            // アンカーが指定されていない場合・状況依存型ヘルプを表示できなかった場合は通常のヘルプを表示
            helpPath = Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
            Common.ShellExecute(helpPath);
        }
        catch (Exception ex)
        {
            await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, "ヘルプを表示できませんでした。\n" + ex.Message + "\n" + helpPath);
        }
    }

    /// <summary>
    /// ログの記録と表示
    /// </summary>
    /// <param name="logEventLevel"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static IAsyncOperation<IUICommand> ShowLogMessageDialogAsync(LogEventLevel logEventLevel, String message)
    {
        return WinUi3Common.ShowLogMessageDialogAsync(App.MainWindow, logEventLevel, message);
    }

    // ====================================================================
    // private 定数
    // ====================================================================

    /// <summary>
    /// ヘルプファイル名
    /// </summary>
    private const String FILE_NAME_HELP_PREFIX = Cfm2Constants.APP_ID + "_JPN";
}
