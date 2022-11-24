﻿// ============================================================================
// 
// メインウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// ToDo: 子ウィンドウ表示を WindowEx2 へ、合併進捗表示をどうするか？
// コマンドライン起動、関連付け、id 属性なし警告、状況依存ヘルプ実装、更新起動メッセージ、ダイアログからの MessageDialog の親設定
// ----------------------------------------------------------------------------

using System.Diagnostics;

using CFileMerge2.Models.SharedMisc;

namespace CFileMerge2.Views.MainWindows;

public sealed partial class MainWindow : WindowEx2
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public MainWindow()
    {
        Debug.WriteLine("MainWindow() A");
        InitializeComponent();
        Debug.WriteLine("MainWindow() B");

        // 初期化
        SizeToContent = SizeToContent.Height;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
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
