// ============================================================================
// 
// 環境設定ページのナビゲーションページの ViewModel 基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using CFileMerge2.Views;

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public partial class Cfm2SettingsNavigationPageViewModel : NavigationPageViewModel
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsNavigationPageViewModel(WindowEx3 window, Cfm2SettingsPageViewModel cfm2SettingsPageViewModel)
            : base(window)
    {
        _cfm2SettingsPageViewModel = cfm2SettingsPageViewModel;
    }

    // ====================================================================
    // protected 変数
    // ====================================================================

    // 親ビューモデル
    protected readonly Cfm2SettingsPageViewModel _cfm2SettingsPageViewModel;
}
