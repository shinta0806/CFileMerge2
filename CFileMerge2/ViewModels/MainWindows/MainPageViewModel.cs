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
using System.Text.RegularExpressions;
using System.Windows.Input;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views.AboutWindows;
using CFileMerge2.Views.Cfm2SettingsWindows;
using CFileMerge2.Views.MainWindows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Hnx8.ReadJEnc;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Shinta;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

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
		MenuFlyoutItemSampleFolderClickedCommand = new RelayCommand(MenuFlyoutItemSampleFolderClicked);
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

	/// <summary>
	/// 進捗（0～1 の小数）
	/// </summary>
	private Double _progressValue;
	public Double ProgressValue
	{
		get => _progressValue;
		set => SetProperty(ref _progressValue, value);
	}

	// --------------------------------------------------------------------
	// 一般のプロパティー
	// --------------------------------------------------------------------

	// --------------------------------------------------------------------
	// コマンド
	// --------------------------------------------------------------------

	#region 最近使用したメイクファイルフライアウトの制御
	public ICommand MenuFlyoutItemRecentMakeClickedCommand
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
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_MenuFlyoutItemRecentMakeClicked_Error".ToLocalized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 参照ボタンの制御
	public ICommand ButtonBrowseMakeClickedCommand
	{
		get;
	}

	private async void ButtonBrowseMakeClicked()
	{
		try
		{
#if false
            OpenFileDialog openFileDialog = new()
            {
                Title = "my title",
                Filter = "メイクファイル|*.cfm2|すべてのファイル|*.*",
            };
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            MakePath = openFileDialog.FileName;
#endif
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
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_ButtonBrowseMakeClicked_Error".ToLocalized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 環境設定ボタンの制御
	public ICommand ButtonCfm2SettingsClickedCommand
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
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_ButtonCfm2SettingsClicked_Error".ToLocalized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region ヘルプフライアウトの制御
#pragma warning disable CA1822
	public ICommand MenuFlyoutItemHelpClickedCommand
	{
		get => _mainWindow.HelpClickedCommand;
	}
#pragma warning restore CA1822
	#endregion

	#region サンプルフォルダーフライアウトの制御
	public ICommand MenuFlyoutItemSampleFolderClickedCommand
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
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_MenuFlyoutItemSampleFolderClicked_Error".ToLocalized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region バージョン情報フライアウトの制御
	public ICommand MenuFlyoutItemAboutClickedCommand
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
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_MenuFlyoutItemAboutClicked_Error".ToLocalized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 出力ファイルを開くボタンの制御
	public ICommand ButtonOpenOutFileClickedCommand
	{
		get;
	}

	private async void ButtonOpenOutFileClicked()
	{
		try
		{
			Boolean open = true;

			if (String.IsNullOrEmpty(_mergeInfo.OutFullPath))
			{
				open = await Task.Run(async () =>
				{
					try
					{
						Debug.Assert(!_progress, "ButtonOpenOutFileClicked() 既に実行中");
						Log.Information("出力ファイルを開きます：" + MakePath);
						ShowProgressArea();
						MergeCore();
#if DEBUGz
                        Thread.Sleep(3 * 1000);
#endif
						return true;
					}
					catch (OperationCanceledException)
					{
						Log.Information("出力ファイルを開くのを中止しました。");
						return false;
					}
					catch (Exception ex)
					{
						await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_OpenOutFile_Error".ToLocalized() + "\n" + ex.Message);
						SerilogUtils.LogStackTrace(ex);
						return false;
					}
					finally
					{
						HideProgressArea();
					}
				});
			}

			if (open)
			{
				Common.ShellExecute(_mergeInfo.OutFullPath);
			}
		}
		catch (Exception ex)
		{
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_ButtonOpenOutFileClicked_Error".ToLocalized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region スタートボタンの制御
	public ICommand ButtonGoClickedCommand
	{
		get;
	}

	private async void ButtonGoClicked() => await MergeAsync();
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
			await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_MenuFlyoutRecentMakeOpening_Error".ToLocalized() + "\n" + ex.Message);
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
	// private 定数
	// ====================================================================

	/// <summary>
	/// アンカーファイルの出力先フォルダー
	/// </summary>
	private const String ANCHOR_OUT_DEFAULT = "HelpParts";

	// ====================================================================
	// private 変数
	// ====================================================================

	/// <summary>
	/// メインウィンドウ
	/// </summary>
	private readonly MainWindow _mainWindow;

	/// <summary>
	/// 合併作業用の情報
	/// </summary>
	private MergeInfo _mergeInfo = new();

	/// <summary>
	/// 処理中
	/// </summary>
	private Boolean _progress;

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
	private async void AppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
	{
		// await を待つようにするため、いったんキャンセル
		args.Cancel = true;

		if (_progress)
		{
			// 合併中の場合は確認
			MessageDialog messageDialog = _mainWindow.CreateMessageDialog("MainPageViewModel_AppWindowClosing_Confirm".ToLocalized(), Common.LK_GENERAL_LABEL_CONFIRM.ToLocalized());
			messageDialog.Commands.Add(new UICommand(Common.LK_GENERAL_LABEL_YES.ToLocalized()));
			messageDialog.Commands.Add(new UICommand(Common.LK_GENERAL_LABEL_NO.ToLocalized()));
			IUICommand cmd = await messageDialog.ShowAsync();
			if (cmd.Label != Common.LK_GENERAL_LABEL_YES.ToLocalized())
			{
				// キャンセルが確定
				return;
			}
		}

		// 終了処理
		Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Cancel();
		Cfm2Model.Instance.EnvModel.Cfm2Settings.MakePath = MakePath;
		Cfm2Common.SaveCfm2Settings();

		while (_progress)
		{
			// 合併中の場合は終了を待つ
			await Task.Delay(Common.GENERAL_SLEEP_TIME);
		}

		Common.DeleteTempFolder();
		Log.Information("終了しました：" + Common.LK_GENERAL_APP_NAME.ToLocalized() + " " + Cfm2Constants.APP_VER + " --------------------");

		// 改めて閉じる
		_mainWindow.Close();
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
	/// バージョン更新時の処理
	/// </summary>
	private async Task DoVerChangedIfNeededAsync()
	{
		// 更新起動時とパス変更時の記録
		// 新規起動時は、両フラグが立つのでダブらないように注意
		String prevLaunchVer = Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchVer;
		Boolean verChanged = prevLaunchVer != Cfm2Constants.APP_VER;
		if (verChanged)
		{
			// ユーザーにメッセージ表示する前にログしておく
			if (String.IsNullOrEmpty(prevLaunchVer))
			{
				Log.Information("新規起動：" + Cfm2Constants.APP_VER);
			}
			else
			{
				Log.Information("更新起動：" + prevLaunchVer + "→" + Cfm2Constants.APP_VER);
			}
		}
		String prevLaunchPath = Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchPath;
		Boolean pathChanged = (String.Compare(prevLaunchPath, Cfm2Model.Instance.EnvModel.ExeFullPath, true) != 0);
		if (pathChanged && !String.IsNullOrEmpty(prevLaunchPath))
		{
			Log.Information("パス変更起動：" + prevLaunchPath + "→" + Cfm2Model.Instance.EnvModel.ExeFullPath);
		}

		// 更新起動時とパス変更時の処理
		if (verChanged || pathChanged)
		{
			Cfm2Common.LogEnvironmentInfo();
		}
		if (verChanged)
		{
			await NewVersionLaunchedAsync();
		}
	}

	/// <summary>
	/// Anchor タグを実行
	/// </summary>
	private void ExecuteCfmTagAnchorPath(LinkedListNode<String> line, Int32 column)
	{
		_mergeInfo.AnchorPositions.Add(new(line, column));
	}

	/// <summary>
	/// GenerateAnchorFiles タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	private void ExecuteCfmTagGenerateAnchorFiles(CfmTagInfo tagInfo)
	{
		String[] tagValues = tagInfo.Value.Split(',');
		for (Int32 i = 0; i < tagValues.Length; i++)
		{
			tagValues[i] = tagValues[i].Trim();
		}

		// アンカーメイクファイル
		_mergeInfo.AnchorMakeFullPath = GetPathByMakeFullPath(tagValues[0], String.Format("MainPageViewModel_SrcName_AnchorMakeFile".ToLocalized(), TagKey.GenerateAnchorFiles.ToString()));

		// アンカー出力先
		String outSrc;
		if (tagValues.Length < 2)
		{
			// デフォルトの出力先
			outSrc = ANCHOR_OUT_DEFAULT;
		}
		else
		{
			if (String.IsNullOrEmpty(tagValues[1]))
			{
				// デフォルトの出力先
				outSrc = ANCHOR_OUT_DEFAULT;
			}
			else
			{
				// タグ値
				outSrc = tagValues[1];
			}
		}
		_mergeInfo.AnchorOutFullFolder = GetPath(outSrc, Path.GetDirectoryName(_mergeInfo.OutFullPath) ?? String.Empty, "MainPageViewModel_SrcName_AnchorOutFolder".ToLocalized()) + "\\";

		// アンカーファイル作成対象
		if (tagValues.Length >= 3)
		{
			_mergeInfo.AnchorTargets = GetTargetRanks(tagValues[2], _mergeInfo.AnchorTargets);
		}
	}

	/// <summary>
	/// Include タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <param name="parentLines">親となる行群</param>
	/// <param name="parentLine">挿入位置</param>
	/// <exception cref="Exception"></exception>
	private void ExecuteCfmTagInclude(CfmTagInfo tagInfo, LinkedList<String> parentLines, LinkedListNode<String> parentLine)
	{
		// インクルードパスを取得（IncludeFolder を加味する必要があるため GetPath() は使えない）
		String path;
		if (String.IsNullOrEmpty(tagInfo.Value))
		{
			throw new Exception(String.Format("MainPageViewModel_ExecuteCfmTagInclude_Error_NoPath".ToLocalized(), Cfm2Constants.CFM_TAG_KEYS[(Int32)tagInfo.Key]));
		}
		if (Path.IsPathRooted(tagInfo.Value))
		{
			// 絶対パス
			path = tagInfo.Value;
		}
		else
		{
			// インクルードフォルダーからの相対パス
			Debug.Assert(!String.IsNullOrEmpty(_mergeInfo.IncludeFullFolder), "ExecuteTagInclude() IncludeFolder が初期化されていない");
			path = Path.GetFullPath(tagInfo.Value, _mergeInfo.IncludeFullFolder);
		}
		if (!Path.HasExtension(path))
		{
			path += _mergeInfo.IncludeDefaultExt;
		}

		// インクルード
		ParseFile(path, parentLines, parentLine);
	}

	/// <summary>
	/// IncludeDefaultExt タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	private void ExecuteCfmTagIncludeDefaultExt(CfmTagInfo tagInfo)
	{
		_mergeInfo.IncludeDefaultExt = tagInfo.Value;
		if (!String.IsNullOrEmpty(_mergeInfo.IncludeDefaultExt) && _mergeInfo.IncludeDefaultExt[0] != '.')
		{
			_mergeInfo.IncludeDefaultExt = '.' + _mergeInfo.IncludeDefaultExt;
		}
	}

	/// <summary>
	/// IncludeFolder タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <exception cref="Exception"></exception>
	private void ExecuteCfmTagIncludeFolder(CfmTagInfo tagInfo)
	{
		_mergeInfo.IncludeFullFolder = GetPathByMakeFullPath(tagInfo);
		if (!Directory.Exists(_mergeInfo.IncludeFullFolder))
		{
			// 続行可能といえば続行可能であるが、Include できない際に原因が分かりづらくなるのでここでエラーとする
			throw new Exception("MainPageViewModel_ExecuteCfmTagIncludeFolder_Error_NoExist".ToLocalized() + "\n" + tagInfo.Value);
		}
	}

	/// <summary>
	/// OutFile タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <exception cref="Exception"></exception>
	private void ExecuteCfmTagOutFile(CfmTagInfo tagInfo)
	{
		_mergeInfo.OutFullPath = GetPathByMakeFullPath(tagInfo);
		Debug.WriteLine("ExexuteTagOutFile() " + _mergeInfo.OutFullPath);
		if (String.Compare(_mergeInfo.OutFullPath, _mergeInfo.MakeFullPath, true) == 0)
		{
			throw new Exception("MainPageViewModel_ExecuteCfmTagOutFile_Error_SameFile".ToLocalized());
		}
	}

	/// <summary>
	/// Set タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	private void ExecuteCfmTagSet(CfmTagInfo tagInfo)
	{
		// 変数名と変数値に分割
		Int32 eqPos = tagInfo.Value.IndexOf('=');
		if (eqPos < 0)
		{
			_mergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagSet_Warning_NoEq".ToLocalized() + tagInfo.Value);
			return;
		}

		String varName = tagInfo.Value[0..eqPos].Trim().ToLower();
		if (String.IsNullOrEmpty(varName))
		{
			_mergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagSet_Warning_NoName".ToLocalized() + tagInfo.Value);
			return;
		}

		String varValue = tagInfo.Value[(eqPos + 1)..].Trim();

		_mergeInfo.Vars[varName] = varValue;
	}

	/// <summary>
	/// Toc タグを実行
	/// </summary>
	private void ExecuteCfmTagToc(CfmTagInfo tagInfo)
	{
		// 実際に目次を作成するのは、メイクファイルをすべて読み込んだ後なので、ここでは必要性の記録に留める
		_mergeInfo.TocNeeded = true;
		_mergeInfo.TocTargets = GetTargetRanks(tagInfo.Value, _mergeInfo.TocTargets);
	}

	/// <summary>
	/// Var タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <param name="line">タグのある行</param>
	/// <param name="column">タグのある桁</param>
	private void ExecuteCfmTagVar(CfmTagInfo tagInfo, LinkedListNode<String> line, Int32 column)
	{
		// 変数名取得
		String varName = tagInfo.Value.Trim().ToLower();
		if (String.IsNullOrEmpty(varName))
		{
			_mergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagVar_Warning_NoName".ToLocalized());
			return;
		}
		if (!_mergeInfo.Vars.ContainsKey(varName))
		{
			_mergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagVar_Warning_NoDeclare".ToLocalized() + tagInfo.Value);
			return;
		}

		line.Value = line.Value.Insert(column, _mergeInfo.Vars[varName]);
	}

	/// <summary>
	/// フルパスを取得（絶対パスまたは相対パスより）
	/// </summary>
	/// <param name="path">絶対パスまたは相対パス</param>
	/// <param name="basePath">相対パスの基準フォルダー</param>
	/// <param name="srcName">記述元（エラー表示用）</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private static String GetPath(String path, String basePath, String srcName)
	{
		Log.Debug("GetPath() srcName: " + srcName);
		if (String.IsNullOrEmpty(path))
		{
			throw new Exception(String.Format("MainPageViewModel_GetPath_Error_NoPath".ToLocalized(), srcName));
		}
		if (Path.IsPathRooted(path))
		{
			// 絶対パス
			return path;
		}
		else
		{
			// 相対パス
			return Path.GetFullPath(path, basePath);
		}
	}

	/// <summary>
	/// フルパスを取得（絶対パスまたはメイクファイルからの相対パスより）
	/// </summary>
	/// <param name="path">絶対パスまたはメイクファイルからの相対パス</param>
	/// <param name="srcName">記述元（エラー表示用）</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private String GetPathByMakeFullPath(String path, String srcName)
	{
		return GetPath(path, Path.GetDirectoryName(_mergeInfo.MakeFullPath) ?? String.Empty, srcName);
	}

	/// <summary>
	/// タグの値からフルパスを取得（絶対パスまたはメイクファイルからの相対パスより）
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private String GetPathByMakeFullPath(CfmTagInfo tagInfo)
	{
		return GetPathByMakeFullPath(tagInfo.Value, String.Format("MainPageViewModel_SrcName_Tag".ToLocalized(), Cfm2Constants.CFM_TAG_KEYS[(Int32)tagInfo.Key]));
	}

	/// <summary>
	/// Hx タグの対象ランクを取得
	/// </summary>
	/// <param name="rankString">対象ランクを列挙した文字列（例："13" なら h1 と h3 が対象）</param>
	/// <param name="defaultTargetRanks">取得不可の場合に返すデフォルト対象ランク</param>
	/// <returns></returns>
	private static Boolean[] GetTargetRanks(String rankString, Boolean[] defaultTargetRanks)
	{
		if (String.IsNullOrEmpty(rankString))
		{
			return defaultTargetRanks;
		}

		Boolean[] targetRanks = new Boolean[Cfm2Constants.HX_TAG_RANK_MAX + 1];
		for (Int32 i = 0; i < rankString.Length; i++)
		{
			if (Int32.TryParse(rankString[i..(i + 1)], out Int32 rank) && 0 < rank && rank <= Cfm2Constants.HX_TAG_RANK_MAX)
			{
				targetRanks[rank] = true;
			}
		}

		if (!targetRanks.Contains(true))
		{
			return defaultTargetRanks;
		}

		return targetRanks;
	}

	/// <summary>
	/// プログレスエリア非表示
	/// </summary>
	private void HideProgressArea()
	{
		_mainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
		{
			_progress = false;
			_mainWindow.RemoveVeil();
		});
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

			if (Path.GetExtension(cmd).ToLower() == Cfm2Constants.FILE_EXT_CFM2_MAKE)
			{
				// プロジェクトファイルが exe にドロップされた場合
				return cmd;
			}
		}

		return null;
	}

	/// <summary>
	/// 合併メインルーチン
	/// 続行不可能なエラーは直ちに Exception で中止する
	/// 続行可能なエラーは MergeInfo.Errors に貯めて最後に表示する
	/// </summary>
	/// <returns></returns>
	private async Task MergeAsync()
	{
		await Task.Run(async () =>
		{
			try
			{
				Debug.Assert(!_progress, "MergeAsync() 既に実行中");
				Log.Information("合併を開始します：" + MakePath);
				ShowProgressArea();
				Int32 startTick = Environment.TickCount;
				MergeCore();

				// 目次作成
				SetProgressValue(MergeStep.InsertToc, 0.0);
				if (_mergeInfo.TocNeeded)
				{
					ParseCfmTagsForToc();
				}

				// 出力
				SetProgressValue(MergeStep.Output, 0.0);
				Directory.CreateDirectory(Path.GetDirectoryName(_mergeInfo.OutFullPath) ?? String.Empty);
				Write(_mergeInfo.OutFullPath, _mergeInfo.Lines);

				// アンカー出力
				SetProgressValue(MergeStep.OutputAnchor, 0.0);
				if (!String.IsNullOrEmpty(_mergeInfo.AnchorMakeFullPath))
				{
					OutputAnchors();
				}
				SetProgressValue(MergeStep.OutputAnchor, 100.0);

#if DEBUGz
                Thread.Sleep(5 * 1000);
#endif

				// 最近使用したメイクファイル追加
				AddRecent(_mergeInfo.MakeFullPath);

				// 報告
				if (_mergeInfo.Warnings.Any())
				{
					// 警告あり
					String message = "MainPageViewModel_MergeAsync_Warnings".ToLocalized() + "\n";
					for (Int32 i = 0; i < _mergeInfo.Warnings.Count; i++)
					{
						message += _mergeInfo.Warnings[i] + "\n";
					}
					await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Warning, message);
				}
				else
				{
					// 完了
					await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Information,
							String.Format("MainPageViewModel_MergeAsync_Done".ToLocalized(), (Environment.TickCount - startTick).ToString("#,0")));
				}
			}
			catch (OperationCanceledException)
			{
				Log.Information("合併を中止しました。");
			}
			catch (Exception ex)
			{
				await _mainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "MainPageViewModel_MergeAsync_Error".ToLocalized() + "\n" + ex.Message);
				SerilogUtils.LogStackTrace(ex);
			}
			finally
			{
				HideProgressArea();
			}
		});
	}

	/// <summary>
	/// 合併コア
	/// 出力は行わない
	/// </summary>
	private void MergeCore()
	{
		_mergeInfo = new();

		// デフォルト値を設定
		_mergeInfo.MakeFullPath = Path.GetFullPath(MakePath, Cfm2Model.Instance.EnvModel.ExeFullFolder);
		_mergeInfo.IncludeFullFolder = Path.GetDirectoryName(_mergeInfo.MakeFullPath) ?? String.Empty;
		_mergeInfo.OutFullPath = Path.GetDirectoryName(_mergeInfo.MakeFullPath) + "\\" + Path.GetFileNameWithoutExtension(_mergeInfo.MakeFullPath) + "Output" + Common.FILE_EXT_HTML;
		for (Int32 i = 0; i < _mergeInfo.TocTargets.Length; i++)
		{
			_mergeInfo.TocTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets[i];
			_mergeInfo.AnchorTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets[i];
		}

		// メイクファイル読み込み（再帰）
		(_mergeInfo.Encoding, _mergeInfo.NewLine) = ParseFile(_mergeInfo.MakeFullPath, _mergeInfo.Lines, null);
	}

	/// <summary>
	/// 新バージョンで初回起動された時の処理を行う
	/// </summary>
	private async Task NewVersionLaunchedAsync()
	{
		String newVerMsg = String.Empty;
		LogEventLevel logEventLevel = LogEventLevel.Information;

		// α・β警告、ならびに、更新時のメッセージ（2023/01/01）
		// 更新のご挨拶
		if (String.IsNullOrEmpty(Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchVer))
		{
			// 新規の場合は原則として挨拶しない
		}
		else
		{
			// 更新
			newVerMsg = String.Format("MainPageViewModel_Updated".ToLocalized(), Common.LK_GENERAL_APP_NAME.ToLocalized());
		}

		// α・βの注意
		if (Cfm2Constants.APP_VER.Contains('α'))
		{
			newVerMsg += "\n\n" + "MainPageViewModel_Warning_Alpha".ToLocalized();
			logEventLevel = LogEventLevel.Warning;
		}
		else if (Cfm2Constants.APP_VER.Contains('β'))
		{
			newVerMsg += "\n\n" + "MainPageViewModel_Warning_Beta".ToLocalized();
			logEventLevel = LogEventLevel.Warning;
		}

		Cfm2Common.SaveCfm2Settings();
		if (String.IsNullOrEmpty(newVerMsg))
		{
			return;
		}

		// 表示
		await _mainWindow.ShowLogMessageDialogAsync(logEventLevel, newVerMsg);
	}

	/// <summary>
	/// アンカーファイル群を出力
	/// </summary>
	private void OutputAnchors()
	{
		// アンカー出力先フォルダー作成
		Directory.CreateDirectory(_mergeInfo.AnchorOutFullFolder);

		// アンカーファイル作成対象の Hx タグを検索
		List<HxTagInfo> hxTagInfos = ParseHxTags(_mergeInfo.AnchorTargets);

		// アンカーメイクファイル読み込み（再帰）
		_mergeInfo.AnchorPositions.Clear();
		LinkedList<String> lines = new();
		ParseFile(_mergeInfo.AnchorMakeFullPath, lines, null);

		// アンカー挿入位置の行番号を計算
		List<KeyValuePair<Int32, Int32>> anchorPositionIndexes = new();
		for (Int32 i = 0; i < _mergeInfo.AnchorPositions.Count; i++)
		{
			Int32 lineIndex = -1;
			LinkedListNode<String>? line = _mergeInfo.AnchorPositions[i].Key;
			while (line != null)
			{
				lineIndex++;
				line = line.Previous;
			}
			anchorPositionIndexes.Add(new(lineIndex, _mergeInfo.AnchorPositions[i].Value));
		}

		// アンカーファイルのフォルダーを基準とした時の出力ファイルの相対パス
		String relativePath = Path.GetRelativePath(_mergeInfo.AnchorOutFullFolder, _mergeInfo.OutFullPath).Replace('\\', '/');

		// アンカーファイル出力
		for (Int32 i = 0; i < hxTagInfos.Count; i++)
		{
			String anchorPath = _mergeInfo.AnchorOutFullFolder + Path.GetFileNameWithoutExtension(_mergeInfo.OutFullPath) + "_" + hxTagInfos[i].Id + Common.FILE_EXT_HTML;
			if (!Cfm2Model.Instance.EnvModel.Cfm2Settings.OverwriteAnchorFiles && File.Exists(anchorPath))
			{
				// 既にアンカーファイルが存在していて上書きしない場合は何もしない
			}
			else
			{
				List<String> anchorFileContents = new(lines);

				// アンカー置換
				for (Int32 j = 0; j < anchorPositionIndexes.Count; j++)
				{
					anchorFileContents[anchorPositionIndexes[j].Key] = anchorFileContents[anchorPositionIndexes[j].Key].Insert(anchorPositionIndexes[j].Value, relativePath + "#" + hxTagInfos[i].Id);
				}

				Write(anchorPath, anchorFileContents);
			}

			if (i % Cfm2Constants.PROGRESS_INTERVAL == 0)
			{
				SetProgressValue(MergeStep.OutputAnchor, (Double)i / hxTagInfos.Count);
#if DEBUGz
                Thread.Sleep(500);
#endif
			}
		}
	}

	/// <summary>
	/// Cfm タグがあれば抽出する
	/// 抽出したタグは line から削除する
	/// </summary>
	/// <param name="line">解析対象行</param>
	/// <param name="column">解析開始桁</param>
	/// <param name="removeTocTag">抽出したタグが Toc タグだった場合に削除するかどうか</param>
	/// <returns>抽出した Cfm タグ, 次の解析開始桁</returns>
	private (CfmTagInfo? tagInfo, Int32 column) ParseCfmTag(LinkedListNode<String> line, Int32 column, Boolean removeTocTag)
	{
		if (column >= line.Value.Length)
		{
			// 行末まで解析した
			return (null, column);
		}

		Match match = Regex.Match(line.Value[column..], @"\<\!\-\-\s*?cfm\/(.+?)\-\-\>", RegexOptions.IgnoreCase);
		if (!match.Success)
		{
			// Cfm タグが無い
			return (null, line.Value.Length);
		}

		Debug.Assert(match.Groups.Count >= 2, "ParseCfmTag() match.Groups が不足");
		Int32 addColumn = match.Index + match.Length;
		String tagContent = match.Groups[1].Value;
		Int32 colon = tagContent.IndexOf(':');
		if (colon < 0)
		{
			// キーと値を区切る ':' が無い
			Log.Debug("ParseTag() 区切りコロン無し, " + tagContent + ", add: " + addColumn);
			_mergeInfo.Warnings.Add("MainPageViewModel_ParseCfmTag_Warning_NoColon".ToLocalized() + tagContent);
			return (null, addColumn);
		}

		Int32 key = Array.IndexOf(Cfm2Constants.CFM_TAG_KEYS, tagContent[0..colon].Trim().ToLower());
		if (key < 0)
		{
			Log.Debug("ParseTag() サポートされていないキー, " + tagContent + ", add: " + addColumn);
			_mergeInfo.Warnings.Add("MainPageViewModel_ParseCfmTag_Warning_NotSupported" + tagContent);
			return (null, addColumn);
		}

		CfmTagInfo tagInfo = new()
		{
			Key = (TagKey)key,
			Value = tagContent[(colon + 1)..].Trim(),
		};

		if (!removeTocTag && tagInfo.Key == TagKey.Toc)
		{
		}
		else
		{
#if DEBUG
			if (tagInfo.Key == TagKey.Var)
			{
			}
#endif
			// タグは出力しないので削除する（Replace だと複数回削除してしまうので Remove）
			line.Value = line.Value.Remove(column + match.Index, match.Length);

			// タグの分、追加位置を差し引く
			addColumn -= match.Length;
		}

		return (tagInfo, addColumn);
	}

	/// <summary>
	/// Cfm タグを解析して内容を更新する（全体用）
	/// </summary>
	/// <param name="lines">解析対象の行群</param>
	/// <param name="startLine">解析開始行</param>
	private void ParseCfmTagsForMain(LinkedList<String> lines, LinkedListNode<String> startLine)
	{
		LinkedListNode<String>? line = startLine;

		// 行をたどるループ
		for (; ; )
		{
			Int32 column = 0;
			LinkedListNode<String>? removeLine = null;

			// 列をたどるループ
			for (; ; )
			{
				(CfmTagInfo? tagInfo, Int32 addColumn) = ParseCfmTag(line, column, false);
				if (tagInfo != null)
				{
					// 有効なタグが見つかった（タグに対応する文字列は削除されている）
					switch (tagInfo.Key)
					{
						case TagKey.OutFile:
							ExecuteCfmTagOutFile(tagInfo);
							break;
						case TagKey.IncludeFolder:
							ExecuteCfmTagIncludeFolder(tagInfo);
							break;
						case TagKey.IncludeDefaultExt:
							ExecuteCfmTagIncludeDefaultExt(tagInfo);
							break;
						case TagKey.Include:
							Int32 prevLines = lines.Count;
							ExecuteCfmTagInclude(tagInfo, lines, line);
							ReserveRemoveIfNeeded(line, ref removeLine);
							Int32 deltaLines = lines.Count - prevLines;
							for (Int32 i = 0; i < deltaLines; i++)
							{
								line = line.Next;
								Debug.Assert(line != null, "ParseCfmTagsForMain() インクルードしているのに line が足りない");
							}
							break;
						case TagKey.Set:
							ExecuteCfmTagSet(tagInfo);
							break;
						case TagKey.Var:
							ExecuteCfmTagVar(tagInfo, line, column + addColumn);
							break;
						case TagKey.Toc:
							ExecuteCfmTagToc(tagInfo);
							break;
						case TagKey.GenerateAnchorFiles:
							ExecuteCfmTagGenerateAnchorFiles(tagInfo);
							break;
						case TagKey.AnchorPath:
							ExecuteCfmTagAnchorPath(line, column + addColumn);
							break;
						default:
							Debug.Assert(false, "ParseCfmTagsForMain() Cfm タグ捕捉漏れ");
							break;
					}
					ReserveRemoveIfNeeded(line, ref removeLine);
				}

				// 解析位置（列）を進める
				column += addColumn;
				if (column >= line.Value.Length)
				{
					break;
				}
			}

			// 次の行へ
			line = line.Next;

			// 予約されている場合は行削除
			// line.Next より後で実行する
			if (removeLine != null)
			{
				lines.Remove(removeLine);
			}

			_mergeInfo.NumProgressLines++;
			if (_mergeInfo.NumProgressLines % Cfm2Constants.PROGRESS_INTERVAL == 0)
			{
				SetProgressValue(MergeStep.ParseFile, (Double)_mergeInfo.NumProgressLines / _mergeInfo.NumTotalLines);
#if DEBUGz
                Thread.Sleep(100);
#endif
			}
			if (line == null)
			{
				break;
			}
			Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}
	}

	/// <summary>
	/// Cfm タグを解析して内容を更新する（目次作成用）
	/// </summary>
	private void ParseCfmTagsForToc()
	{
		LinkedListNode<String>? line = _mergeInfo.Lines.First;
		Debug.Assert(line != null, "ParseHxTags() _mergeInfo.Lines が空");

		// 行をたどるループ
		for (; ; )
		{
			Int32 column = 0;

			// 列をたどるループ
			for (; ; )
			{
				(CfmTagInfo? tagInfo, Int32 addColumn) = ParseCfmTag(line, column, true);
				if (tagInfo != null)
				{
					// 有効なタグが見つかった（タグに対応する文字列は削除されている）
					if (tagInfo.Key == TagKey.Toc)
					{
						StringBuilder stringBuilder = new();
						stringBuilder.Append("<div class=\"" + Cfm2Constants.TOC_AREA_CLASS_NAME + "\">\n");
						List<HxTagInfo> hxTagInfos = ParseHxTags(_mergeInfo.TocTargets);
						for (Int32 i = 0; i < hxTagInfos.Count; i++)
						{
							stringBuilder.Append("  <div class=\"" + Cfm2Constants.TOC_ITEM_CLASS_NAME_PREFIX + hxTagInfos[i].Rank + "\"><a href=\"#" +
									hxTagInfos[i].Id + "\">" + hxTagInfos[i].Caption + "</a></div>\n");
						}
						stringBuilder.Append("</div>");
						line.Value = line.Value.Insert(column + addColumn, stringBuilder.ToString());
					}
				}

				// 解析位置（列）を進める
				column += addColumn;
				if (column >= line.Value.Length)
				{
					break;
				}
			}

			// 次の行へ
			line = line.Next;
			if (line == null)
			{
				break;
			}
		}
	}

	/// <summary>
	/// ファイルの内容を読み込み解析する
	/// 解析後の内容を parentLine の直後に追加（parentLine が null の場合は先頭に追加）
	/// </summary>
	/// <param name="path">読み込むファイルのフルパス</param>
	/// <param name="parentLines">親の行群</param>
	/// <param name="parentLine">読み込んだ内容を挿入する位置</param>
	/// <exception cref="Exception"></exception>
	private (Encoding encoding, String newLine) ParseFile(String path, LinkedList<String> parentLines, LinkedListNode<String>? parentLine)
	{
		if (String.IsNullOrEmpty(path))
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_NotSpecified".ToLocalized());
		}
		if (!File.Exists(path))
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_NoFile".ToLocalized() + "\n" + path);
		}
		if (_mergeInfo.IncludeStack.Any(x => String.Compare(x, path, true) == 0))
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_CyclicInclude".ToLocalized() + "\n" + path);
		}

		// インクルード履歴プッシュ
		_mergeInfo.IncludeStack.Add(path);

		// 文字コード自動判別
		FileInfo fileInfo = new(path);
		using FileReader reader = new(fileInfo);
		Encoding encoding = reader.Read(fileInfo).GetEncoding() ?? throw new Exception("MainPageViewModel_ParseFile_Error_Encoding".ToLocalized() + "\n" + path);

		// 改行コード自動判定
		String newLine;
		if (reader.Text.Contains("\r\n"))
		{
			newLine = "\r\n";
		}
		else if (reader.Text.Contains('\r'))
		{
			newLine = "\r";
		}
		else if (reader.Text.Contains('\n'))
		{
			newLine = "\n";
		}
		else
		{
			newLine = "\r\n";
		}

		// このファイルの階層以下の内容
		String[] childLineStrings = reader.Text.Split(new String[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		if (childLineStrings.Length == 0)
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_Empty".ToLocalized() + "\n" + path);
		}
		_mergeInfo.NumTotalLines += childLineStrings.Length;
		LinkedList<String> childLines = new(childLineStrings);

		// このファイルの階層以下を解析
		Debug.Assert(childLines.First != null, "ParseFile() 内容が空");
		ParseCfmTagsForMain(childLines, childLines.First);

		// 先頭行の追加
		LinkedListNode<String>? childLine = childLines.First;
		if (parentLine == null)
		{
			// 先頭に追加
			parentLine = parentLines.AddFirst(childLine.Value);
		}
		else
		{
			// parentLine に追加
			parentLine = parentLines.AddAfter(parentLine, childLine.Value);
		}
		childLine = childLine.Next;

		// 残りの行の追加
		while (childLine != null)
		{
			parentLine = parentLines.AddAfter(parentLine, childLine.Value);
			childLine = childLine.Next;
		}

		// インクルード履歴ポップ
		Debug.Assert(_mergeInfo.IncludeStack.Last() == path, "ParseFile() インクルード履歴破損");
		_mergeInfo.IncludeStack.RemoveAt(_mergeInfo.IncludeStack.Count - 1);
		Log.Debug("ParseFile() end: " + _mergeInfo.NumProgressLines + " / " + _mergeInfo.NumTotalLines);

		return (encoding, newLine);
	}

	/// <summary>
	/// HTML Hx タグがあれば抽出する
	/// </summary>
	/// <param name="line">解析対象行</param>
	/// <param name="column">解析開始桁</param>
	/// <returns></returns>
	private (Int32 addColumn, HxTagInfo? hxTagInfo) ParseHxTag(LinkedListNode<String> line, Int32 column, Boolean[] targetRanks)
	{
		if (column >= line.Value.Length)
		{
			// 行末まで解析した
			return (column, null);
		}

		Match hxMatch = Regex.Match(line.Value[column..], @"\<h([1-6])\s.+?\>", RegexOptions.IgnoreCase);
		if (!hxMatch.Success)
		{
			// Hx タグが無い
			return (line.Value.Length, null);
		}

		Debug.Assert(hxMatch.Groups.Count >= 2, "ParseHxTag() hxMatch.Groups が不足");
		Int32 addColumn = hxMatch.Index + hxMatch.Length;

		// ランク確認
		_ = Int32.TryParse(hxMatch.Groups[1].Value, out Int32 rank);
		if (rank < Cfm2Constants.HX_TAG_RANK_MIN || rank > Cfm2Constants.HX_TAG_RANK_MAX)
		{
			_mergeInfo.Warnings.Add("MainPageViewModel_ParseHxTag_Warning_HxRank".ToLocalized() + hxMatch.Value);
			return (addColumn, null);
		}
		if (!targetRanks[rank])
		{
			// 環境設定により対象外
			return (addColumn, null);
		}

		// ID 属性を抽出する
		Match idMatch = Regex.Match(hxMatch.Value, @"id\s*?=\s*?" + "\"" + "(.+?)" + "\"", RegexOptions.IgnoreCase);
		if (!idMatch.Success)
		{
			// ID 属性が無い
			_mergeInfo.Warnings.Add("MainPageViewModel_ParseHxTag_Warning_HxId".ToLocalized() + hxMatch.Value);
			return (addColumn, null);
		}
		Debug.Assert(idMatch.Groups.Count >= 2, "ParseHxTag() idMatch.Groups が不足");
		String id = idMatch.Groups[1].Value;

		// 見出しを抽出する
		Int32 captionBeginPos = column + hxMatch.Index + hxMatch.Length;
		Int32 captionEndPos = line.Value.IndexOf("</h", captionBeginPos, StringComparison.OrdinalIgnoreCase);
		if (captionEndPos < 0)
		{
			_mergeInfo.Warnings.Add("MainPageViewModel_ParseHxTag_Warning_HxNotClose".ToLocalized() + hxMatch.Value);
			return (addColumn, null);
		}
		String caption = line.Value[captionBeginPos..captionEndPos].Trim();

		// 目次情報追加
		HxTagInfo hxTagInfo = new()
		{
			Rank = rank,
			Id = id,
			Caption = caption,
		};

		return (addColumn, hxTagInfo);
	}

	/// <summary>
	/// HTML Hx タグを解析して目次情報を収集する
	/// </summary>
	private List<HxTagInfo> ParseHxTags(Boolean[] targetRanks)
	{
		LinkedListNode<String>? line = _mergeInfo.Lines.First;
		Debug.Assert(line != null, "ParseHxTags() _mergeInfo.Lines が空");
		Int32 numProgressLines = 0;
		List<HxTagInfo> hxTagInfos = new();

		// 行をたどるループ
		for (; ; )
		{
			Int32 column = 0;

			// 列をたどるループ
			for (; ; )
			{
				(Int32 addColumn, HxTagInfo? hxTagInfo) = ParseHxTag(line, column, targetRanks);
				if (hxTagInfo != null)
				{
					hxTagInfos.Add(hxTagInfo);
				}

				// 解析位置（列）を進める
				column += addColumn;
				if (column >= line.Value.Length)
				{
					break;
				}
			}

			// 次の行へ
			line = line.Next;
			numProgressLines++;
			if (numProgressLines % Cfm2Constants.PROGRESS_INTERVAL == 0)
			{
				SetProgressValue(MergeStep.InsertToc, (Double)numProgressLines / _mergeInfo.Lines.Count);
#if DEBUGz
                Thread.Sleep(20);
#endif
			}
			if (line == null)
			{
				break;
			}
		}
		return hxTagInfos;
	}

	/// <summary>
	/// 空行なら削除予約
	/// </summary>
	/// <param name="line"></param>
	/// <param name="removeLine"></param>
	private static void ReserveRemoveIfNeeded(LinkedListNode<String> line, ref LinkedListNode<String>? removeLine)
	{
		if (String.IsNullOrEmpty(line.Value))
		{
			removeLine = line;
		}
	}

	/// <summary>
	/// プログレスバーの進捗率を設定
	/// </summary>
	/// <param name="mergeStep">現在のステップ</param>
	/// <param name="currentProgress">現在のステップの中での進捗率</param>
	private void SetProgressValue(MergeStep mergeStep, Double currentProgress)
	{
		Double amount = 0;

		// 今までのステップの作業量
		for (Int32 i = 0; i < (Int32)mergeStep; i++)
		{
			amount += Cfm2Constants.MERGE_STEP_AMOUNT[i];
		}

		// 今のステップの作業量
		amount += Cfm2Constants.MERGE_STEP_AMOUNT[(Int32)mergeStep] * currentProgress;

		// 全作業量
		Int32 total = 0;
		for (Int32 i = 0; i < (Int32)MergeStep.__End__; i++)
		{
			total += Cfm2Constants.MERGE_STEP_AMOUNT[i];
		}

		// 進捗率
		Double progress = amount / total;

		_mainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
		{
			// インクルードにより一時的に進捗率が下がる場合があるが、表示上は下げない
			if (progress > ProgressValue)
			{
				ProgressValue = progress;
			}
		});
	}

	/// <summary>
	/// プログレスエリアを表示
	/// </summary>
	private void ShowProgressArea()
	{
		_mainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
		{
			ProgressValue = 0.0;
			_progress = true;
			_mainWindow.AddVeil("ProgressGrid", this);
		});
	}

	/// <summary>
	/// ファイル出力
	/// </summary>
	private void Write(String path, IEnumerable<String> lines)
	{
		File.WriteAllText(path, String.Join(_mergeInfo.NewLine, lines), _mergeInfo.Encoding);
	}
}
