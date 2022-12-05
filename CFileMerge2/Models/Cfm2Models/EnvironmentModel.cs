// ============================================================================
// 
// 環境設定類を管理する
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Windows.Input;

using CFileMerge2.Contracts.Services;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Services;

using CommunityToolkit.Mvvm.Input;

using Serilog;
using Serilog.Events;

using Shinta;

namespace CFileMerge2.Models.Cfm2Models;

internal class EnvironmentModel
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public EnvironmentModel()
    {
        // 最初にログの設定をする
        SetLog();

        // コマンド
        HelpClickedCommand = new RelayCommand<String>(HelpClicked);

        // await できないため環境設定は読み込まない
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // 一般プロパティー
    // --------------------------------------------------------------------

    /// <summary>
    /// 環境設定
    /// </summary>
    public Cfm2Settings Cfm2Settings { get; set; } = new();

    /// <summary>
    /// EXE フルパス
    /// </summary>
    private String? _exeFullPath;
    public String ExeFullPath
    {
        get
        {
            if (_exeFullPath == null)
            {
                // 単一ファイル時にも内容が格納される GetCommandLineArgs を用いる（Assembly 系の Location は不可）
                _exeFullPath = Environment.GetCommandLineArgs()[0];
                if (Path.GetExtension(_exeFullPath).ToLower() != Common.FILE_EXT_EXE)
                {
                    _exeFullPath = Path.ChangeExtension(_exeFullPath, Common.FILE_EXT_EXE);
                }
            }
            return _exeFullPath;
        }
    }

    /// <summary>
    /// EXE フォルダーのフルパス（末尾 '\\'）
    /// </summary>
    private String? _exeFullFolder;
    public String ExeFullFolder
    {
        get
        {
            if (_exeFullFolder == null)
            {
                _exeFullFolder = Path.GetDirectoryName(ExeFullPath) + "\\";
            }
            return _exeFullFolder;
        }
    }

    /// <summary>
    /// アプリケーション終了時タスク安全中断用
    /// </summary>
    public CancellationTokenSource AppCancellationTokenSource
    {
        get;
    } = new();



    // --------------------------------------------------------------------
    // コマンド
    // --------------------------------------------------------------------

    #region ヘルプリンクの制御
    public ICommand HelpClickedCommand
    {
        get;
    }

    private async void HelpClicked(String? parameter)
    {
        try
        {
            await Cfm2Common.ShowHelpAsync(parameter);
        }
        catch (Exception ex)
        {
            await App.MainWindow.ShowLogMessageDialogAsync(LogEventLevel.Error, "ヘルプ表示時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 環境設定を読み込み
    /// </summary>
    /// <returns></returns>
    public async Task LoadCfm2SettingsAsync()
    {
        try
        {
            Cfm2Settings = await App.GetService<ILocalSettingsService>().ReadSettingAsync<Cfm2Settings>(Cfm2Constants.SETTINGS_KEY_CFM2_SETTINGS) ?? new Cfm2Settings();
            Cfm2Settings.Adjust();
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// 環境設定を保存
    /// </summary>
    /// <returns></returns>
    public Task SaveCfm2SettingsAsync()
    {
        try
        {
            return App.GetService<ILocalSettingsService>().SaveSettingAsync(Cfm2Constants.SETTINGS_KEY_CFM2_SETTINGS, Cfm2Settings);
        }
        catch (Exception)
        {
            return Task.CompletedTask;
        }
    }

    // ====================================================================
    // private 定数
    // ====================================================================

    /// <summary>
    /// ログファイル名
    /// </summary>
    private const String FILE_NAME_LOG = "Log" + Common.FILE_EXT_TXT;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// ログ設定
    /// </summary>
    private static void SetLog()
    {
        // ロガー生成
        SerilogUtils.CreateLogger(5 * 1024 * 1024, 5, ((LocalSettingsService)App.GetService<ILocalSettingsService>()).Folder() + FILE_NAME_LOG);

        // 起動ログ
        Log.Information("起動しました：" + Cfm2Constants.APP_NAME_J + " " + Cfm2Constants.APP_VER + " ====================");
        Log.Information("プロセス動作モード：" + (Environment.Is64BitProcess ? "64" : "32"));
#if DEBUG
        Log.Debug("デバッグモード：" + Common.DEBUG_ENABLED_MARK);
#endif
    }
}
