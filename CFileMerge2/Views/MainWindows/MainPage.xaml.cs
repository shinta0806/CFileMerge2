// ============================================================================
// 
// メインページのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.MainWindows;

using Microsoft.UI.Xaml;

using Shinta.WinUi3;

using Windows.Graphics;

namespace CFileMerge2.Views.MainWindows;

public sealed partial class MainPage : PageEx3
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public MainPage(MainWindow window)
			: base(window)
	{
		ViewModel = new(window);
		InitializeComponent();

		// イベントハンドラー
		Loaded += MainPageLoaded;
	}

	// ====================================================================
	// public プロパティー
	// ====================================================================

	/// <summary>
	/// ビューモデル
	/// </summary>
	public MainPageViewModel ViewModel
	{
		get;
	}

	// ====================================================================
	// private 関数
	// ====================================================================

	/// <summary>
	/// イベントハンドラー：ページがロードされた
	/// </summary>
	/// <param name="_1"></param>
	/// <param name="_2"></param>
	private void MainPageLoaded(Object _1, RoutedEventArgs _2)
	{
		// 自動で横幅を復元した場合、ユーザーが表示スケールの変更を繰り返すと横幅がどんどん大きくなってしまう（チケット 37）
		// 表示スケールを考慮した横幅をここで指定する
		Int32 height = Window.AppWindow.Size.Height;
		Double scale = WinUi3Common.DisplayScale(Window);
		Window.AppWindow.Resize(new SizeInt32((Int32)(800 * scale), height));
	}
}
