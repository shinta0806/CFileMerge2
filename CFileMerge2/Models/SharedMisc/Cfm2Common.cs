// ============================================================================
// 
// ちょちょいとファイル合併 2 共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using CFileMerge2.Contracts.Services;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Services;
using CFileMerge2.Views;

using Serilog.Events;

using Shinta;
using Shinta.WinUi3;

using WinUIEx;

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
    public static async Task CheckLatestInfoAsync(Boolean forceShow, WindowEx window)
    {
        LatestInfoManager latestInfoManager = Cfm2Common.CreateLatestInfoManager(forceShow, window);
        if (await latestInfoManager.CheckAsync())
        {
            Cfm2Model.Instance.EnvModel.Cfm2Settings.RssCheckDate = DateTime.Now.Date;
            await Cfm2Model.Instance.EnvModel.SaveCfm2SettingsAsync();
        }
    }

    /// <summary>
    /// 最新情報管理者を作成
    /// </summary>
    /// <param name="forceShow"></param>
    /// <param name="window"></param>
    /// <returns></returns>
    public static LatestInfoManager CreateLatestInfoManager(Boolean forceShow, WindowEx window)
    {
        return new LatestInfoManager("http://shinta.coresv.com/soft/CFileMerge2_JPN.xml", forceShow, 3, Cfm2Constants.APP_VER,
                Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Token, window,
                ((LocalSettingsService)App.GetService<ILocalSettingsService>()).Folder() + "LatestInfo" + Common.FILE_EXT_CONFIG);
    }

    /// <summary>
    /// 環境情報をログする
    /// </summary>
    public static void LogEnvironmentInfo()
    {
        SystemEnvironment se = new();
        se.LogEnvironment();
    }

    /// <summary>
    /// ヘルプの表示
    /// </summary>
    /// <param name="anchor"></param>
    /// <returns></returns>
    public static async Task ShowHelpAsync(WindowEx3 window, String? anchor = null)
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
                    await window.ShowLogMessageDialogAsync(LogEventLevel.Error, "Cfm2Common_ShowHelpAsync_Error_NoAnchor".ToLocalized() + "\n" + ex.Message + "\n" + helpPath
                            + "\n" + "Cfm2Common_ShowHelpAsync_ShowNormalHelp".ToLocalized());
                }
            }

            // アンカーが指定されていない場合・状況依存型ヘルプを表示できなかった場合は通常のヘルプを表示
            helpPath = Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
            Common.ShellExecute(helpPath);
        }
        catch (Exception ex)
        {
            await window.ShowLogMessageDialogAsync(LogEventLevel.Error, "Cfm2Common_ShowHelpAsync_Error_CannotShowHelp".ToLocalized() + "\n" + ex.Message + "\n" + helpPath);
        }
    }

    /// <summary>
    /// テンポラリフォルダー配下のファイル・フォルダー名として使えるパス（呼びだす度に異なる、拡張子なし）
    /// </summary>
    /// <returns></returns>
    public static String TempPath()
    {
        // マルチスレッドでも安全にインクリメント
        Int32 counter = Interlocked.Increment(ref _tempPathCounter);
        return Common.TempFolderPath() + counter.ToString() + "_" + Environment.CurrentManagedThreadId.ToString();
    }

    // ====================================================================
    // private 定数
    // ====================================================================

    /// <summary>
    /// ヘルプファイル名
    /// </summary>
    private const String FILE_NAME_HELP_PREFIX = Cfm2Constants.APP_ID + "_JPN";

    // ====================================================================
    // private 変数
    // ====================================================================

    // TempPath() 用カウンター（同じスレッドでもファイル名が分かれるようにするため）
    private static Int32 _tempPathCounter = 0;
}
