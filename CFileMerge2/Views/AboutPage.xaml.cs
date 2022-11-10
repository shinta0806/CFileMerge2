// ============================================================================
// 
// バージョン情報ページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CFileMerge2.Views;

public sealed partial class AboutPage : Page
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public AboutPage(WindowEx window)
    {
        ViewModel = new AboutPageViewModel(window);
        InitializeComponent();

#if false
        var a = (Button)FindName("ButtonOk");
        var b = a.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
#endif
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// ビューモデル
    /// </summary>
    public AboutPageViewModel ViewModel
    {
        get;
    }
}
