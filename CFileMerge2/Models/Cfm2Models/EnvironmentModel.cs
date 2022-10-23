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
