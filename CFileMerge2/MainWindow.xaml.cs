// ============================================================================
// 
// メインウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System.Diagnostics;

using CFileMerge2.Models.SharedMisc;

using Microsoft.UI.Xaml;

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
        Activated += WindowActivated;
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
    /// Activated / Deactivated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void WindowActivated(Object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.CodeActivated)
        {
            InitializeIfNeeded();
        }
    }
}
