// ============================================================================
// 
// アプリケーションのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// The .NET Generic Host provides dependency injection, configuration, logging, and other services.
// https://docs.microsoft.com/dotnet/core/extensions/generic-host
// https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
// https://docs.microsoft.com/dotnet/core/extensions/configuration
// https://docs.microsoft.com/dotnet/core/extensions/logging
// ----------------------------------------------------------------------------

using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views;
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
	// public プロパティー
	// ====================================================================

#if false
	/// <summary>
	/// メインウィンドウ
	/// </summary>
	public static WindowEx3 MainWindow
	{
		get;
	} = new MainWindow();
#endif

	// ====================================================================
	// protected 関数
	// ====================================================================

	/// <summary>
	/// イベントハンドラー：起動
	/// </summary>
	/// <param name="args"></param>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		base.OnLaunched(args);

		// モデル生成
		_ = Cfm2Model.Instance;

		// 集約エラーハンドラー設定
		UnhandledException += App_UnhandledException;

		// 各種初期化
		Initialize();

		// メインウィンドウを開く
		_mainWindow = new();
		_mainWindow.Activate();
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
	private async void App_UnhandledException(Object _, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
	{
		// まずはログのみ
		String message = "不明なエラーが発生しました。アプリケーションを終了します。\n"
				+ args.Message + "\n" + args.Exception.Message + "\n" + args.Exception.InnerException?.Message + "\n" + args.Exception.StackTrace;
		Log.Fatal(message);

		// 表示
		try
		{
			await _mainWindow?.CreateMessageDialog(message, LogEventLevel.Fatal.ToString().ToLocalized()).ShowAsync();
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
