// ============================================================================
// 
// 環境設定ウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Strings;

using Microsoft.UI.Windowing;

using Shinta.WinUi3;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsWindow : WindowEx3
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public Cfm2SettingsWindow()
	{
		InitializeComponent();

		// ヘルプボタン
		HelpButtonParameter = "Kankyousettei";
		IsHelpButtonEnabled = true;

		// 初期化
		SizeToContent = SizeToContent.Height;
		AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
		Title = Localize.Cfm2SettingsWindow_Title.Localized();
		Content = new Cfm2SettingsPage(this);
	}
}

