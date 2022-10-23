﻿// ============================================================================
// 
// メインビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CFileMerge2.Contracts.Services;
using CFileMerge2.Models.SharedMisc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Shinta;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Composition.Desktop;
using Windows.UI.Popups;
using WinRT.Interop;
using WinUIEx.Messaging;

namespace CFileMerge2.ViewModels;

public class MainPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    // --------------------------------------------------------------------
    // メインコンストラクター
    // --------------------------------------------------------------------
    public MainPageViewModel()
    {
        // コマンド
        ButtonBrowseMakeClickedCommand = new RelayCommand(ButtonBrowseMakeClicked);
        ButtonOpenOutFileClickedCommand = new RelayCommand(ButtonOpenOutFileClicked);
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

    // メイクファイル
    private String _makePath = String.Empty;
    public String MakePath
    {
        get => _makePath;
        set => SetProperty(ref _makePath, value);
    }

    // プログレスエリア表示
    private Visibility _progressVisibility = Visibility.Collapsed;
    public Visibility ProgressVisibility
    {
        get => _progressVisibility;
        set => SetProperty(ref _progressVisibility, value);
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
#if DEBUG
                        Thread.Sleep(3 * 1000);
#endif
                    }
                    catch (Exception ex)
                    {
                        await App.MainWindow.CreateMessageDialog(ex.Message, Cfm2Constants.LABEL_ERROR).ShowAsync();
                    }
                });
            }
            Common.ShellExecute(_mergeInfo.OutFullPath);
        }
        catch (Exception ex)
        {
            await App.MainWindow.CreateMessageDialog(ex.Message, Cfm2Constants.LABEL_ERROR).ShowAsync();
        }
        finally
        {
            HideProgressArea();
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

    // --------------------------------------------------------------------
    // イベントハンドラー：メイン UI のフォーカスを取得しようとしている
    // --------------------------------------------------------------------
    public void MainUiGettingFocus(UIElement sender, GettingFocusEventArgs args)
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

    // --------------------------------------------------------------------
    // イベントハンドラー：メイン UI のサイズが変更された
    // Depend: Window.SizeToContent が実装されればこのコードは不要
    // --------------------------------------------------------------------
    public void MainUiSizeChanged(Object sender, SizeChangedEventArgs args)
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

    // --------------------------------------------------------------------
    // イベントハンドラー：ページがロードされた
    // --------------------------------------------------------------------
    public async void PageLoaded(Object sender, RoutedEventArgs args)
    {
        Debug.WriteLine("GridLoaded()");
        await LoadSettingsAsync();
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    // 合併作業用の情報
    private MergeInfo _mergeInfo = new();

    // 前回のメイン UI の高さ
    private Double _prevMainUiHeight;

    // ====================================================================
    // private 関数
    // ====================================================================

    // --------------------------------------------------------------------
    // イベントハンドラー：ウィンドウが閉じられようとしている
    // --------------------------------------------------------------------
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
        await App.GetService<ILocalSettingsService>().SaveSettingAsync(Cfm2Constants.SETTINGS_KEY_MAKE_PATH, MakePath);

        // 改めて閉じる
        App.MainWindow.Close();
    }

    // --------------------------------------------------------------------
    // Include タグを実行
    // --------------------------------------------------------------------
    private void ExecuteCfmTagInclude(TagInfo tagInfo, LinkedList<String> parentLines, LinkedListNode<String> parentLine)
    {
        // インクルードパスを取得（IncludeFolder を加味する必要があるため GetPath() は使えない）
        String path;
        if (String.IsNullOrEmpty(tagInfo.Value))
        {
            throw new Exception(Cfm2Constants.TAG_KEYS[(Int32)tagInfo.Key] + " タグのパスが指定されていません。");
        }
        if (Path.IsPathRooted(tagInfo.Value))
        {
            // 絶対パス
            path = tagInfo.Value;
        }
        else
        {
            // インクルードフォルダーからの相対パス
            Debug.Assert(!String.IsNullOrEmpty(_mergeInfo.IncludeFolderFullPath), "ExecuteTagInclude() IncludeFolder が初期化されていない");
            path = Path.GetFullPath(tagInfo.Value, _mergeInfo.IncludeFolderFullPath);
        }
        if (!Path.HasExtension(path))
        {
            path += _mergeInfo.IncludeDefaultExt;
        }
        Debug.WriteLine("ExecuteTagInclude() " + path);

        // インクルード
        ParseFile(path, parentLines, parentLine);
    }

    // --------------------------------------------------------------------
    // IncludeDefaultExt タグを実行
    // --------------------------------------------------------------------
    private void ExecuteCfmTagIncludeDefaultExt(TagInfo tagInfo)
    {
        _mergeInfo.IncludeDefaultExt = tagInfo.Value;
        if (!String.IsNullOrEmpty(_mergeInfo.IncludeDefaultExt) && _mergeInfo.IncludeDefaultExt[0] != '.')
        {
            _mergeInfo.IncludeDefaultExt = '.' + _mergeInfo.IncludeDefaultExt;
        }
        Debug.WriteLine("ExecuteTagIncludeDefaultExt() " + _mergeInfo.IncludeDefaultExt);
    }

    // --------------------------------------------------------------------
    // IncludeFolder タグを実行
    // --------------------------------------------------------------------
    private void ExecuteCfmTagIncludeFolder(TagInfo tagInfo)
    {
        _mergeInfo.IncludeFolderFullPath = GetPath(tagInfo);
        Debug.WriteLine("ExecuteTagIncludeFolder() " + _mergeInfo.IncludeFolderFullPath);
        if (!Directory.Exists(_mergeInfo.IncludeFolderFullPath))
        {
            // 続行可能といえば続行可能であるが、Include できない際に原因が分かりづらくなるのでここでエラーとする
            throw new Exception("インクルードフォルダーが存在しません：\n" + tagInfo.Value);
        }
    }

    // --------------------------------------------------------------------
    // OutFile タグを実行
    // --------------------------------------------------------------------
    private void ExexuteCfmTagOutFile(TagInfo tagInfo)
    {
        _mergeInfo.OutFullPath = GetPath(tagInfo);
        Debug.WriteLine("ExexuteTagOutFile() " + _mergeInfo.OutFullPath);
        if (String.Compare(_mergeInfo.OutFullPath, _mergeInfo.MakeFullPath, true) == 0)
        {
            throw new Exception("出力先ファイルがメイクファイルと同じです。");
        }
    }

    // --------------------------------------------------------------------
    // Set タグを実行
    // --------------------------------------------------------------------
    private void ExecuteCfmTagSet(TagInfo tagInfo)
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

    // --------------------------------------------------------------------
    // Var タグを実行
    // --------------------------------------------------------------------
    private void ExecuteCfmTagVar(TagInfo tagInfo, LinkedListNode<String> line, Int32 column)
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

    // --------------------------------------------------------------------
    // タグの値からパスを取得
    // --------------------------------------------------------------------
    private String GetPath(TagInfo tagInfo)
    {
        if (String.IsNullOrEmpty(tagInfo.Value))
        {
            throw new Exception(Cfm2Constants.TAG_KEYS[(Int32)tagInfo.Key] + " タグのパスが指定されていません。");
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

    // --------------------------------------------------------------------
    // プログレスエリアを非表示
    // --------------------------------------------------------------------
    private void HideProgressArea()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            ProgressVisibility = Visibility.Collapsed;
        });
    }

    // --------------------------------------------------------------------
    // 目次作成
    // --------------------------------------------------------------------
    private void InsertToc()
    {

    }

    // --------------------------------------------------------------------
    // 設定読み込み
    // --------------------------------------------------------------------
    private async Task LoadSettingsAsync()
    {
        MakePath = await App.GetService<ILocalSettingsService>().ReadSettingAsync<String>(Cfm2Constants.SETTINGS_KEY_MAKE_PATH) ?? String.Empty;
    }

    // --------------------------------------------------------------------
    // 合併メインルーチン
    // 続行不可能なエラーは直ちに Exception を投げる
    // 続行可能なエラーは MergeInfo.Errors に貯めて最後に表示する
    // --------------------------------------------------------------------
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
                InsertToc();

                // 出力
                Directory.CreateDirectory(Path.GetDirectoryName(_mergeInfo.OutFullPath) ?? String.Empty);
                File.WriteAllLines(_mergeInfo.OutFullPath, _mergeInfo.Lines);

