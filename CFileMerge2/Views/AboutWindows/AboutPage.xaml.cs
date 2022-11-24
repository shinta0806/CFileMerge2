﻿// ============================================================================
// 
// バージョン情報ページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.AboutWindows;

using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace CFileMerge2.Views.AboutWindows;

public sealed partial class AboutPage : PageEx2
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public AboutPage(WindowEx2 window)
            : base(window)
    {
        ViewModel = new AboutPageViewModel(window);
        InitializeComponent();
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