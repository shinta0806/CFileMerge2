﻿// ============================================================================
// 
// メインウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// ToDo: 更新起動メッセージ
// ----------------------------------------------------------------------------

using System.Diagnostics;

using CFileMerge2.Models.SharedMisc;

using Shinta;
using Shinta.WinUi3;

namespace CFileMerge2.Views.MainWindows;

public sealed partial class MainWindow : WindowEx3
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // 初期化
        SizeToContent = SizeToContent.Height;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Title = Common.LK_GENERAL_APP_NAME.ToLocalized();
        Content = null;

        // なぜか MainWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        // ToDo: 効くようになればこのコードは不要
        Width = 800;

        // Height は後で MainPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        // ToDo: SizeToContent が実装されればこのコードは不要
        Height = 230;
    }
}
