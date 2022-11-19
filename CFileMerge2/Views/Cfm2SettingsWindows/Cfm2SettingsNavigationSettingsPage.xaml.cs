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
    public Cfm2SettingsNavigationSettingsPage()
    {
        ViewModel = new Cfm2SettingsNavigationSettingsPageViewModel();
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
