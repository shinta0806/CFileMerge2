// ============================================================================
// 
// メンテナンスページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.IO.Compression;
using System.Windows.Input;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Strings;
using CFileMerge2.Views;

using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Dispatching;

using Shinta;
using Shinta.WinUi3;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public partial class Cfm2SettingsNavigationMaintenancePageViewModel : Cfm2SettingsNavigationPageViewModel
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public Cfm2SettingsNavigationMaintenancePageViewModel(WindowEx3 window, Cfm2SettingsPageViewModel cfm2SettingsPageViewModel)
			: base(window, cfm2SettingsPageViewModel)
	{
		// コマンド
		ButtonCheckRssClickedCommand = new RelayCommand(ButtonCheckRssClicked);
		ButtonBackupClickedCommand = new RelayCommand(ButtonBackupClicked);
		ButtonRestoreClickedCommand = new RelayCommand(ButtonRestoreClicked);
	}

	// ====================================================================
	// public プロパティー
	// ====================================================================

	// --------------------------------------------------------------------
	// View 通信用のプロパティー
	// --------------------------------------------------------------------

	/// <summary>
	/// 最新情報を自動的に確認するのラベル
	/// </summary>
#pragma warning disable CA1822
	public String CheckBoxCheckRssContent
	{
		get => String.Format(Localize.Cfm2SettingsNavigationMaintenancePage_CheckBoxCheckRss_Content.Localized(), Localize.AppInfo_AppName.Localized());
	}
