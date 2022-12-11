﻿// ============================================================================
// 
// 設定ページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Collections.ObjectModel;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Views;

using Shinta;

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public class Cfm2SettingsNavigationSettingsPageViewModel : Cfm2SettingsNavigationPageViewModel
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsNavigationSettingsPageViewModel(WindowEx3 window, Cfm2SettingsPageViewModel cfm2SettingsPageViewModel)
            : base(window, cfm2SettingsPageViewModel)
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

    /// <summary>
    /// アンカーファイルが既に存在する場合に上書きするかどうか
    /// </summary>
    private Boolean _overwriteAnchorFiles;
    public Boolean OverwriteAnchorFiles
    {
        get => _overwriteAnchorFiles;
        set => SetProperty(ref _overwriteAnchorFiles, value);
    }

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 入力されているプロパティーの妥当性を確認
    /// ＜例外＞ Exception
    /// </summary>
    public override void CheckProperties()
    {
        if (!TocTargets.Contains(true))
        {
            throw new Exception("Cfm2SettingsNavigationSettingsPageViewModel_CheckProperties_Error_Toc".ToLocalized());
        }
        if (!AnchorTargets.Contains(true))
        {
            throw new Exception("Cfm2SettingsNavigationSettingsPageViewModel_CheckProperties_Error_AnchorFile".ToLocalized());
        }
    }

    /// <summary>
    /// プロパティーから設定に反映
    /// </summary>
    public override void PropertiesToSettings()
    {
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets.Length; i++)
        {
            Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets[i] = TocTargets[i];
        }
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets.Length; i++)
        {
            Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets[i] = AnchorTargets[i];
        }
        Cfm2Model.Instance.EnvModel.Cfm2Settings.OverwriteAnchorFiles = OverwriteAnchorFiles;
    }

    /// <summary>
    /// 設定をプロパティーに反映
    /// </summary>
    public override void SettingsToProperties()
    {
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets.Length; i++)
        {
            TocTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets[i];
        }
        for (Int32 i = 0; i < Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets.Length; i++)
        {
            AnchorTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets[i];
        }
        OverwriteAnchorFiles = Cfm2Model.Instance.EnvModel.Cfm2Settings.OverwriteAnchorFiles;
    }
}
