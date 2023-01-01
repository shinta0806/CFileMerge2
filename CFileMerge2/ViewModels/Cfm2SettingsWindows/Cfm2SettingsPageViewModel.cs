// ============================================================================
// 
// 環境設定ページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Windows.Input;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views;
using CFileMerge2.Views.Cfm2SettingsWindows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Serilog;
using Serilog.Events;
using Shinta;
using Windows.UI.Popups;
using WinUIEx;

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public class Cfm2SettingsPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsPageViewModel(WindowEx3 window)
    {
        // 初期化
        _window = window;
        Cfm2SettingsNavigationSettingsPage settingsPage = new(window, this);
        Cfm2SettingsNavigationMaintenancePage maintenancePage = new(window, this);
        _pages = new Page[]
        {
            settingsPage,
            maintenancePage,
        };
        _navigationViewContent = settingsPage;
        _pageViewModels = new NavigationPageViewModel[]
        {
            settingsPage.ViewModel,
            maintenancePage.ViewModel,
        };

        // コマンド
        ButtonDefaultClickedCommand = new RelayCommand(ButtonDefaultClicked);
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

    #region 初期化ボタンの制御
    public ICommand ButtonDefaultClickedCommand
    {
        get;
    }

    private async void ButtonDefaultClicked()
    {
        try
        {
            MessageDialog messageDialog = _window.CreateMessageDialog("Cfm2SettingsPageViewModel_Default_Confirm".ToLocalized(),
                    LogEventLevel.Warning.ToString().ToLocalized());
            messageDialog.Commands.Add(new UICommand(Common.LK_GENERAL_LABEL_YES.ToLocalized()));
            messageDialog.Commands.Add(new UICommand(Common.LK_GENERAL_LABEL_NO.ToLocalized()));
            IUICommand cmd = await messageDialog.ShowAsync();
            if (cmd.Label != Common.LK_GENERAL_LABEL_YES.ToLocalized())
            {
                return;
            }

            // 初期設定
            Cfm2Model.Instance.EnvModel.Cfm2Settings = new();
            Cfm2Model.Instance.EnvModel.Cfm2Settings.Adjust();
            SettingsToProperties();
            await Cfm2Model.Instance.EnvModel.SaveCfm2SettingsAsync();
        }
        catch (Exception ex)
        {
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "Cfm2SettingsPageViewModel_ButtonDefaultClicked_Error".ToLocalized() + "\n" + ex.Message);
            SerilogUtils.LogStackTrace(ex);
        }
    }
    #endregion

    #region OK ボタンの制御
    public ICommand ButtonOkClickedCommand
    {
        get;
    }

    private async void ButtonOkClicked()
    {
        try
        {
            // 妥当性確認後にプロパティーから設定に反映
            CheckPropertiesAndPropertiesToSettings();

            // 保存
            await Cfm2Model.Instance.EnvModel.SaveCfm2SettingsAsync();

            _window.Close();
        }
        catch (Exception ex)
        {
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "Cfm2SettingsPageViewModel_ButtonOkClicked_Error".ToLocalized() + "\n" + ex.Message);
            SerilogUtils.LogStackTrace(ex);
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
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "Cfm2SettingsPageViewModel_ButtonCancelClicked_Error".ToLocalized() + "\n" + ex.Message);
            SerilogUtils.LogStackTrace(ex);
        }
    }
    #endregion

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 妥当性確認後にプロパティーから設定に反映
    /// </summary>
    public void CheckPropertiesAndPropertiesToSettings()
    {
        // 配下のナビゲーションの妥当性確認
        for (Int32 i = 0; i < _pageViewModels.Length; i++)
        {
            _pageViewModels[i].CheckProperties();
        }

        // 配下のナビゲーションのプロパティーから設定に反映
        for (Int32 i = 0; i < _pageViewModels.Length; i++)
        {
            _pageViewModels[i].PropertiesToSettings();
        }
    }

    /// <summary>
    /// イベントハンドラー：Escape キー押下
    /// </summary>
    public void KeyboardAcceleratorEscapeInvoked(KeyboardAccelerator _1, KeyboardAcceleratorInvokedEventArgs _2)
    {
        ButtonCancelClicked();
    }

    /// <summary>
    /// イベントハンドラー：ナビゲーション選択変更
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public async void NavigationViewSelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
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
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "Cfm2SettingsPageViewModel_NavigationViewSelectionChanged_Error".ToLocalized() + "\n" + ex.Message);
            SerilogUtils.LogStackTrace(ex);
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
            SerilogUtils.LogStackTrace(ex);
        }
    }

    /// <summary>
    /// 設定をプロパティーに反映
    /// </summary>
    public void SettingsToProperties()
    {
        // 配下のナビゲーションの設定をプロパティーに反映
        for (Int32 i = 0; i < _pageViewModels.Length; i++)
        {
            _pageViewModels[i].SettingsToProperties();
        }
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// ウィンドウ
    /// </summary>
    private readonly WindowEx3 _window;

    /// <summary>
    /// ページ
    /// </summary>
    private readonly Page[] _pages;

    /// <summary>
    /// ページのビューモデル
    /// </summary>
    private readonly NavigationPageViewModel[] _pageViewModels;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        SettingsToProperties();
    }
}
