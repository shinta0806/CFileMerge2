﻿// ============================================================================
// 
// メインページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.MainWindows;

namespace CFileMerge2.Views.MainWindows;

public sealed partial class MainPage : PageEx3
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public MainPage()
            : base((WindowEx3)App.MainWindow)
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
