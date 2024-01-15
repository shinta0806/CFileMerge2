// ============================================================================
// 
// アプリケーションのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Strings;
using CFileMerge2.Views.MainWindows;

using Microsoft.UI.Xaml;

using Shinta;

namespace CFileMerge2;

public partial class App : Application
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	public App()
	{
		InitializeComponent();
	}

	// ====================================================================
	// protected 関数
	// ====================================================================

	/// <summary>
	/// イベントハンドラー：起動
	/// </summary>
	/// <param name="args"></param>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		try
		{
			base.OnLaunched(args);

			// モデル生成
			_ = Cfm2Model.Instance;

			// 集約エラーハンドラー設定
			UnhandledException += AppUnhandledException;

			// 各種初期化
			Initialize();

			// メインウィンドウを開く
			_mainWindow = new();
			_mainWindow.Activate();
		}
		catch (Exception ex)
		{
			SerilogUtils.LogException("起動時エラー", ex);
		}
	}

	// ====================================================================
	// private 変数
	// ====================================================================

	/// <summary>
	/// メインウィンドウ
	/// </summary>
	private MainWindow? _mainWindow;

	// ====================================================================
	// private 関数
	// ====================================================================

	/// <summary>
	/// 集約エラーハンドラー
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="args"></param>
	private void AppUnhandledException(Object _, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
	{
		// まずはログのみ
		String message = String.Empty;
		try
		{
			message += Localize.App_Fatal_UnhandledException.Localized() + "\n";
		}
		catch (Exception)
		{
		}
		message += args.Message + "\n" + args.Exception.Message + "\n" + args.Exception.InnerException?.Message + "\n" + args.Exception.StackTrace;
		Log.Fatal(message);

		// 表示
		try
		{
			// 集約エラーハンドラーの中で await するとデバッガーにも拾われないエラーが発生するので、Wait() にする
			_mainWindow?.CreateMessageDialog(message, Localize.Fatal.Localized()).ShowAsync().AsTask().Wait();
		}
		catch (Exception)
		{
		}

		Environment.Exit(1);
	}

	/// <summary>
	/// 初期化
	/// </summary>
	private static void Initialize()
	{
		// テンポラリフォルダー準備
		Common.InitializeTempFolder();

		// ログ
		Cfm2Common.LogEnvironmentInfo();

		// 環境設定読み込み
		// メインウィンドウで読み込むと await の関係でメインページと順番がちぐはぐになったりするので、ここで読み込む必要がある
		Cfm2Common.LoadNkm3Settings();
	}
}
