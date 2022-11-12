// ============================================================================
// 
// メインページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using System.Diagnostics;
using CFileMerge2.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace CFileMerge2.Views;

public sealed partial class MainPage : Page
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public MainPage()
    {
        ViewModel = App.GetService<MainPageViewModel>();
        InitializeComponent();
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// ビューモデル
    /// </summary>
    public MainPageViewModel ViewModel
    {
        get;
    }
}
