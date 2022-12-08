// ============================================================================
// 
// メインビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views.AboutWindows;
using CFileMerge2.Views.Cfm2SettingsWindows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Hnx8.ReadJEnc;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Serilog;
using Serilog.Events;

using Shinta;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

using WinUIEx;

using DragEventArgs = Microsoft.UI.Xaml.DragEventArgs;

namespace CFileMerge2.ViewModels.MainWindows;

public class MainPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public MainPageViewModel()
    {
        // チェック
        Debug.Assert(Cfm2Constants.CFM_TAG_KEYS.Length == (Int32)TagKey.__End__, "MainPageViewModel() TAG_KEYS が変");
        Debug.Assert(Cfm2Constants.MERGE_STEP_AMOUNT.Length == (Int32)MergeStep.__End__, "MainPageViewModel() MERGE_STEP_AMOUNT が変");

        // コマンド
        MenuFlyoutItemRecentMakeClickedCommand = new RelayCommand<String>(MenuFlyoutItemRecentMakeClicked);
        ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClicked);
        ButtonCfm2SettingsClickedCommand = new RelayCommand(ButtonCfm2SettingsClicked);
        MenuFlyoutItemSampleFolderClickedCommand = new RelayCommand(MenuFlyoutItemSampleFolderClicked);
        MenuFlyoutItemAboutClickedCommand = new RelayCommand(MenuFlyoutItemAboutClicked);
        ButtonOpenOutFileClickedCommand = new RelayCommand(ButtonOpenOutFileClicked);
        ButtonGoClickedCommand = new RelayCommand(ButtonGoClicked);

        // イベントハンドラー
        App.MainWindow.AppWindow.Closing += AppWindowClosing;
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
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "最近使用したメイクファイル適用時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
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
            FileOpenPicker fileOpenPicker = App.MainWindow.CreateOpenFilePicker();
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
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "参照時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
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
            await App.MainWindow.ShowDialogAsync(settingsWindow);
        }
        catch (Exception ex)
        {
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "環境設定表示時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion

    #region ヘルプフライアウトの制御
    public ICommand MenuFlyoutItemHelpClickedCommand
    {
        get => App.MainWindow.HelpClickedCommand;
    }
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
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "サンプルフォルダー表示時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
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
            await App.MainWindow.ShowDialogAsync(aboutWindow);
        }
        catch (Exception ex)
        {
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "バージョン情報表示時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
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
                        await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "出力ファイルを開く処理時エラー：\n" + ex.Message);
                        Log.Information("スタックトレース：\n" + ex.StackTrace);
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
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "出力ファイルを開く時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
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
            foreach (String path in Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes)
            {
                MenuFlyoutItem menuFlyoutItem = new()
                {
                    Text = Path.GetFileName(path),
                    Command = MenuFlyoutItemRecentMakeClickedCommand,
                    CommandParameter = path,
                };
                menuFlyout.Items.Add(menuFlyoutItem);
            }
        }
        catch (Exception ex)
        {
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "最近使用したメイクファイル時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
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

            // 最新情報確認
            await CheckRssIfNeededAsync();
        }
        catch (Exception ex)
        {
            // ユーザー起因では発生しないイベントなのでログのみ
            Log.Error("ページロード時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
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
    /// 合併作業用の情報
    /// </summary>
    private MergeInfo _mergeInfo = new();

    /// <summary>
    /// 処理中
    /// </summary>
    private Boolean _progress;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 最近使用したメイクファイルを追加
    /// </summary>
    private void AddRecent(String path)
    {
        if (Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes.Contains(path))
        {
            // 既に追加されている場合はいったん削除
            Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes.Remove(path);
        }

        // 先頭に追加
        Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes.Insert(0, path);
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            IsRecentMakeEnabled = true;
        });

        // 溢れた分は削除
        if (Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes.Count > Cfm2Constants.RECENT_MAKE_PATHES_MAX)
        {
            Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes.RemoveRange(Cfm2Constants.RECENT_MAKE_PATHES_MAX,
                    Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes.Count - Cfm2Constants.RECENT_MAKE_PATHES_MAX);
        }
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
            MessageDialog messageDialog = App.MainWindow.CreateMessageDialog("合併作業中です。\n終了してもよろしいですか？", Cfm2Constants.LABEL_CONFIRM);
            messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_YES));
            messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_NO));
            IUICommand cmd = await messageDialog.ShowAsync();
            if (cmd.Label != Cfm2Constants.LABEL_YES)
            {
                // キャンセルが確定
                return;
            }
        }

        // 終了処理
        Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Cancel();
        Cfm2Model.Instance.EnvModel.Cfm2Settings.MakePath = MakePath;
        Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchVer = Cfm2Constants.APP_VER;
        Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchPath = Cfm2Model.Instance.EnvModel.ExeFullPath;
        await Cfm2Model.Instance.EnvModel.SaveCfm2SettingsAsync();

        while (_progress)
        {
            // 合併中の場合は終了を待つ
            await Task.Delay(Common.GENERAL_SLEEP_TIME);
        }

        Common.DeleteTempFolder();
        Log.Information("終了しました：" + Cfm2Constants.APP_NAME_J + " " + Cfm2Constants.APP_VER + " --------------------");

        // 改めて閉じる
        App.MainWindow.Close();
    }

    /// <summary>
    /// 最新情報確認
    /// </summary>
    /// <returns></returns>
    private static Task CheckRssIfNeededAsync()
    {
        if (!Cfm2Model.Instance.EnvModel.Cfm2Settings.IsCheckRssNeeded())
        {
            return Task.CompletedTask;
        }
        return Cfm2Common.CheckLatestInfoAsync(false, App.MainWindow);
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
        _mergeInfo.AnchorMakeFullPath = GetPathByMakeFullPath(tagValues[0], TagKey.GenerateAnchorFiles.ToString() + " タグのアンカーメイクファイル");

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
        _mergeInfo.AnchorOutFullFolder = GetPath(outSrc, Path.GetDirectoryName(_mergeInfo.OutFullPath) ?? String.Empty, "アンカー出力先フォルダー") + "\\";

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
            throw new Exception(Cfm2Constants.CFM_TAG_KEYS[(Int32)tagInfo.Key] + " タグのパスが指定されていません。");
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
        Debug.WriteLine("ExecuteTagInclude() " + path);

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
        Debug.WriteLine("ExecuteTagIncludeDefaultExt() " + _mergeInfo.IncludeDefaultExt);
    }

    /// <summary>
    /// IncludeFolder タグを実行
    /// </summary>
    /// <param name="tagInfo">タグ情報</param>
    /// <exception cref="Exception"></exception>
    private void ExecuteCfmTagIncludeFolder(CfmTagInfo tagInfo)
    {
        _mergeInfo.IncludeFullFolder = GetPathByMakeFullPath(tagInfo);
        Debug.WriteLine("ExecuteTagIncludeFolder() " + _mergeInfo.IncludeFullFolder);
        if (!Directory.Exists(_mergeInfo.IncludeFullFolder))
        {
            // 続行可能といえば続行可能であるが、Include できない際に原因が分かりづらくなるのでここでエラーとする
            throw new Exception("インクルードフォルダーが存在しません：\n" + tagInfo.Value);
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
            throw new Exception("出力先ファイルがメイクファイルと同じです。");
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
            _mergeInfo.Warnings.Add("Set タグが「変数名 = 変数値」の形式になっていません：" + tagInfo.Value);
            return;
        }

        String varName = tagInfo.Value[0..eqPos].Trim().ToLower();
        if (String.IsNullOrEmpty(varName))
        {
            _mergeInfo.Warnings.Add("Set タグの変数名が指定されていません：" + tagInfo.Value);
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
            _mergeInfo.Warnings.Add("Var タグの変数名が指定されていません。");
            return;
        }
        if (!_mergeInfo.Vars.ContainsKey(varName))
        {
            _mergeInfo.Warnings.Add("Var タグで指定された変数名が Set タグで宣言されていません：" + tagInfo.Value);
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
    private String GetPath(String path, String basePath, String srcName)
    {
        if (String.IsNullOrEmpty(path))
        {
            throw new Exception(srcName + "のパスが指定されていません。");
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
        return GetPathByMakeFullPath(tagInfo.Value, Cfm2Constants.CFM_TAG_KEYS[(Int32)tagInfo.Key] + " タグ");
    }

    /// <summary>
    /// Hx タグの対象ランクを取得
    /// </summary>
    /// <param name="rankString">対象ランクを列挙した文字列（例："13" なら h1 と h3 が対象）</param>
    /// <param name="defaultTargetRanks">取得不可の場合に返すデフォルト対象ランク</param>
    /// <returns></returns>
    private Boolean[] GetTargetRanks(String rankString, Boolean[] defaultTargetRanks)
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
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            _progress = false;
            App.MainWindow.RemoveVeil();
            Debug.WriteLine("HideProgressArea() " + _progress);
        });
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
#if DEBUG
        App.MainWindow.Title = "［デバッグ］" + App.MainWindow.Title;
#endif
#if TEST
        Title = "［テスト］" + Title;
#endif

        // 文字コード準備
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // その他
        IsRecentMakeEnabled = Cfm2Model.Instance.EnvModel.Cfm2Settings.RecentMakePathes.Any();
    }

    /// <summary>
    /// コマンドライン引数を解析してメイクファイルのパスを取得
    /// </summary>
    private String? MakePathByCommandLine()
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
                    String message = "警告があります。\n";
                    for (Int32 i = 0; i < _mergeInfo.Warnings.Count; i++)
                    {
                        message += _mergeInfo.Warnings[i] + "\n";
                    }
                    await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Warning, message);
                }
                else
                {
                    // 完了
                    await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Information, "完了しました。\n経過時間：" + (Environment.TickCount - startTick).ToString("#,0") + " ミリ秒");
                }
            }
            catch (OperationCanceledException)
            {
                Log.Information("合併を中止しました。");
            }
            catch (Exception ex)
            {
                await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "合併時エラー：\n" + ex.Message);
                Log.Information("スタックトレース：\n" + ex.StackTrace);
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
            Debug.WriteLine("ParseTag() 区切りコロン無し, " + tagContent + ", add: " + addColumn);
            _mergeInfo.Warnings.Add("Cfm タグに区切りコロンがありません：" + tagContent);
            return (null, addColumn);
        }

        Int32 key = Array.IndexOf(Cfm2Constants.CFM_TAG_KEYS, tagContent[0..colon].Trim().ToLower());
        if (key < 0)
        {
            Debug.WriteLine("ParseTag() サポートされていないキー, " + tagContent + ", add: " + addColumn);
            _mergeInfo.Warnings.Add("サポートされていない Cfm タグです：" + tagContent);
            return (null, addColumn);
        }

        CfmTagInfo tagInfo = new()
        {
            Key = (TagKey)key,
            Value = tagContent[(colon + 1)..].Trim(),
        };
        Debug.WriteLine("ParseTag() [" + tagInfo.Key + "], [" + tagInfo.Value + "] add: " + addColumn);

        if (!removeTocTag && tagInfo.Key == TagKey.Toc)
        {
        }
        else
        {
            // タグは出力しないので削除する
            line.Value = line.Value.Replace(match.Value, null);

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
            throw new Exception("ファイルが指定されていません。");
        }
        if (!File.Exists(path))
        {
            throw new Exception("ファイルが見つかりません：\n" + path);
        }
        if (_mergeInfo.IncludeStack.Any(x => String.Compare(x, path, true) == 0))
        {
            throw new Exception("インクルードが循環しています：\n" + path);
        }

        // インクルード履歴プッシュ
        _mergeInfo.IncludeStack.Add(path);

        // 文字コード自動判別
        FileInfo fileInfo = new(path);
        using FileReader reader = new(fileInfo);
        Encoding encoding = reader.Read(fileInfo).GetEncoding();
        if (encoding == null)
        {
            throw new Exception("文字コードを判定できませんでした：\n" + path);
        }

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
            throw new Exception("内容が空です：\n" + path);
        }
        _mergeInfo.NumTotalLines += childLineStrings.Length;
        LinkedList<String> childLines = new(childLineStrings);

        // このファイルの階層以下を解析
        Debug.Assert(childLines.First != null, "ParseFile() 内容が空");
        ParseCfmTagsForMain(childLines, childLines.First);
        Debug.WriteLine("ParseFile() chileLines: " + childLines.Count + ", " + path);

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
        Debug.WriteLine("ParseFile() end: " + _mergeInfo.NumProgressLines + " / " + _mergeInfo.NumTotalLines);

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
        Int32.TryParse(hxMatch.Groups[1].Value, out Int32 rank);
        if (rank < Cfm2Constants.HX_TAG_RANK_MIN || rank > Cfm2Constants.HX_TAG_RANK_MAX)
        {
            _mergeInfo.Warnings.Add("HTML H タグのランクが HTML Living Standard 仕様の範囲外です：" + hxMatch.Value);
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
            _mergeInfo.Warnings.Add("HTML H タグに ID 属性がないため目次が作成できません：" + hxMatch.Value);
            return (addColumn, null);
        }
        Debug.Assert(idMatch.Groups.Count >= 2, "ParseHxTag() idMatch.Groups が不足");
        String id = idMatch.Groups[1].Value;

        // 見出しを抽出する
        Int32 captionBeginPos = column + hxMatch.Index + hxMatch.Length;
        Int32 captionEndPos = line.Value.IndexOf("</h", captionBeginPos, StringComparison.OrdinalIgnoreCase);
        if (captionEndPos < 0)
        {
            _mergeInfo.Warnings.Add("HTML H タグが閉じられていないため目次が作成できません：" + hxMatch.Value);
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

        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
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
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            ProgressValue = 0.0;
            _progress = true;
            App.MainWindow.AddVeil("ProgressGrid", this);
            Debug.WriteLine("ShowProgressArea() " + _progress);
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
