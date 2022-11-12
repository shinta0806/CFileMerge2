// ============================================================================
// 
// 環境設定ページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views.Cfm2SettingsWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog.Events;
using Serilog;

namespace CFileMerge2.ViewModels;

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
    }

    private Page _navigationViewContent;
    public Page NavigationViewContent
    {
        get => _navigationViewContent;
        set => SetProperty(ref _navigationViewContent, value);
    }


    // ====================================================================
    // public 関数
    // ====================================================================

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
            Log.Debug("PageLoaded()");
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

}
