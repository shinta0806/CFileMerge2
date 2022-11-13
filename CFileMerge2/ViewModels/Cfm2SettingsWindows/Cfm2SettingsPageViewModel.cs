// ============================================================================
// 
// 環境設定ページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Windows.Input;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views.Cfm2SettingsWindows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Serilog;
using Serilog.Events;
using Windows.Graphics;

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public class Cfm2SettingsPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsPageViewModel(WindowEx window)
    {
        // 初期化
        _window = window;
        _pages = new Page[]
        {
            new Cfm2SettingsNavigationSettingsPage(),
            new Cfm2SettingsNavigationMaintenancePage(),
        };
        _navigationViewContent = _pages[0];

        // コマンド
        ButtonOkClickedCommand = new RelayCommand(ButtonOkClicked);
        ButtonCancelClickedCommand = new RelayCommand(ButtonCancelClicked);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // View 通信用のプロパティー
    // --------------------------------------------------------------------

    /// <summary>
    /// ナビゲーションで選択されたアイテムに対するコンテンツ
    /// </summary>
    private Page _navigationViewContent;
    public Page NavigationViewContent
    {
        get => _navigationViewContent;
        set => SetProperty(ref _navigationViewContent, value);
    }

    // --------------------------------------------------------------------
    // 一般のプロパティー
    // --------------------------------------------------------------------

    // --------------------------------------------------------------------
    // コマンド
    // --------------------------------------------------------------------

    #region OK ボタンの制御
    public ICommand ButtonOkClickedCommand
    {
        get;
    }

    private async void ButtonOkClicked()
    {
        try
        {
            // 配下のナビゲーションの妥当性確認
            ((Cfm2SettingsNavigationSettingsPage)_pages[(Int32)Cfm2SettingsNavigationViewItems.Settings]).ViewModel.CheckProperties();

            // 配下のナビゲーションのプロパティーから設定に反映
            ((Cfm2SettingsNavigationSettingsPage)_pages[(Int32)Cfm2SettingsNavigationViewItems.Settings]).ViewModel.PropertiesToSettings();

            // 保存
            await Cfm2Model.Instance.EnvModel.SaveCfm2Settings();

            _window.Close();
        }
        catch (Exception ex)
        {
            await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, "OK ボタンクリック時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion

    #region キャンセルボタンの制御
    public ICommand ButtonCancelClickedCommand
    {
        get;
    }

    private async void ButtonCancelClicked()
    {
        try
        {
            _window.Close();
        }
        catch (Exception ex)
        {
            await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, "キャンセルボタンクリック時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：メインパネルのサイズが変更された
    /// Depend: Window.SizeToContent が実装されればこのコードは不要
    /// </summary>
    public void MainPanelSizeChanged(Object sender, SizeChangedEventArgs _)
    {
        try
        {
            _mainPanelHeight = ((StackPanel)sender).ActualHeight;
            Log.Debug("Cfm2SettingsPageViewModel.MainPanelSizeChanged() _mainPanelHeight: " + _mainPanelHeight);
        }
        catch (Exception ex)
        {
            // ユーザー起因では発生しないイベントなのでログのみ
            Log.Error("環境設定ページメインパネルサイズ変更時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }

    /// <summary>
    /// イベントハンドラー：ナビゲーション選択変更
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        try
        {
            NavigationViewItem? navigationViewItem = args.SelectedItem as NavigationViewItem;
            String? tag = navigationViewItem?.Tag?.ToString();
            if (String.IsNullOrEmpty(tag))
            {
                return;
            }
            if (!Enum.TryParse(tag, out Cfm2SettingsNavigationViewItems item))
            {
                return;
            }

            NavigationViewContent = _pages[(Int32)item];
        }
        catch (Exception ex)
        {
            await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Error, "ナビゲーション選択時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }

    /// <summary>
    /// イベントハンドラー：ページがロードされた
    /// </summary>
    public void PageLoaded(Object _1, RoutedEventArgs _2)
    {
        try
        {
            Initialize();
        }
        catch (Exception ex)
        {
            // ユーザー起因では発生しないイベントなのでログのみ
            Log.Error("ページロード時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// ウィンドウ
    /// </summary>
    private readonly WindowEx _window;

    /// <summary>
    /// ページ
    /// </summary>
    private readonly Page[] _pages;

    /// <summary>
    /// メインパネルの高さ
    /// </summary>
    private Double _mainPanelHeight;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        _window.Title = "環境設定";

        // 配下のナビゲーションの設定をプロパティーに反映
        ((Cfm2SettingsNavigationSettingsPage)_pages[(Int32)Cfm2SettingsNavigationViewItems.Settings]).ViewModel.SettingsToProperties();

        // SizeToContent
        //ReresizeClient();
    }

    /// <summary>
    /// メインパネルの高さに合わせてウィンドウサイズを設定
    /// ToDo: Window.SizeToContent が実装されればこのコードは不要
    /// </summary>
    private void ReresizeClient()
    {
        Log.Debug("Cfm2SettingsPageViewModel.ReresizeClient() " + _mainPanelHeight);
        _window.AppWindow.ResizeClient(new SizeInt32(_window.AppWindow.ClientSize.Width, (Int32)(_mainPanelHeight)));
    }

}
