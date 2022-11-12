// ============================================================================
// 
// 環境設定ページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CFileMerge2.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CFileMerge2.Views.Cfm2SettingsWindows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
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
