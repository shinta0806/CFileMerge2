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

using System.Diagnostics;

using CFileMerge2.Activation;
using CFileMerge2.Contracts.Services;
using CFileMerge2.Core.Contracts.Services;
using CFileMerge2.Core.Services;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Services;
using CFileMerge2.ViewModels.MainWindows;
using CFileMerge2.Views;
using CFileMerge2.Views.MainWindows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using Serilog.Events;
using WinUIEx;

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

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<MainPage>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        // 集約エラーハンドラー設定
        UnhandledException += App_UnhandledException;
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// ホスト
    /// </summary>
    public IHost Host
    {
        get;
    }

    /// <summary>
    /// メインウィンドウ
    /// </summary>
    public static WindowEx MainWindow
    {
        get;
    } = new MainWindow();

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// サービス取得
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    // ====================================================================
    // protected 関数
    // ====================================================================

    /// <summary>
    /// イベントハンドラー：起動
    /// </summary>
    /// <param name="args"></param>
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // モデル生成
        _ = Cfm2Model.Instance;

        // 環境設定読み込み
        // メインウィンドウで読み込むと await の関係でメインページと順番がちぐはぐになったりするので、ここで読み込む必要がある
        await Cfm2Model.Instance.EnvModel.LoadCfm2SettingsAsync();

        // ここからメインウィンドウが実用になるようだ
        await App.GetService<IActivationService>().ActivateAsync(args);
    }

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
        Debug.WriteLine("App_UnhandledException() " + args.Exception.Message);
        await Cfm2Common.ShowLogMessageDialogAsync(LogEventLevel.Fatal, "不明なエラーが発生しました。アプリケーションを終了します。\n"
                + args.Message + "\n" + args.Exception.Message + "\n" + args.Exception.InnerException?.Message + "\n" + args.Exception.StackTrace);
        Environment.Exit(1);
    }
}
