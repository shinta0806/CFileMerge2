// ============================================================================
// 
// バージョン情報ページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Windows.Input;

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Strings;
using CFileMerge2.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Shinta;

namespace CFileMerge2.ViewModels.AboutWindows;

public class AboutPageViewModel : ObservableRecipient
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public AboutPageViewModel(WindowEx3 window)
	{
		// 初期化
		_window = window;

		// コマンド
		ButtonCheckUpdateClickedCommand = new RelayCommand(ButtonCheckUpdateClicked);
		ButtonHistoryClickedCommand = new RelayCommand(ButtonHistoryClicked);
		ButtonOkClickedCommand = new RelayCommand(ButtonOkClicked);
	}

	// ====================================================================
	// public プロパティー
	// ====================================================================

	// --------------------------------------------------------------------
	// View 通信用のプロパティー
	// --------------------------------------------------------------------

	/// <summary>
	/// アプリケーション名
	/// </summary>
#pragma warning disable CA1822
	public String AppName
	{
		get => Localize.AppInfo_AppName.Localized();
	}

	/// <summary>
	/// 配布形態
	/// </summary>
	public String AppDistrib
	{
		get => Localize.AppInfo_Distribution_Store.Localized();
	}
#pragma warning restore CA1822

	// --------------------------------------------------------------------
	// コマンド
	// --------------------------------------------------------------------

	#region 更新プログラムの確認ボタンの制御
	public ICommand ButtonCheckUpdateClickedCommand
	{
		get;
	}

	private async void ButtonCheckUpdateClicked()
	{
		try
		{
			Common.OpenMicrosoftStore(Cfm2Constants.STORE_PRODUCT_ID);
		}
		catch (Exception ex)
		{
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.AboutPageViewModel_Error_ButtonCheckUpdateClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	#region 更新履歴ボタンの制御
	public ICommand ButtonHistoryClickedCommand
	{
		get;
	}

	private async void ButtonHistoryClicked()
	{
		try
		{
			Common.ShellExecute(Cfm2Model.Instance.EnvModel.ExeFullFolder + Cfm2Constants.FOLDER_NAME_DOCUMENTS + FILE_NAME_HISTORY);
		}
		catch (Exception ex)
		{
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.AboutPageViewModel_Error_ButtonHistoryClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}

	}
	#endregion

	#region OK ボタンの制御
	public ICommand ButtonOkClickedCommand
	{
		get;
	}

	private async void ButtonOkClicked()
	{
		try
		{
			_window.Close();
		}
		catch (Exception ex)
		{
			await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, Localize.AboutPageViewModel_Error_ButtonOkClicked.Localized() + "\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}
	#endregion

	// ====================================================================
	// public 関数
	// ====================================================================

	/// <summary>
	/// イベントハンドラー：Escape キー押下
	/// </summary>
	public void KeyboardAcceleratorEscapeInvoked(KeyboardAccelerator _1, KeyboardAcceleratorInvokedEventArgs _2)
	{
		ButtonOkClicked();
	}

	/// <summary>
	/// イベントハンドラー：ページがロードされた
	/// </summary>
	public void PageLoaded(Object _1, RoutedEventArgs _2)
	{
		try
		{
			Initialize();

			// フォーカス
			FrameworkElement frameworkElement = (FrameworkElement)_window.Content;
			Button button = (Button)frameworkElement.FindName(Cfm2Constants.ELEMENT_NAME_BUTTON_OK);
			button.Focus(FocusState.Programmatic);
		}
		catch (Exception ex)
		{
			// ユーザー起因では発生しないイベントなのでログのみ
			Log.Error("ページロード時エラー：\n" + ex.Message);
			SerilogUtils.LogStackTrace(ex);
		}
	}

	// ====================================================================
	// private 変数
	// ====================================================================

	private const String FILE_NAME_HISTORY = "CFileMerge2_History_JPN" + Common.FILE_EXT_TXT;

	// ====================================================================
	// private 変数
	// ====================================================================

	/// <summary>
	/// ウィンドウ
	/// </summary>
	private readonly WindowEx3 _window;

	// ====================================================================
	// private 関数
	// ====================================================================

	/// <summary>
	/// 初期化
	/// </summary>
	private static void Initialize()
	{
	}
}
