// ============================================================================
// 
// メインビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Serilog;
using Serilog.Events;

using Shinta;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace CFileMerge2.ViewModels;

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
        ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClicked);
        ButtonOpenOutFileClickedCommand = new RelayCommand(ButtonOpenOutFileClicked);
        MenuFlyoutItemSampleFolderClickedCommand = new RelayCommand(MenuFlyoutItemSampleFolderClicked);
        MenuFlyoutItemAboutClickedCommand = new RelayCommand(MenuFlyoutItemAboutClicked);
        ButtonGoClickedCommand = new RelayCommand(ButtonGoClicked);

        // イベントハンドラー
        App.MainWindow.AppWindow.Closing += AppWindowClosing;
        App.MainWindow.Activated += MainWindowActivated;
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
    /// オーバーラップエリア表示
    /// </summary>
    private Visibility _overlapVisibility = Visibility.Collapsed;
    public Visibility OverlapVisibility
    {
        get => _overlapVisibility;
        set => SetProperty(ref _overlapVisibility, value);
    }

    /// <summary>
    /// プログレスエリア表示
    /// </summary>
    private Visibility _progressVisibility = Visibility.Collapsed;
    public Visibility ProgressVisibility
    {
        get => _progressVisibility;
        set => SetProperty(ref _progressVisibility, value);
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

    #region 参照ボタンの制御
    public ICommand ButtonBrowseMakeClickedCommand
    {
        get;
    }

    private async void ButtonBrowseMakeClicked()
    {
        FileOpenPicker fileOpenPicker = App.MainWindow.CreateOpenFilePicker();
        fileOpenPicker.FileTypeFilter.Add(Cfm2Constants.FILE_EXT_CFM2_MAKE);
        fileOpenPicker.FileTypeFilter.Add("*");

#if DEBUG
        Guid IInitializeWithWindowIID = new(0x3E68D4BD, 0x7135, 0x4D10, 0x80, 0x18, 0x9F, 0xB6, 0xD9, 0xF3, 0x3F, 0xA1);
        var asObjRef = global::WinRT.MarshalInspectable<object>.CreateMarshaler2(fileOpenPicker, IInitializeWithWindowIID);
        var ThisPtr = asObjRef.GetAbi();
#endif

        StorageFile? file = await fileOpenPicker.PickSingleFileAsync();
        if (file == null)
        {
            return;
        }

        MakePath = file.Path;
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
            if (String.IsNullOrEmpty(_mergeInfo.OutFullPath))
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        Debug.Assert(ProgressVisibility == Visibility.Collapsed, "ButtonOpenOutFileClicked() 既に実行中");
                        ShowProgressArea();
                        MergeCore();
#if DEBUGz
                        Thread.Sleep(3 * 1000);
#endif
                    }
                    catch (Exception ex)
                    {
                        await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, ex.Message);
                    }
                });
            }
            Common.ShellExecute(_mergeInfo.OutFullPath);
        }
        catch (Exception ex)
        {
            await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, ex.Message);
        }
        finally
        {
            HideProgressArea();
        }
    }
    #endregion

    #region ヘルプフライアウトの制御
    public ICommand MenuFlyoutItemHelpClickedCommand
    {
        get => Cfm2Model.Instance.EnvModel.HelpClickedCommand;
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
            await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, ex.Message);
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
        AboutWindow aboutWindow = new();
        await ShowDialogAsync(aboutWindow);
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
    /// 環境設定を適用
    /// </summary>
    private void ApplySettings()
    {
        MakePath = Cfm2Model.Instance.EnvModel.Cfm2Settings.MakePath;
    }

    /// <summary>
    /// イベントハンドラー：メインパネルのフォーカスを取得しようとしている
    /// </summary>
    public void MainPanelGettingFocus(UIElement _, GettingFocusEventArgs args)
    {
        Debug.WriteLine("MainUiGettingFocus() " + Environment.TickCount);
        if (OverlapVisibility == Visibility.Collapsed)
        {
            // オーバーラップエリアが非表示（なにも作業等をしていない状態）ならそのままフォーカスを取得する
            return;
        }

        // オーバーラップエリアが表示されている場合はフォーカスを取得しない
        // 終了確認後に Cancel を直接いじると落ちるので TryCancel() を使う
        if (args.TryCancel())
        {
            args.Handled = true;
        }
    }

    /// <summary>
    /// イベントハンドラー：メインパネルのサイズが変更された
    /// なぜか他のウィンドウと同じように VerticalAlignment="Top" を指定するとうまくいかない
    /// Depend: Window.SizeToContent が実装されればこのコードは不要
    /// </summary>
    public void MainPanelSizeChanged(Object sender, SizeChangedEventArgs _)
    {
        Double mainUiHeight = ((StackPanel)sender).ActualHeight;
        Debug.WriteLine("MainUiSizeChanged() mainUiHeight: " + mainUiHeight);
        if (mainUiHeight < _prevMainUiHeight)
        {
            return;
        }

        App.MainWindow.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(App.MainWindow.AppWindow.ClientSize.Width, (Int32)mainUiHeight));
        _prevMainUiHeight = mainUiHeight;
    }

    /// <summary>
    /// イベントハンドラー：ページがロードされた
    /// </summary>
    public void PageLoaded(Object _1, RoutedEventArgs _2)
    {
        Initialize();
        ApplySettings();
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
    /// 前回のメイン UI の高さ
    /// </summary>
    private Double _prevMainUiHeight;

    /// <summary>
    /// 開いているダイアログウィンドウ
    /// </summary>
    private WindowEx? _openingDialog;

    /// <summary>
    /// ダイアログ制御用
    /// </summary>
    private AutoResetEvent _dialogEvent = new(false);

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：ウィンドウが閉じられようとしている
    /// </summary>
    private async void AppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        // await を待つようにするため、いったんキャンセル
        args.Cancel = true;

        if (ProgressVisibility == Visibility.Visible)
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
        Cfm2Model.Instance.EnvModel.Cfm2Settings.MakePath = MakePath;
        await Cfm2Model.Instance.EnvModel.SaveCfm2Settings();
        Log.Information("終了しました：" + Cfm2Constants.APP_NAME_J + " " + Cfm2Constants.APP_VER + " --------------------");

        // 改めて閉じる
        App.MainWindow.Close();
    }

    /// <summary>
    /// イベントハンドラー：ダイアログが閉じられた
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void DialogClosed(object sender, WindowEventArgs args)
    {
        Debug.WriteLine("DialogClosed()");
        _dialogEvent.Set();
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
            // タグ値
            outSrc = tagValues[1];
        }
        _mergeInfo.AnchorOutFullFolder = GetPath(outSrc, Path.GetDirectoryName(_mergeInfo.OutFullPath) ?? String.Empty, "アンカー出力先フォルダー") + "\\";
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
    private void ExecuteCfmTagToc()
    {
        // 実際に目次を作成するのは、メイクファイルをすべて読み込んだ後なので、ここでは必要性の記録に留める
        _mergeInfo.TocNeeded = true;
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
    /// オーバーラップエリア非表示
    /// </summary>
    private void HideOverlapArea()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            OverlapVisibility = Visibility.Collapsed;
        });
    }

    /// <summary>
    /// プログレスエリア非表示
    /// </summary>
    private void HideProgressArea()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            ProgressVisibility = Visibility.Collapsed;
            HideOverlapArea();
        });
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        Debug.WriteLine("Initialize()");
#if DEBUG
        App.MainWindow.Title = "［デバッグ］" + App.MainWindow.Title;
