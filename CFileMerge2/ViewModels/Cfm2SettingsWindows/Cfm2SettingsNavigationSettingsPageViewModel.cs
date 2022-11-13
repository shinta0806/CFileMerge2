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
public class Cfm2SettingsNavigationSettingsPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsNavigationSettingsPageViewModel()
    {
        TocTargets = new(Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets);
        AnchorTargets = new(Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// h1～h6 に対して目次を作成するかどうか
    /// </summary>
    public ObservableCollection<Boolean> TocTargets
    {
        get;
    }

    /// <summary>
    /// h1～h6 に対してアンカーファイルを作成するかどうか
    /// </summary>
    public ObservableCollection<Boolean> AnchorTargets
    {
        get;
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
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets.Length; i++)
        {
            Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets[i] = TocTargets[i];
        }
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets.Length; i++)
        {
            Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets[i] = AnchorTargets[i];
        }
    }

    /// <summary>
    /// 設定をプロパティーに反映
    /// </summary>
    public void SettingsToProperties()
    {
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets.Length; i++)
        {
            TocTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets[i];
        }
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets.Length; i++)
        {
            AnchorTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets[i];
        }
    }

}
