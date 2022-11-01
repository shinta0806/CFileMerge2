// ============================================================================
// 
// 環境設定類を管理する
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System.Windows.Input;

using CFileMerge2.Contracts.Services;
using CFileMerge2.Models.SharedMisc;

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
        //SetLogWriter();

        // コマンド
        HelpClickedCommand = new RelayCommand<String>(HelpClicked);

        // 環境設定の Load() はしない（YlModel.Instance 生成途中で EnvironmentModel が生成され、エラー発生時に YukaListerModel.Instance 経由でのログ記録ができないため）
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
    public Cfm2Settings Cfm2Settings { get; private set; } = new();

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
            await App.MainWindow.CreateMessageDialog("ヘルプ表示時エラー：\n" + ex.Message, Cfm2Constants.LABEL_ERROR).ShowAsync();
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
    public async Task LoadCfm2Settings()
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
    public async Task SaveCfm2Settings()
    {
        try
        {
            await App.GetService<ILocalSettingsService>().SaveSettingAsync(Cfm2Constants.SETTINGS_KEY_CFM2_SETTINGS, Cfm2Settings);
        }
        catch (Exception)
        {
        }
    }
}
