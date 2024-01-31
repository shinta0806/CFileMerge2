// ============================================================================
// 
// メインビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Text;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Strings;
using CFileMerge2.ViewModels.ProgressWindows;
using CFileMerge2.Views.AboutWindows;
using CFileMerge2.Views.Cfm2SettingsWindows;
using CFileMerge2.Views.MainWindows;
using CFileMerge2.Views.ProgressWindows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Shinta;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CFileMerge2.ViewModels.MainWindows;

public class MainPageViewModel : ObservableRecipient
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public MainPageViewModel(MainWindow mainWindow)
	{
		// チェック
		Debug.Assert(Cfm2Constants.CFM_TAG_KEYS.Length == (Int32)TagKey.__End__, "MainPageViewModel() TAG_KEYS が変");
		Debug.Assert(Cfm2Constants.MERGE_STEP_AMOUNT.Length == (Int32)MergeStep.__End__, "MainPageViewModel() MERGE_STEP_AMOUNT が変");

		// 初期化
		_mainWindow = mainWindow;
		_recentMakeManager = new RecentPathManager(RecentItemType.File, Cfm2Constants.RECENT_MAKE_PATHES_MAX);

		// コマンド
		MenuFlyoutItemRecentMakeClickedCommand = new RelayCommand<String>(MenuFlyoutItemRecentMakeClicked);
		ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClicked);
		ButtonCfm2SettingsClickedCommand = new RelayCommand(ButtonCfm2SettingsClicked);
		MenuFlyoutItemFaqClickedCommand = new RelayCommand(MenuFlyoutItemFaqClicked);
		MenuFlyoutItemSampleFolderClickedCommand = new RelayCommand(MenuFlyoutItemSampleFolderClicked);
		MenuFlyoutItemCreatorSupportClickedCommand = new RelayCommand(MenuFlyoutItemCreatorSupportClicked);
		MenuFlyoutItemFantiaClickedCommand = new RelayCommand(MenuFlyoutItemFantiaClicked);
		MenuFlyoutItemCheckUpdateClickedCommand = new RelayCommand(MenuFlyoutItemCheckUpdateClicked);
		MenuFlyoutItemHistoryClickedCommand = new RelayCommand(MenuFlyoutItemHistoryClicked);
		MenuFlyoutItemAboutClickedCommand = new RelayCommand(MenuFlyoutItemAboutClicked);
		ButtonOpenOutFileClickedCommand = new RelayCommand(ButtonOpenOutFileClicked);
		ButtonGoClickedCommand = new RelayCommand(ButtonGoClicked);

		// イベントハンドラー
		_mainWindow.AppWindow.Closing += AppWindowClosing;
	}

	// ====================================================================
	// public プロパティー
	// ====================================================================

	// --------------------------------------------------------------------
	// View 通信用のプロパティー
	// --------------------------------------------------------------------

	/// <summary>
	/// メイクファイルのパス（相対パスも可）
	/// </summary>
	private String _makePath = String.Empty;
	public String MakePath
	{
		get => _makePath;
		set => SetProperty(ref _makePath, value);
	}

	/// <summary>
	/// 最近使用したメイクファイルのボタンを有効にするか
	/// </summary>
	private Boolean _isRecentMakeEnabled;
	public Boolean IsRecentMakeEnabled
	{
		get => _isRecentMakeEnabled;
		set => SetProperty(ref _isRecentMakeEnabled, value);
	}

	// --------------------------------------------------------------------
	// 一般のプロパティー
	// --------------------------------------------------------------------

	// --------------------------------------------------------------------
	// コマンド
	// --------------------------------------------------------------------

	#region 最近使用したメイクファイルフライアウトの制御
	public RelayCommand<String> MenuFlyoutItemRecentMakeClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemRecentMakeClicked(String? path)
	{
		try
		{
			if (String.IsNullOrEmpty(path))
			{
				return;
			}

			MakePath = path;
			AddRecent(path);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemRecentMakeClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 参照ボタンの制御
	public RelayCommand ButtonBrowseMakeClickedCommand
	{
		get;
	}

	private async void ButtonBrowseMakeClicked()
	{
		try
		{
			FileOpenPicker fileOpenPicker = _mainWindow.CreateOpenFilePicker();
			fileOpenPicker.FileTypeFilter.Add(Cfm2Constants.FILE_EXT_CFM2_MAKE);
			fileOpenPicker.FileTypeFilter.Add("*");

			StorageFile? file = await fileOpenPicker.PickSingleFileAsync();
			if (file == null)
			{
				return;
			}

			MakePath = file.Path;
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_ButtonBrowseMakeClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 環境設定ボタンの制御
	public RelayCommand ButtonCfm2SettingsClickedCommand
	{
		get;
	}

	private async void ButtonCfm2SettingsClicked()
	{
		try
		{
			Cfm2SettingsWindow settingsWindow = new();
			await _mainWindow.ShowDialogAsync(settingsWindow);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_ButtonCfm2SettingsClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region ヘルプフライアウトの制御
#pragma warning disable CA1822
	public RelayCommand<String> MenuFlyoutItemHelpClickedCommand
	{
		get => _mainWindow.HelpClickedCommand;
	}
#pragma warning restore CA1822
	#endregion

	#region よくある質問フライアウトの制御
	public RelayCommand MenuFlyoutItemFaqClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemFaqClicked()
	{
		try
		{
			Common.ShellExecute(Cfm2Constants.URL_FAQ);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemFaqClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region チュートリアルフライアウトの制御
#pragma warning disable CA1822
	public RelayCommand<String> MenuFlyoutItemTutorialClickedCommand
	{
		get => _mainWindow.HelpClickedCommand;
	}
#pragma warning restore CA1822
	#endregion

	#region サンプルフォルダーフライアウトの制御
	public RelayCommand MenuFlyoutItemSampleFolderClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemSampleFolderClicked()
	{
		try
		{
			Common.ShellExecute(Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + Cfm2Constants.FOLDER_NAME_SAMPLE);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemSampleFolderClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region クリエイターサポートフライアウトの制御
	public RelayCommand MenuFlyoutItemCreatorSupportClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemCreatorSupportClicked()
	{
		try
		{
			Common.ShellExecute(Cfm2Constants.AUTHOR_CREATOR_SUPPORT);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemCreatorSupportClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region ファンサイトフライアウトの制御
	public RelayCommand MenuFlyoutItemFantiaClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemFantiaClicked()
	{
		try
		{
			Common.ShellExecute(Cfm2Constants.AUTHOR_FANTIA);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemFantiaClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 更新プログラムの確認フライアウトの制御
	public RelayCommand MenuFlyoutItemCheckUpdateClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemCheckUpdateClicked()
	{
		try
		{
			Common.OpenMicrosoftStore(Cfm2Constants.STORE_PRODUCT_ID);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemCheckUpdateClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 更新履歴フライアウトの制御
	public RelayCommand MenuFlyoutItemHistoryClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemHistoryClicked()
	{
		try
		{
			Common.ShellExecute(Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + Localize.File_HistoryWithoutExtension.Localized() + Common.FILE_EXT_TXT);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemHistoryClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region バージョン情報フライアウトの制御
	public RelayCommand MenuFlyoutItemAboutClickedCommand
	{
		get;
	}

	private async void MenuFlyoutItemAboutClicked()
	{
		try
		{
			AboutWindow aboutWindow = new();
			await _mainWindow.ShowDialogAsync(aboutWindow);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutItemAboutClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 出力ファイルを開くボタンの制御
	public RelayCommand ButtonOpenOutFileClickedCommand
	{
		get;
	}

	private async void ButtonOpenOutFileClicked()
	{
		try
		{
			if (String.IsNullOrEmpty(_outFullPath))
			{
				(ProgressWindow progressWindow, ProgressPageViewModel progressPageViewModel) = CreateProgressWindow(true);
				await _mainWindow.ShowDialogAsync(progressWindow);
				_outFullPath = progressPageViewModel.MergeInfo.OutFullPath;
			}
			if (!String.IsNullOrEmpty(_outFullPath))
			{
				Common.ShellExecute(_outFullPath);
			}
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowExceptionLogMessageDialogAsync(Localize.MainPageViewModel_Error_ButtonOpenOutFileClicked.Localized(), ex);
		}
	}
	#endregion

	#region スタートボタンの制御
	public RelayCommand ButtonGoClickedCommand
	{
		get;
	}

	private async void ButtonGoClicked()
	{
		try
		{
			(ProgressWindow progressWindow, ProgressPageViewModel progressPageViewModel) = CreateProgressWindow(false);
			await _mainWindow.ShowDialogAsync(progressWindow);
			_outFullPath = progressPageViewModel.MergeInfo.OutFullPath;

			// 最近使用したメイクファイル追加
			AddRecent(progressPageViewModel.MergeInfo.MakeFullPath);
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowExceptionLogMessageDialogAsync(Localize.MainPageViewModel_Error_ButtonGoClicked.Localized(), ex);
		}
	}
	#endregion

	// ====================================================================
	// public 関数
	// ====================================================================

	/// <summary>
	/// イベントハンドラー：最近使用したメイクファイルメニューが開く前
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public async void MenuFlyoutRecentMakeOpening(Object sender, Object _)
	{
		try
		{
			MenuFlyout menuFlyout = (MenuFlyout)sender;
			menuFlyout.Items.Clear();

			ReadOnlyCollection<String> recentPathes = _recentMakeManager.RecentPathes();
			foreach (String recentPath in recentPathes)
			{
				MenuFlyoutItem menuFlyoutItem = new()
				{
					Text = Path.GetFileName(recentPath),
					Command = MenuFlyoutItemRecentMakeClickedCommand,
					CommandParameter = recentPath,
				};
				menuFlyout.Items.Add(menuFlyoutItem);
			}
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.MainPageViewModel_Error_MenuFlyoutRecentMakeOpening.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}

	/// <summary>
	/// イベントハンドラー：ページがロードされた
	/// </summary>
	public async void PageLoaded(Object _1, RoutedEventArgs _2)
	{
		try
		{
			Log.Debug("PageLoaded()");
			Initialize();
			ApplySettings();

			// 環境の変化に対応
			await DoVerChangedIfNeededAsync();

			// 最新情報確認
			await CheckRssIfNeededAsync();
		}
		catch (Exception ex)
		{
			// ユーザー起因では発生しないイベントなのでログのみ
			Log.Error("ページロード時エラー：\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}

	/// <summary>
	/// イベントハンドラー：TextBoxMake にドラッグされている
	/// </summary>
	/// <param name="_"></param>
	/// <param name="args"></param>
	public void TextBlockMakeDragOver(Object _, DragEventArgs args)
	{
		if (!args.DataView.Contains(StandardDataFormats.StorageItems))
		{
			return;
		}

		args.AcceptedOperation = DataPackageOperation.Copy;
	}

	/// <summary>
	/// イベントハンドラー：TextBoxMake にドロップされた
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public async void TextBoxMakeDrop(Object _, DragEventArgs args)
	{
		if (!args.DataView.Contains(StandardDataFormats.StorageItems))
		{
			return;
		}

		IReadOnlyList<IStorageItem> storageItems = await args.DataView.GetStorageItemsAsync();
		foreach (IStorageItem storageItem in storageItems)
		{
			if (File.Exists(storageItem.Path))
			{
				// ファイルなら受け入れる（フォルダーは受け入れない）
				MakePath = storageItem.Path;
			}
		}
	}

	// ====================================================================
	// private 変数
	// ====================================================================

	/// <summary>
	/// メインウィンドウ
	/// </summary>
	private readonly MainWindow _mainWindow;

	/// <summary>
	/// 出力ファイル
	/// </summary>
	private String? _outFullPath;

	/// <summary>
	/// 最近使用したメイクファイル
	/// </summary>
	private readonly RecentPathManager _recentMakeManager;

	// ====================================================================
	// private 関数
	// ====================================================================

	/// <summary>
	/// 最近使用したメイクファイルを追加
	/// </summary>
	private void AddRecent(String path)
	{
		_recentMakeManager.Add(path);
		ReadOnlyCollection<String> recentPathes = _recentMakeManager.RecentPathes();
		_mainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
		{
			IsRecentMakeEnabled = recentPathes.Any();
		});
		Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes2 = new(recentPathes);
	}

	/// <summary>
	/// 環境設定を適用
	/// </summary>
	private void ApplySettings()
	{
		String? makePathByCommandLine = MakePathByCommandLine();
		if (String.IsNullOrEmpty(makePathByCommandLine))
		{
			MakePath = Cfm2Model.Instance.EnvModel.Cfm2Settings.MakePath;
		}
		else
		{
			MakePath = makePathByCommandLine;
		}
	}

	/// <summary>
	/// イベントハンドラー：ウィンドウが閉じられようとしている
	/// </summary>
	private void AppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
	{
		try
		{
			// 終了処理
			Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Cancel();
			Cfm2Model.Instance.EnvModel.Cfm2Settings.MakePath = MakePath;
			Cfm2Common.SaveCfm2Settings();

			Common.DeleteTempFolder();
			Log.Information("終了しました：" + Localize.AppInfo_AppName.Localized() + " " + Cfm2Constants.APP_VER + " --------------------");
		}
		catch (Exception ex)
		{
			SerilogUtils.LogException("終了時エラー", ex);
		}
	}

	/// <summary>
	/// 最新情報確認
	/// </summary>
	/// <returns></returns>
	private Task CheckRssIfNeededAsync()
	{
		if (!Cfm2Model.Instance.EnvModel.Cfm2Settings.IsCheckRssNeeded())
		{
			return Task.CompletedTask;
		}
		return Cfm2Common.CheckLatestInfoAsync(false, _mainWindow);
	}

	/// <summary>
	/// 進捗ウィンドウを作成
	/// </summary>
	/// <returns></returns>
	private (ProgressWindow, ProgressPageViewModel) CreateProgressWindow(Boolean onlyCore)
	{
		ProgressWindow progressWindow = new(MakePath, onlyCore);
		return (progressWindow, ((ProgressPage)progressWindow.Content).ViewModel);
	}

	/// <summary>
	/// バージョン更新時の処理
	/// </summary>
	private async Task DoVerChangeedAsync()
	{
		// 保存
		Cfm2Common.SaveCfm2Settings();

		// メッセージ
		LogEventLevel logEventLevel = LogEventLevel.Information;
		String message = String.Empty;

		// α・βの注意
		if (Cfm2Constants.APP_VER.Contains('α'))
		{
			message += Localize.MainPageViewModel_Warning_Alpha.Localized() + "\n\n";
			logEventLevel = LogEventLevel.Warning;
		}
		else if (Cfm2Constants.APP_VER.Contains('β'))
		{
			message += Localize.MainPageViewModel_Warning_Beta.Localized() + "\n\n";
			logEventLevel = LogEventLevel.Warning;
		}

		message += String.Format(Localize.MainPageViewModel_Updated.Localized(), Localize.AppInfo_AppName.Localized());

		// 表示
		await _mainWindow.ShowLogMessageDialogAsync(logEventLevel, message);
	}

	/// <summary>
	/// バージョン更新時の処理
	/// </summary>
	private async Task DoVerChangedIfNeededAsync()
	{
		// ToDo: 初回起動時はα・βの注意が表示されない
		// 更新起動確認
		String prevLaunchVer = Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchVer;
		Boolean verChanged = prevLaunchVer != Cfm2Constants.APP_VER;
		if (verChanged)
		{
			if (String.IsNullOrEmpty(prevLaunchVer))
			{
				Log.Information("新規起動：" + Cfm2Constants.APP_VER);
			}
			else
			{
				Log.Information("更新起動：" + prevLaunchVer + "→" + Cfm2Constants.APP_VER);
				await DoVerChangeedAsync();
			}
		}
	}

	/// <summary>
	/// 初期化
	/// </summary>
	private void Initialize()
	{
#if DEBUG
		_mainWindow.Title = "［デバッグ］" + _mainWindow.Title;
#endif
#if TEST
        Title = "［テスト］" + Title;
#endif

		// 文字コード準備
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		// その他
		_recentMakeManager.SetPathes(Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes2);
		IsRecentMakeEnabled = _recentMakeManager.RecentPathes().Any();
	}

	/// <summary>
	/// コマンドライン引数を解析してメイクファイルのパスを取得
	/// </summary>
	private static String? MakePathByCommandLine()
	{
		String[] cmds = Environment.GetCommandLineArgs();

		for (Int32 i = 1; i < cmds.Length; i++)
		{
			String cmd = cmds[i];

			if (String.Equals(Path.GetExtension(cmd), Cfm2Constants.FILE_EXT_CFM2_MAKE, StringComparison.OrdinalIgnoreCase))
			{
				// プロジェクトファイルが exe にドロップされた場合
				return cmd;
			}
		}

		return null;
	}
}
