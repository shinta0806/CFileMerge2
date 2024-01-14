// ============================================================================
// 
// ちょちょいとファイル合併 2 共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Strings;
using CFileMerge2.Views;

using Shinta;
using Shinta.WinUi3;

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
			SaveCfm2Settings();
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
		return new LatestInfoManager("http://shinta.coresv.com/soft/CFileMerge2_JPN.xml", forceShow, 3, Cfm2Constants.APP_VER, window)
		{
			CancellationToken = Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Token,
		};
	}

	/// <summary>
	/// 環境設定を読み込み
	/// </summary>
	public static void LoadNkm3Settings()
	{
		try
		{
			Cfm2Model.Instance.EnvModel.Cfm2Settings = Cfm2Model.Instance.EnvModel.JsonManager.Load<Cfm2Settings>(Cfm2Constants.FILE_NAME_CFM2_SETTINGS, false);
			Cfm2Model.Instance.EnvModel.Cfm2Settings.Adjust();
		}
		catch (Exception ex)
		{
			SerilogUtils.LogException("環境設定読み込み時エラー", ex);
		}
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
	/// 環境設定を保存
	/// </summary>
	public static void SaveCfm2Settings()
	{
		try
		{
			Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchVer = Cfm2Constants.APP_VER;
			Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchPath = Cfm2Model.Instance.EnvModel.ExeFullPath;
			Cfm2Model.Instance.EnvModel.JsonManager.Save(Cfm2Model.Instance.EnvModel.Cfm2Settings, Cfm2Constants.FILE_NAME_CFM2_SETTINGS, false);
		}
		catch (Exception ex)
		{
			SerilogUtils.LogException("環境設定保存時エラー", ex);
		}
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
					await window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.GeneralView_Error_NoAnchor.Localized() + "\n" + ex.Message + "\n" + helpPath
						+ "\n" + Localize.GeneralView_Information_ShowNormalHelp.Localized());
				}
			}

			// アンカーが指定されていない場合・状況依存型ヘルプを表示できなかった場合は通常のヘルプを表示
			helpPath = Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
			Common.ShellExecute(helpPath);
		}
		catch (Exception ex)
		{
			await window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.GeneralView_Error_CannotShowHelp.Localized() + "\n" + ex.Message + "\n" + helpPath);
		}
	}

	// ====================================================================
	// private 定数
	// ====================================================================

	/// <summary>
	/// ヘルプファイル名
	/// </summary>
	private const String FILE_NAME_HELP_PREFIX = Cfm2Constants.APP_ID + "_JPN";
}
