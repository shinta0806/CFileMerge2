// ============================================================================
// 
// 設定ページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.Cfm2SettingsWindows;

using Microsoft.UI.Xaml.Controls;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsNavigationSettingsPage : Page
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsNavigationSettingsPage(WindowEx3 window, Cfm2SettingsPageViewModel cfm2SettingsPageViewModel)
    {
        ViewModel = new Cfm2SettingsNavigationSettingsPageViewModel(window, cfm2SettingsPageViewModel);
        InitializeComponent();
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// ビューモデル
    /// </summary>
    public Cfm2SettingsNavigationSettingsPageViewModel ViewModel
    {
        get;
    }
}
