// ============================================================================
// 
// メインビューの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
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
using Newtonsoft.Json.Linq;
using Shinta;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Composition.Desktop;
using Windows.UI.Popups;
using WinRT.Interop;

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
        FileOpenPicker fileOpenPicker = new();
        fileOpenPicker.FileTypeFilter.Add(Cfm2Constants.FILE_EXT_CFM2_MAKE);
        fileOpenPicker.FileTypeFilter.Add("*");
        IntPtr hwnd = App.MainWindow.GetWindowHandle();
        InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

        StorageFile? file = await fileOpenPicker.PickSingleFileAsync();
        if (file == null)
        {
            return;
        }

        MakePath = file.Path;

#if false
        Debug.WriteLine("ButtonBrowseMakeClicked() " + file.Path);

        String? read = await App.GetService<ILocalSettingsService>().ReadSettingAsync<String>("TestLocalSettingsKey");
        Debug.WriteLine("ButtonBrowseMakeClicked() read: " + read);
        await App.GetService<ILocalSettingsService>().SaveSettingAsync("TestLocalSettingsKey", "hoge " + DateTime.Now.ToString());

        Debug.WriteLine("Path: " + ApplicationData.Current.LocalFolder.Path);
        Debug.WriteLine("Name: " + ApplicationData.Current.LocalSettings.Name);
