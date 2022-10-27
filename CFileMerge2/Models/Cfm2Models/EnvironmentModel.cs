// ============================================================================
// 
// 環境設定類を管理する
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFileMerge2.Contracts.Services;
using CFileMerge2.Models.SharedMisc;
using Shinta;

namespace CFileMerge2.Models.Cfm2Models;
internal class EnvironmentModel
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    // --------------------------------------------------------------------
    // メインコンストラクター
    // --------------------------------------------------------------------
    public EnvironmentModel()
    {
        // 最初にログの設定をする
        //SetLogWriter();

        // 環境設定の Load() はしない（YlModel.Instance 生成途中で EnvironmentModel が生成され、エラー発生時に YukaListerModel.Instance 経由でのログ記録ができないため）
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // 一般プロパティー
    // --------------------------------------------------------------------

    // 環境設定
    public Cfm2Settings Cfm2Settings { get; private set; } = new();

    // EXE フルパス
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

    // EXE フォルダーのフルパス（末尾 '\\'）
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