#if DEBUG
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
                    await App.MainWindow.CreateMessageDialog(message, Cfm2Constants.LABEL_WARNING).ShowAsync();
                }
                else
                {
                    // 完了
                    await App.MainWindow.CreateMessageDialog("完了しました。\n経過時間：" + (Environment.TickCount - startTick).ToString("#,0") + " ミリ秒", Cfm2Constants.LABEL_INFORMATION).ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await App.MainWindow.CreateMessageDialog(ex.Message, Cfm2Constants.LABEL_ERROR).ShowAsync();
            }
            finally
            {
                HideProgressArea();
            }
        });
    }

    // --------------------------------------------------------------------
    // 合併コア
    // 出力は行わない
    // --------------------------------------------------------------------
    private void MergeCore()
    {
        _mergeInfo = new();

        // デフォルト値を設定
        _mergeInfo.MakeFullPath = Path.GetFullPath(MakePath);
        _mergeInfo.IncludeFolderFullPath = Path.GetDirectoryName(_mergeInfo.MakeFullPath) ?? String.Empty;
        _mergeInfo.OutFullPath = Path.GetDirectoryName(_mergeInfo.MakeFullPath) + "\\" + Path.GetFileNameWithoutExtension(_mergeInfo.MakeFullPath) + "Output" + Common.FILE_EXT_HTML;

        // メイクファイル読み込み（再帰）
        ParseFile(_mergeInfo.MakeFullPath, _mergeInfo.Lines, null);
    }

    // --------------------------------------------------------------------
    // Cfm タグがあれば抽出する
    // --------------------------------------------------------------------
    private (Int32 column, TagInfo? tagInfo) ParseCfmTag(LinkedListNode<String> line, Int32 column, Boolean removeTocTag)
    {
        if (column >= line.Value.Length)
        {
            // 行末まで解析した
            return (column, null);
        }

        Match match = Regex.Match(line.Value[column..], @"\<\!\-\-\s*?cfm\/(.+?)\-\-\>", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            // Cfm タグが無い
            return (line.Value.Length, null);
        }

        Debug.Assert(match.Groups.Count >= 2, "ParseTag() match.Groups が不足");
#if false
        Debug.WriteLine("ParseTag() match: " + match.Value);
        Debug.WriteLine("ParseTag() groups: " + match.Groups.Count);
        Debug.WriteLine("ParseTag() group[0]: " + match.Groups[0].Value);
        Debug.WriteLine("ParseTag() group[1]: " + match.Groups[1].Value);
#endif
        Int32 addColumn = match.Index + match.Length;
        String tagContent = match.Groups[1].Value;
        Int32 colon = tagContent.IndexOf(':');
        if (colon < 0)
        {
            // キーと値を区切る ':' が無い
            Debug.WriteLine("ParseTag() 区切りコロン無し, " + tagContent + ", add: " + addColumn);
            _mergeInfo.Warnings.Add("Cfm タグに区切りコロンがありません：" + tagContent);
            return (addColumn, null);
        }

        Int32 key = Array.IndexOf(Cfm2Constants.TAG_KEYS, tagContent[0..colon].Trim().ToLower());
        if (key < 0)
        {
            Debug.WriteLine("ParseTag() サポートされていないキー, " + tagContent + ", add: " + addColumn);
            _mergeInfo.Warnings.Add("サポートされていない Cfm タグです：" + tagContent);
            return (addColumn, null);
        }

        TagInfo tagInfo = new()
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

        return (addColumn, tagInfo);
    }

    // --------------------------------------------------------------------
    // タグを解析して内容を更新する
    // --------------------------------------------------------------------
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
                (Int32 addColumn, TagInfo? tagInfo) = ParseCfmTag(line, column, false);
                if (tagInfo != null)
                {
                    // 有効なタグが見つかった（タグに対応する文字列は削除されている）
                    switch (tagInfo.Key)
                    {
                        case TagKey.OutFile:
                            ExexuteCfmTagOutFile(tagInfo);
                            break;
                        case TagKey.IncludeFolder:
                            ExecuteCfmTagIncludeFolder(tagInfo);
                            break;
                        case TagKey.IncludeDefaultExt:
                            ExecuteCfmTagIncludeDefaultExt(tagInfo);
                            break;
                        case TagKey.Include:
                            ExecuteCfmTagInclude(tagInfo, lines, line);
                            break;
                        case TagKey.Set:
                            ExecuteCfmTagSet(tagInfo);
                            break;
                        case TagKey.Var:
                            ExecuteCfmTagVar(tagInfo, line, column + addColumn);
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
            if (line == null)
            {
                break;
            }
        }
    }

    // --------------------------------------------------------------------
    // ファイルの内容を読み込み解析する
    // 解析後の内容を parentLine の直後に追加（parentLine が null の場合は先頭に追加）
    // --------------------------------------------------------------------
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
    }

    // --------------------------------------------------------------------
    // プログレスエリアを表示
    // --------------------------------------------------------------------
    private void ShowProgressArea()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            ProgressVisibility = Visibility.Visible;
        });
    }
}
