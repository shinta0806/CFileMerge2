// ============================================================================
// 
// メインウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ToDo: 環境設定、メイクファイル D&D、メイクファイル履歴、文字コード・改行コード保全、
// コマンドライン起動、関連付け、id 属性なし警告、状況依存ヘルプ用ファイル、状況依存ヘルプ実装
// 集約イベントハンドラー
// ----------------------------------------------------------------------------

using System.Diagnostics;

using CFileMerge2.Models.SharedMisc;

using Microsoft.UI.Xaml;

using Serilog;

using Shinta;

namespace CFileMerge2;

public sealed partial class MainWindow : WindowEx
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

        // チェック
        Debug.Assert(Cfm2Constants.CFM_TAG_KEYS.Length == (Int32)TagKey.__End__, "MainWindow() TAG_KEYS が変");
        Debug.Assert(Cfm2Constants.MERGE_STEP_AMOUNT.Length == (Int32)MergeStep.__End__, "MainWindow() MERGE_STEP_AMOUNT が変");

        // 初期化
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;

        // イベントハンドラー
        Activated += MainWindow_Activated;
        Closed += MainWindow_Closed;
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    /// <summary>
    /// 初期化済フラグ
    /// </summary>
    private Boolean _initialized;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 必要に応じて初期化
    /// </summary>
    private void InitializeIfNeeded()
    {
        if (_initialized)
        {
            return;
        }

        Debug.WriteLine("InitializeIfNeeded()");
#if DEBUG
        Title = "［デバッグ］" + Title;
#endif
#if TEST
        Title = "［テスト］" + Title;
#endif

        // なぜか MainWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        // Depend: 効くようになればこのコードは不要
        Width = 800;

        // Height は後で MainPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        // Depend: Window.SizeToContent が実装されればこのコードは不要
        Height = 200;

        // 初期化完了
        _initialized = true;
    }

    /// <summary>
    /// イベントハンドラー：Activated / Deactivated
    /// </summary>
    private void MainWindow_Activated(Object _, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.CodeActivated)
        {
            InitializeIfNeeded();
        }
    }

    /// <summary>
    /// イベントハンドラー：ウィンドウクローズ
    /// </summary>
    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        Log.Information("終了しました：" + Cfm2Constants.APP_NAME_J + " " + Cfm2Constants.APP_VER + " --------------------");
    }
}
