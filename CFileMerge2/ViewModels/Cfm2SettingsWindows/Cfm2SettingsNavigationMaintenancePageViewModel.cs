// ============================================================================
// 
// メンテナンスページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFileMerge2.Models.SharedMisc;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Serilog;
using System.Diagnostics;
using CFileMerge2.Models.Cfm2Models;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CFileMerge2.Views.Cfm2SettingsWindows;
using Serilog.Events;
using Windows.UI.Popups;
using CFileMerge2.Views;
using WinUIEx;
using Microsoft.UI.Dispatching;

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public class Cfm2SettingsNavigationMaintenancePageViewModel : NavigationPageViewModel
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsNavigationMaintenancePageViewModel(WindowEx2 window)
            : base(window)
    {
        // コマンド
        ButtonCheckRssClickedCommand = new RelayCommand(ButtonCheckRssClicked);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // View 通信用のプロパティー
    // --------------------------------------------------------------------

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
                    MessageDialog messageDialog = _window.CreateMessageDialog("最新情報の確認を無効にすると、" + Cfm2Constants.APP_NAME_J
                            + "の新版がリリースされた際の更新内容などが表示されません。\n\n本当に無効にしてもよろしいですか？", Cfm2Constants.LABEL_WARNING);
                    messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_YES));
                    messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_NO));
                    IUICommand cmd = await messageDialog.ShowAsync();
                    if (cmd.Label != Cfm2Constants.LABEL_YES)
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
            await Cfm2Common.CheckLatestInfoAsync(true);
        }
        catch (Exception ex)
        {
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "今すぐ最新情報を確認時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
        finally
        {
            IsProgressRingActive = false;
            IsButtonCheckRssEnabled = true;
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
}
