// ============================================================================
// 
// ウィンドウの拡張
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Windows.Input;

using CFileMerge2.Models.SharedMisc;

using CommunityToolkit.Mvvm.Input;

using Shinta.WinUi3.Views;

namespace CFileMerge2.Views;

public class WindowEx3 : WindowEx2
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public WindowEx3()
	{
		// コマンド
		HelpClickedCommand = new RelayCommand<String>(HelpClicked);
	}

	// ====================================================================
	// public プロパティー
	// ====================================================================

	// --------------------------------------------------------------------
	// コマンド
	// --------------------------------------------------------------------

	#region ヘルプリンクの制御
	public RelayCommand<String> HelpClickedCommand
	{
		get;
	}

	private async void HelpClicked(String? parameter)
	{
		await ShowHelpAsync(parameter);
	}
	#endregion

	// ====================================================================
	// protected 関数
	// ====================================================================

	/// <summary>
	/// ヘルプを表示
	/// </summary>
	/// <returns></returns>
	protected async override void ShowHelp()
	{
		await ShowHelpAsync(HelpButtonParameter);
	}

	// ====================================================================
	// private 関数
	// ====================================================================

	/// <summary>
	/// ヘルプを表示
	/// </summary>
	/// <param name="parameter"></param>
	/// <returns></returns>
	private async Task ShowHelpAsync(String? parameter)
	{
		try
		{
			await Cfm2Common.ShowHelpAsync(this, parameter);
		}
		catch (Exception ex)
		{
			await ShowExceptionLogContentDialogAsync("ヘルプ表示時エラー", ex);
		}
	}
}
