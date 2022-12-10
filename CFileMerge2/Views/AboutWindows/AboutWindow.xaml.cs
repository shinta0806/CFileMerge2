// ============================================================================
// 
// バージョン情報ウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

using CFileMerge2.Models.SharedMisc;

using Shinta;
using Shinta.WinUi3;

namespace CFileMerge2.Views.AboutWindows;

public sealed partial class AboutWindow : WindowEx3
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
        SizeToContent = SizeToContent.Height;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Title = String.Format("AboutWindow_Title".ToLocalized(), Cfm2Constants.LK_GENERAL_APP_NAME.ToLocalized());
        Content = new AboutPage(this);
    }
}
