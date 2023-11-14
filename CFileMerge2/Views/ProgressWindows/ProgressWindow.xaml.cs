// ============================================================================
// 
// 進捗ウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using CFileMerge2.Models.SharedMisc;

using Shinta;
using Shinta.WinUi3;

namespace CFileMerge2.Views.ProgressWindows;

public sealed partial class ProgressWindow : WindowEx3
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public ProgressWindow(String makePath, Boolean onlyCore)
	{
		InitializeComponent();

		// 初期化
		SizeToContent = SizeToContent.WidthAndHeight;
		AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
		Title = "ProgressWindow_Title".ToLocalized();
		Content = new ProgressPage(this, makePath, onlyCore);
	}
}
