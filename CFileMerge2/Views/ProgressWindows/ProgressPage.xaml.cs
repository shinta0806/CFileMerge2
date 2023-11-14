// ============================================================================
// 
// 進捗ページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.ProgressWindows;

namespace CFileMerge2.Views.ProgressWindows;

public sealed partial class ProgressPage : PageEx3
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	/// <param name="progressWindow"></param>
	public ProgressPage(ProgressWindow progressWindow, String makePath, Boolean onlyCore)
		: base(progressWindow)
	{
		ViewModel = new(progressWindow, makePath, onlyCore);
		InitializeComponent();
	}

	// ====================================================================
	// public プロパティー
	// ====================================================================

	/// <summary>
	/// ビューモデル
	/// </summary>
	public ProgressPageViewModel ViewModel
	{
		get;
	}
}
