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
	public ProgressPage(ProgressWindow progressWindow, String makePath)
		: base(progressWindow)
	{
		ViewModel = new(progressWindow, makePath);
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
