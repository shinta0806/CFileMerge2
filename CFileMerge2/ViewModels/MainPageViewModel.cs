// ============================================================================
// 
// メインビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Serilog.Events;
using Shinta;

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
        // コマンド
        ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClicked);
        ButtonOpenOutFileClickedCommand = new RelayCommand(ButtonOpenOutFileClicked);
        MenuFlyoutItemSampleFolderClickedCommand = new RelayCommand(MenuFlyoutItemSampleFolderClicked);
        MenuFlyoutItemAboutClickedCommand = new RelayCommand(MenuFlyoutItemAboutClicked);
        ButtonGoClickedCommand = new RelayCommand(ButtonGoClicked);

        // イベントハンドラー
        App.MainWindow.AppWindow.Closing += AppWindow_Closing;
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
        await App.MainWindow.CreateMessageDialog(Cfm2Constants.APP_NAME_J + "\n" + Cfm2Constants.APP_VER + "  " + Cfm2Constants.APP_DISTRIB + "\n"
                + Cfm2Constants.COPYRIGHT_J, Cfm2Constants.LABEL_INFORMATION).ShowAsync();
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
    /// イベントハンドラー：メイン UI のフォーカスを取得しようとしている
    /// </summary>
    public void MainUiGettingFocus(UIElement _, GettingFocusEventArgs args)
    {
        Debug.WriteLine("MainUiGettingFocus() " + Environment.TickCount);
        if (ProgressVisibility == Visibility.Visible)
        {
            // プログレスエリアが表示されている場合はフォーカスを取得しない
            // 終了確認後に Cancel を直接いじると落ちるので TryCancel() を使う
            if (args.TryCancel())
            {
                args.Handled = true;
            }
        }
    }

    /// <summary>
    /// イベントハンドラー：メイン UI のサイズが変更された
    /// Depend: Window.SizeToContent が実装されればこのコードは不要
    /// </summary>
    public void MainUiSizeChanged(Object sender, SizeChangedEventArgs _)
    {
        Double mainUiHeight = ((StackPanel)sender).ActualHeight;
        Debug.WriteLine("MainUiSizeChanged() " + mainUiHeight);
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
        ApplySettings();
    }

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

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：ウィンドウが閉じられようとしている
    /// </summary>
    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
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

        // 改めて閉じる
        App.MainWindow.Close();
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
        _mergeInfo.IncludeFullFolder = GetPath(tagInfo);
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
        _mergeInfo.OutFullPath = GetPath(tagInfo);
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
    /// タグの値からパスを取得
    /// </summary>
    /// <param name="tagInfo">タグ情報</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private String GetPath(CfmTagInfo tagInfo)
    {
        if (String.IsNullOrEmpty(tagInfo.Value))
        {
            throw new Exception(Cfm2Constants.CFM_TAG_KEYS[(Int32)tagInfo.Key] + " タグのパスが指定されていません。");
        }
        if (Path.IsPathRooted(tagInfo.Value))
        {
            // 絶対パス
            return tagInfo.Value;
        }
        else
        {
            // メイクファイルからの相対パス
            return Path.GetFullPath(tagInfo.Value, Path.GetDirectoryName(_mergeInfo.MakeFullPath) ?? String.Empty);
        }
    }

    /// <summary>
    /// プログレスエリアを非表示
    /// </summary>
    private void HideProgressArea()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            ProgressVisibility = Visibility.Collapsed;
        });
    }

    /// <summary>
    /// 目次作成
    /// </summary>
    private void InsertToc()
    {
        _mergeInfo.Toc.Add("<div class=\"" + Cfm2Constants.TOC_AREA_CLASS_NAME + "\">");
        ParseHxTags();
        _mergeInfo.Toc.Add("</div>");
        ParseCfmTagsForToc();
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
                    InsertToc();
                }

                // 出力
                SetProgressValue(MergeStep.Output, 0.0);
                Directory.CreateDirectory(Path.GetDirectoryName(_mergeInfo.OutFullPath) ?? String.Empty);
                File.WriteAllLines(_mergeInfo.OutFullPath, _mergeInfo.Lines);

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
                        line.Value = line.Value.Insert(column + addColumn, String.Join('\n', _mergeInfo.Toc));
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
    private Int32 ParseHxTag(LinkedListNode<String> line, Int32 column)
    {
        if (column >= line.Value.Length)
        {
            // 行末まで解析した
            return column;
        }

        Match hxMatch = Regex.Match(line.Value[column..], @"\<h([1-6])\s.+?\>", RegexOptions.IgnoreCase);
        if (!hxMatch.Success)
        {
            // Hx タグが無い
            return line.Value.Length;
        }

        Debug.Assert(hxMatch.Groups.Count >= 2, "ParseHxTag() hxMatch.Groups が不足");
        Int32 addColumn = hxMatch.Index + hxMatch.Length;

        // ランク確認
        Int32.TryParse(hxMatch.Groups[1].Value, out Int32 rank);
        if (rank < Cfm2Constants.HX_TAG_RANK_MIN || rank > Cfm2Constants.HX_TAG_RANK_MAX)
        {
            _mergeInfo.Warnings.Add("HTML H タグのランクが HTML Living Standard 仕様の範囲外です：" + hxMatch.Value);
            return addColumn;
        }
        if (!Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets[rank])
        {
            // 環境設定により対象外
            return addColumn;
        }

        // ID 属性を抽出する
        Match idMatch = Regex.Match(hxMatch.Value, @"id\s*?=\s*?" + "\"" + "(.+?)" + "\"", RegexOptions.IgnoreCase);
        if (!idMatch.Success)
        {
            // ID 属性が無い
            _mergeInfo.Warnings.Add("HTML H タグに ID 属性がないため目次が作成できません：" + hxMatch.Value);
            return addColumn;
        }
        Debug.Assert(idMatch.Groups.Count >= 2, "ParseHxTag() idMatch.Groups が不足");
        String id = idMatch.Groups[1].Value;

        // 見出しを抽出する
        Int32 captionBeginPos = column + hxMatch.Index + hxMatch.Length;
        Int32 captionEndPos = line.Value.IndexOf("</h", captionBeginPos, StringComparison.OrdinalIgnoreCase);
        if (captionEndPos < 0)
        {
            _mergeInfo.Warnings.Add("HTML H タグが閉じられていないため目次が作成できません：" + hxMatch.Value);
            return addColumn;
        }
        String caption = line.Value[captionBeginPos..captionEndPos].Trim();

        // 目次情報追加
        _mergeInfo.Toc.Add("<div class=\"" + Cfm2Constants.TOC_ITEM_CLASS_NAME_PREFIX + rank + "\"><a href=\"#" + id + "\">" + caption + "</a></div>");

        return addColumn;
    }

    /// <summary>
    /// HTML Hx タグを解析して目次情報を収集する
    /// </summary>
    private void ParseHxTags()
    {
        LinkedListNode<String>? line = _mergeInfo.Lines.First;
        Debug.Assert(line != null, "ParseHxTags() _mergeInfo.Lines が空");
        Int32 numProgressLines = 0;

        // 行をたどるループ
        for (; ; )
        {
            Int32 column = 0;

            // 列をたどるループ
            for (; ; )
            {
                Int32 addColumn = ParseHxTag(line, column);

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
            ProgressVisibility = Visibility.Visible;
        });
    }
}
