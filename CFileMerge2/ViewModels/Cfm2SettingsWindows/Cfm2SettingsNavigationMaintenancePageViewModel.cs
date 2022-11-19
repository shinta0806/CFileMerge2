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

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public class Cfm2SettingsNavigationMaintenancePageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsNavigationMaintenancePageViewModel()
    {
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// 最新情報を自動的に確認する
    /// </summary>
    private Boolean _checkRss;
    public Boolean CheckRss
    {
        get => _checkRss;
        set => SetProperty(ref _checkRss, value);
    }

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 入力されているプロパティーの妥当性を確認
    /// ＜例外＞ Exception
    /// </summary>
    public void CheckProperties()
    {
    }

    /// <summary>
    /// プロパティーから設定に反映
    /// </summary>
    public void PropertiesToSettings()
    {
        Cfm2Model.Instance.EnvModel.Cfm2Settings.CheckRss = CheckRss;
    }

    /// <summary>
    /// 設定をプロパティーに反映
    /// </summary>
    public void SettingsToProperties()
    {
        CheckRss = Cfm2Model.Instance.EnvModel.Cfm2Settings.CheckRss;
    }

}
