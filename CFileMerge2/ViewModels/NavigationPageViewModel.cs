// ============================================================================
// 
// ナビゲーションページの ViewModel 基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using CFileMerge2.Views;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CFileMerge2.ViewModels;

public class NavigationPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public NavigationPageViewModel(WindowEx3 window)
    {
        _window = window;
    }

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 入力されているプロパティーの妥当性を確認
    /// ＜例外＞ Exception
    /// </summary>
    public virtual void CheckProperties()
    {
    }

    /// <summary>
    /// プロパティーから設定に反映
    /// </summary>
    public virtual void PropertiesToSettings()
    {
    }

    /// <summary>
    /// 設定をプロパティーに反映
    /// </summary>
    public virtual void SettingsToProperties()
    {
    }

    // ====================================================================
    // protected 変数
    // ====================================================================

    /// <summary>
    /// ウィンドウ
    /// </summary>
    protected readonly WindowEx3 _window;
}