#pragma warning restore CA1822

	/// <summary>
	/// 最新情報を自動的に確認する
	/// </summary>
	private Boolean _checkRss;
	public Boolean CheckRss
	{
		get => _checkRss;
		set
		{
			_ = Task.Run(async () =>
			{
				if (_checkRss && !value)
				{
					MessageDialog messageDialog = _window.CreateMessageDialog(
						String.Format(Localize.Cfm2SettingsNavigationMaintenancePageViewModel_Confirm_CheckRss.Localized(), Localize.AppInfo_AppName.Localized()),
						Localize.Warning.Localized());
					messageDialog.Commands.Add(new UICommand(Localize.Yes.Localized()));
					messageDialog.Commands.Add(new UICommand(Localize.No.Localized()));
					IUICommand cmd = await messageDialog.ShowAsync();
					if (cmd.Label != Localize.Yes.Localized())
					{
						_window.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
						{
							OnPropertyChanged(nameof(CheckRss));
						});
						return;
					}
				}

				SetProperty(ref _checkRss, value);
			});
		}
	}

	/// <summary>
	/// 最新情報確認中
	/// </summary>
	private Boolean _isProgressRingActive;
	public Boolean IsProgressRingActive
	{
		get => _isProgressRingActive;
		set => SetProperty(ref _isProgressRingActive, value);
	}

	/// <summary>
	/// 今すぐ最新情報を確認ボタンの有効性
	/// </summary>
	private Boolean _isButtonCheckRssEnabled = true;
	public Boolean IsButtonCheckRssEnabled
	{
		get => _isButtonCheckRssEnabled;
		set => SetProperty(ref _isButtonCheckRssEnabled, value);
	}

	// --------------------------------------------------------------------
	// コマンド
	// --------------------------------------------------------------------

	#region 今すぐ最新情報を確認ボタンの制御
	public ICommand ButtonCheckRssClickedCommand
	{
		get;
	}

	private async void ButtonCheckRssClicked()
	{
		try
		{
			IsButtonCheckRssEnabled = false;
			IsProgressRingActive = true;
			await Cfm2Common.CheckLatestInfoAsync(true, _window);
		}
		catch (Exception ex)
		{
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.Cfm2SettingsNavigationMaintenancePageViewModel_Error_ButtonCheckRssClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
		finally
		{
			IsProgressRingActive = false;
			IsButtonCheckRssEnabled = true;
		}
	}
	#endregion

	#region 設定のバックアップボタンの制御
	public ICommand ButtonBackupClickedCommand
	{
		get;
	}

	private async void ButtonBackupClicked()
	{
		try
		{
			_cfm2SettingsPageViewModel.CheckPropertiesAndPropertiesToSettings();

			FileSavePicker fileSavePicker = _window.CreateSaveFilePicker();
			fileSavePicker.FileTypeChoices.Add(Localize.GeneralView_FileTypeSta.Localized(), [Common.FILE_EXT_SETTINGS_ARCHIVE]);
			fileSavePicker.SuggestedFileName = Cfm2Constants.APP_ID + "Settings_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");

			StorageFile? file = await fileSavePicker.PickSaveFileAsync();
			if (file == null)
			{
				return;
			}

			File.Delete(file.Path);
			Cfm2Common.LogEnvironmentInfo();
			CreateBackup(file.Path);
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Information, Localize.Cfm2SettingsNavigationMaintenancePageViewModel_Done_ButtonBackupClicked.Localized());
		}
		catch (Exception ex)
		{
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.Cfm2SettingsNavigationMaintenancePageViewModel_Error_ButtonBackupClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 設定の復元ボタンの制御
	public ICommand ButtonRestoreClickedCommand
	{
		get;
	}

	private async void ButtonRestoreClicked()
	{
		try
		{
			FileOpenPicker fileOpenPicker = _window.CreateOpenFilePicker();
			fileOpenPicker.FileTypeFilter.Add(Common.FILE_EXT_SETTINGS_ARCHIVE);
			fileOpenPicker.FileTypeFilter.Add("*");

			StorageFile? file = await fileOpenPicker.PickSingleFileAsync();
			if (file == null)
			{
				return;
			}

			Restore(file.Path);
			_cfm2SettingsPageViewModel.SettingsToProperties();
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Information, Localize.Cfm2SettingsNavigationMaintenancePageViewModel_Done_ButtonRestoreClicked.Localized());
		}
		catch (OperationCanceledException)
		{
			Log.Information("設定の復元を中止しました。");
		}
		catch (Exception ex)
		{
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.Cfm2SettingsNavigationMaintenancePageViewModel_Error_ButtonRestoreClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	// ====================================================================
	// public 関数
	// ====================================================================

	/// <summary>
	/// プロパティーから設定に反映
	/// </summary>
	public override void PropertiesToSettings()
	{
		Cfm2Model.Instance.EnvModel.Cfm2Settings.CheckRss = CheckRss;
	}

	/// <summary>
	/// 設定をプロパティーに反映
	/// </summary>
	public override void SettingsToProperties()
	{
		CheckRss = Cfm2Model.Instance.EnvModel.Cfm2Settings.CheckRss;
	}

	// ====================================================================
	// private 関数
	// ====================================================================

	/// <summary>
	/// 設定フォルダー内の指定拡張子のファイルをテンポラリフォルダーにコピー
	/// </summary>
	/// <param name="ext"></param>
	/// <param name="tempFolderPath"></param>
	private static void BackupFiles(String ext, String tempFolderPath)
	{
		String[] files = Directory.GetFiles(CommonWindows.SettingsFolder(), "*" + ext, SearchOption.AllDirectories);
		foreach (String file in files)
		{
			String subfolderAndLeaf = file[CommonWindows.SettingsFolder().Length..];
			String? subfolder = Path.GetDirectoryName(subfolderAndLeaf);
			if (!String.IsNullOrEmpty(subfolder))
			{
				try
				{
					Directory.CreateDirectory(tempFolderPath + subfolder);
				}
				catch (Exception)
				{
				}
			}
			File.Copy(file, tempFolderPath + subfolderAndLeaf);
		}
	}

	/// <summary>
	/// バックアップ作成
	/// </summary>
	private static void CreateBackup(String destPath)
	{
		String tempFolderPath = Common.TempPath() + "\\" + Common.AppId() + "\\";
		Directory.CreateDirectory(tempFolderPath);

		// 設定を保存
		Cfm2Common.SaveCfm2Settings();

		// 設定類
		BackupFiles(Common.FILE_EXT_JSON, tempFolderPath);

		// ログ
		BackupFiles(Common.FILE_EXT_TXT, tempFolderPath);

		ZipFile.CreateFromDirectory(tempFolderPath, destPath, CompressionLevel.SmallestSize, true);
	}

	/// <summary>
	/// バックアップから復元
	/// </summary>
	/// <param name=""></param>
	private static void Restore(String archivePath)
	{
		// 解凍
		String unzipFolder = Common.TempPath() + "\\";
		Directory.CreateDirectory(unzipFolder);
		try
		{
			ZipFile.ExtractToDirectory(archivePath, unzipFolder);
		}
		catch (Exception ex)
		{
			throw new Exception(Localize.Cfm2SettingsNavigationMaintenancePageViewModel_Error_LoadSettingsArchiveAsync_CannotLoad.Localized(), ex);
		}
		unzipFolder += Common.AppId() + "\\";

		// 設定類
		RestoreFiles(unzipFolder, Common.FILE_EXT_JSON);

		// ログは復元しない

		// 環境設定読み込み
		Cfm2Common.LoadCfm2Settings();
	}

	/// <summary>
	/// テンポラリフォルダー内の指定拡張子のファイルを設定フォルダーにコピー
	/// </summary>
	/// <param name="ext"></param>
	/// <param name="tempFolderPath"></param>
	private static void RestoreFiles(String tempFolderPath, String ext)
	{
		String[] files = Directory.GetFiles(tempFolderPath, "*" + ext, SearchOption.AllDirectories);
		foreach (String file in files)
		{
			String subfolderAndLeaf = file[tempFolderPath.Length..];
			String? subfolder = Path.GetDirectoryName(subfolderAndLeaf);
			if (!String.IsNullOrEmpty(subfolder))
			{
				try
				{
					Directory.CreateDirectory(CommonWindows.SettingsFolder() + subfolder);
				}
				catch (Exception)
				{
				}
			}
			File.Copy(file, CommonWindows.SettingsFolder() + subfolderAndLeaf, true);
		}
	}
}
