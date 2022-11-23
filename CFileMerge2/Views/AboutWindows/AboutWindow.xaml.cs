// ============================================================================
// 
// バージョン情報ウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

using CFileMerge2.Models.SharedMisc;
using WinUIEx;

namespace CFileMerge2.Views.AboutWindows;

public sealed partial class AboutWindow : WindowEx2
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public AboutWindow()
    {
        InitializeComponent();

        // 初期化
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Content = new AboutPage(this);

        // なぜか MainWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        // ToDo: 効くようになればこのコードは不要
        Width = 600;

        // Height は後で MainPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        // ToDo: SizeToContent が実装されればこのコードは不要
        Height = 600;
    }
}
