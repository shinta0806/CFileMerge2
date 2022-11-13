// ============================================================================
// 
// 環境設定ページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.Cfm2SettingsWindows;

using Microsoft.UI.Xaml.Controls;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsPage : Page
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsPage(WindowEx window)
    {
        ViewModel = new Cfm2SettingsPageViewModel(window);
        InitializeComponent();
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// ビューモデル
    /// </summary>
    public Cfm2SettingsPageViewModel ViewModel
    {
        get;
    }

}
