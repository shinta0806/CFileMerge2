// ============================================================================
// 
// 環境設定ウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
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
using CFileMerge2.Models.SharedMisc;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CFileMerge2.Views.Cfm2SettingsWindows;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Cfm2SettingsWindow : WindowEx
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsWindow()
    {
        InitializeComponent();

        // 初期化
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Content = new Cfm2SettingsPage(this);

        // なぜか Cfm2SettingsWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        // ToDo: 効くようになればこのコードは不要
        Width = 600;

        // Height は後で Cfm2SettingsPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        // ToDo: SizeToContent が実装されればこのコードは不要
        Height = 400;

    }
}