#endif
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
    // IncludeDefaultExt タグを実行
    // --------------------------------------------------------------------
    private void ExecuteTagIncludeDefaultExt(TagInfo tagInfo)
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
    private void ExecuteTagIncludeFolder(TagInfo tagInfo)
    {
        _mergeInfo.IncludeFolder = GetPath(tagInfo);
        Debug.WriteLine("ExecuteTagIncludeFolder() " + _mergeInfo.IncludeFolder);
        if (!Directory.Exists(_mergeInfo.IncludeFolder))
        {
            throw new Exception("インクルードフォルダーが存在しません：" + tagInfo.Value);
        }
    }

    // --------------------------------------------------------------------
    // OutFile タグを実行
    // --------------------------------------------------------------------
    private void ExexuteTagOutFile(TagInfo tagInfo)
    {
        _mergeInfo.OutPath = GetPath(tagInfo);
        Debug.WriteLine("ExexuteTagOutFile() " + _mergeInfo.OutPath);
        if (_mergeInfo.MakeFullPath.ToLower() == _mergeInfo.OutPath.ToLower())
        {
            throw new Exception("出力先ファイルがメイクファイルと同じです。");
        }
    }

    // --------------------------------------------------------------------
    // タグの値からパスを取得
    // --------------------------------------------------------------------
    private String GetPath(TagInfo tagInfo)
    {
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
    // 設定読み込み
    // --------------------------------------------------------------------
    private async Task LoadSettingsAsync()
    {
        MakePath = await App.GetService<ILocalSettingsService>().ReadSettingAsync<String>(Cfm2Constants.SETTINGS_KEY_MAKE_PATH) ?? String.Empty;
    }

    // --------------------------------------------------------------------
    // 合併メインルーチン
    // --------------------------------------------------------------------
    private async Task MergeAsync()
    {
        await Task.Run(async () =>
        {
            try
            {
                Debug.Assert(ProgressVisibility == Visibility.Collapsed, "MergeAsync() 既に実行中");
                ShowProgressArea();
                _mergeInfo = new();

                // デフォルト値を設定
                _mergeInfo.MakeFullPath = Path.GetFullPath(MakePath);
                _mergeInfo.IncludeFolder = Path.GetDirectoryName(_mergeInfo.MakeFullPath) ?? String.Empty;

                // メイクファイル読み込み
                ReadFile("メイクファイル", _mergeInfo.MakeFullPath, null);

                // 出力先ファイル設定
                _mergeInfo.OutPath = Path.GetDirectoryName(_mergeInfo.MakeFullPath) + "\\" + Path.GetFileNameWithoutExtension(_mergeInfo.MakeFullPath) + "Output" + Common.FILE_EXT_HTML;

                // メイクファイル処理
                ParseTags(_mergeInfo.Lines.First);

#if DEBUG
                Thread.Sleep(5 * 1000);
#endif
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
    // Cfm タグがあれば抽出する
    // --------------------------------------------------------------------
    private (Int32 column, TagInfo? tagInfo) ParseTag(LinkedListNode<String> line, Int32 column)
    {
        if (column >= line.Value.Length)
        {
            // 行末まで解析した
            return (column, null);
        }

        Match match = Regex.Match(line.Value[column..], @"\<\!\-\-\s*cfm\/(.+?)\-\-\>", RegexOptions.IgnoreCase);
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
        Int32 addColumn = line.Value[column..].IndexOf(match.Value) + match.Length;
        String tagContent = match.Groups[1].Value;
        Int32 colon = tagContent.IndexOf(':');
        if (colon < 0)
        {
            // キーと値を区切る ':' が無い
            Debug.WriteLine("ParseTag() 区切りコロン無し, " + tagContent + ", add: " + addColumn);
            _mergeInfo.Errors.Add("Cfm タグに区切りコロンがありません：" + tagContent);
            return (addColumn, null);
        }

        Int32 key = Array.IndexOf(Cfm2Constants.TAG_KEYS, tagContent[0..colon].Trim().ToLower());
        if (key < 0)
        {
            Debug.WriteLine("ParseTag() サポートされていないキー, " + tagContent + ", add: " + addColumn);
            _mergeInfo.Errors.Add("サポートされていない Cfm タグです：" + tagContent);
            return (addColumn, null);
        }

        TagInfo tagInfo = new()
        {
            Key = (TagKey)key,
            Value = tagContent[(colon + 1)..].Trim(),
        };
        Debug.WriteLine("ParseTag() [" + tagInfo.Key + "], [" + tagInfo.Value + "] add: " + addColumn);

        // タグは出力しないので削除する
        line.Value = line.Value.Replace(match.Value, null);

        return (0, tagInfo);
    }

    // --------------------------------------------------------------------
    // タグを解析して内容を更新する
    // --------------------------------------------------------------------
    private void ParseTags(LinkedListNode<String>? line)
    {
        for (; ; )
        {
            if (line == null)
            {
                break;
            }

            Int32 column = 0;
            for (; ; )
            {
                (Int32 addColumn, TagInfo? tagInfo) = ParseTag(line, column);
                if (tagInfo != null)
                {
                    // 有効なタグが見つかった
                    switch (tagInfo.Key)
                    {
                        case TagKey.OutFile:
                            ExexuteTagOutFile(tagInfo);
                            break;
                        case TagKey.IncludeFolder:
                            ExecuteTagIncludeFolder(tagInfo);
                            break;
                        case TagKey.IncludeDefaultExt:
                            ExecuteTagIncludeDefaultExt(tagInfo);
                            break;
                    }
                }
                else
                {
                    // 有効なタグが無かったので解析位置を進める
                    column += addColumn;
                }

                if (column >= line.Value.Length)
                {
                    break;
                }
            }

            line = line.Next;
        }
    }

    // --------------------------------------------------------------------
    // ファイルの内容を読み込んで Lines に加える（line の直後に加える）
    // --------------------------------------------------------------------
    private void ReadFile(String kind, String path, LinkedListNode<String>? line)
    {
        try
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new Exception("ファイルが指定されていません。");
            }

            String[] lines = File.ReadAllLines(path);
            if (lines.Length == 0)
            {
                throw new Exception("内容が空です。");
            }

            // 先頭行の追加
            LinkedListNode<String> continuePos;
            if (line == null)
            {
                // 末尾に追加
                continuePos = _mergeInfo.Lines.AddLast(lines[0]);
            }
            else
            {
                // 指定位置に追加
                continuePos = _mergeInfo.Lines.AddAfter(line, lines[0]);
            }

            // 残りの行の追加
            for (Int32 i = 1; i < lines.Length; i++)
            {
                continuePos = _mergeInfo.Lines.AddAfter(continuePos, lines[i]);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(kind + "を読み込めませんでした。\n" + path + "\n\n" + ex.Message);
        }
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