#endif
#if TEST
        Title = "［テスト］" + Title;
#endif

        // なぜか MainWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        // ToDo: 効くようになればこのコードは不要
        App.MainWindow.Width = 800;

        // Height は後で MainPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        // 何らかの理由によりウィンドウサイズが大きくなった場合、なぜか前バージョン以下の数値だと効果を発揮しないので、前バージョンより 1 大きな値にする
        // ToDo: Window.SizeToContent が実装されればこのコードは不要
        App.MainWindow.Height = 201;
    }

    /// <summary>
    /// イベントハンドラー：メインウィンドウ Activated / Deactivated
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args"></param>
    public void MainWindowActivated(Object _, WindowActivatedEventArgs args)
    {
        if ((args.WindowActivationState == WindowActivationState.PointerActivated || args.WindowActivationState == WindowActivationState.CodeActivated) && _openingDialog != null)
        {
            _openingDialog.Activate();
        }
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
                Debug.Assert(ProgressVisibility == Visibility.Collapsed, "MergeAsync() 既に実行中");
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
                File.WriteAllLines(_mergeInfo.OutFullPath, _mergeInfo.Lines);

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

                // 報告
                if (_mergeInfo.Warnings.Any())
                {
                    // 警告あり
                    String message = "警告があります。\n";
                    for (Int32 i = 0; i < _mergeInfo.Warnings.Count; i++)
                    {
                        message += _mergeInfo.Warnings[i] + "\n";
                    }
                    await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Warning, message);
                }
                else
                {
                    // 完了
                    await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Information, "完了しました。\n経過時間：" + (Environment.TickCount - startTick).ToString("#,0") + " ミリ秒");
                }
            }
            catch (Exception ex)
            {
                await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, ex.Message);
            }
            finally
            {
                HideProgressArea();
                ReresizeClient();
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

        // メイクファイル読み込み（再帰）
        ParseFile(_mergeInfo.MakeFullPath, _mergeInfo.Lines, null);
    }

    /// <summary>
    /// アンカーファイル群を出力
    /// </summary>
    private void OutputAnchors()
    {
        // アンカー出力先フォルダー作成
        Directory.CreateDirectory(_mergeInfo.AnchorOutFullFolder);

        // アンカーファイル作成対象の Hx タグを検索
        List<HxTagInfo> hxTagInfos = ParseHxTags(Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets);

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
            List<String> anchorFileContents = new(lines);

            // アンカー置換
            for (Int32 j = 0; j < anchorPositionIndexes.Count; j++)
            {
                anchorFileContents[anchorPositionIndexes[j].Key] = anchorFileContents[anchorPositionIndexes[j].Key].Insert(anchorPositionIndexes[j].Value, relativePath + "#" + hxTagInfos[i].Id);
            }

            File.WriteAllLines(_mergeInfo.AnchorOutFullFolder + Path.GetFileNameWithoutExtension(_mergeInfo.OutFullPath) + "_" + hxTagInfos[i].Id + Common.FILE_EXT_HTML, anchorFileContents);

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
                            ExecuteCfmTagToc();
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
#if DEBUG
                Thread.Sleep(20);
#endif
            }
            if (line == null)
            {
                break;
            }
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
                        List<HxTagInfo> hxTagInfos = ParseHxTags(Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets);
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
    private void ParseFile(String path, LinkedList<String> parentLines, LinkedListNode<String>? parentLine)
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

        // このファイルの階層以下の内容
        String[] childLineStrings = File.ReadAllLines(path);
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
    /// 前回調整時のウィンドウサイズに再度設定
    /// MainUiSizeChanged() イベントハンドラー内だと効かないことがあるのでその対策
    /// ToDo: Window.SizeToContent が実装されればこのコードは不要
    /// </summary>
    private void ReresizeClient()
    {
        App.MainWindow.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(App.MainWindow.AppWindow.ClientSize.Width, (Int32)_prevMainUiHeight));
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
    /// ウィンドウをモーダルで表示
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    private Task ShowDialogAsync(WindowEx window)
    {
        if (_openingDialog != null)
        {
            throw new Exception("内部エラー：既にダイアログが開いています。");
        }
        _openingDialog = window;

        // ディスプレイサイズが不明なのでカスケードしない（はみ出し防止）
        ShowOverlapArea();
        window.Closed += DialogClosed;
        window.AppWindow.Move(new PointInt32(App.MainWindow.AppWindow.Position.X, App.MainWindow.AppWindow.Position.Y));
        window.Activate();

        return Task.Run(() =>
        {
            _dialogEvent.WaitOne();
            _openingDialog = null;
            HideOverlapArea();
        });
    }

    /// <summary>
    /// オーバーラップエリア表示
    /// </summary>
    private void ShowOverlapArea()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            OverlapVisibility = Visibility.Visible;
        });
    }

    /// <summary>
    /// プログレスエリアを表示
    /// </summary>
    private void ShowProgressArea()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            ShowOverlapArea();
            ProgressValue = 0.0;
            ProgressVisibility = Visibility.Visible;
        });
    }
}
