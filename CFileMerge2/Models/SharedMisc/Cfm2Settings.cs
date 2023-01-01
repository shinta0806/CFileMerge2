// ============================================================================
// 
// ちょちょいとファイル合併 2 の設定を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

namespace CFileMerge2.Models.SharedMisc;

internal class Cfm2Settings
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2Settings()
    {
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // 設定
    // --------------------------------------------------------------------

    /// <summary>
    /// h1～h6 に対して目次を作成するかどうか
    /// h1 = [1]
    /// </summary>
    public Boolean[] TocTargets
    {
        get;
        set;
    } = new Boolean[Cfm2Constants.HX_TAG_RANK_MAX + 1];

    /// <summary>
    /// h1～h6 に対してアンカーファイルを作成するかどうか
    /// h1 = [1]
    /// </summary>
    public Boolean[] AnchorTargets
    {
        get;
        set;
    } = new Boolean[Cfm2Constants.HX_TAG_RANK_MAX + 1];

    /// <summary>
    /// アンカーファイルが既に存在する場合に上書きするかどうか
    /// </summary>
    public Boolean OverwriteAnchorFiles
    {
        get;
        set;
    } = true;

    // --------------------------------------------------------------------
    // メンテナンス
    // --------------------------------------------------------------------

    /// <summary>
    /// 最新情報を自動的に確認する
    /// </summary>
    public Boolean CheckRss
    {
        get;
        set;
    } = true;

    // --------------------------------------------------------------------
    // 終了時の状態（ちょちょいとファイル合併 2 専用）
    // --------------------------------------------------------------------

    /// <summary>
    /// メイクファイルのパス
    /// </summary>
    public String MakePath
    {
        get;
        set;
    } = String.Empty;

    /// <summary>
    /// 最近使用したメイクファイル
    /// 先頭が最新
    /// </summary>
    public List<String> RecentMakePathes2
    {
        get;
        set;
    } = new();

    // --------------------------------------------------------------------
    // 終了時の状態（一般）
    // --------------------------------------------------------------------

    /// <summary>
    /// 前回起動時のバージョン
    /// </summary>
    public String PrevLaunchVer
    {
        get;
        set;
    } = String.Empty;

    /// <summary>
    /// 前回起動時のパス
    /// </summary>
    public String PrevLaunchPath
    {
        get;
        set;
    } = String.Empty;

    /// <summary>
    /// RSS 確認日
    /// </summary>
    public DateTime RssCheckDate
    {
        get;
        set;
    }

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 調整
    /// </summary>
    public void Adjust()
    {
        if (!TocTargets.Contains(true))
        {
            // 目次作成対象が 1 つもない場合はデフォルトを対象にする
            TocTargets[1] = true;
            TocTargets[2] = true;
        }

        if (!AnchorTargets.Contains(true))
        {
            // アンカー作成対象が 1 つもない場合はデフォルトを対象にする
            AnchorTargets[1] = true;
            AnchorTargets[2] = true;
        }
    }

    /// <summary>
    /// RSS の確認が必要かどうか
    /// </summary>
    /// <returns></returns>
    public Boolean IsCheckRssNeeded()
    {
#if DEBUGz
        return true;
#endif
        if (!CheckRss)
        {
            return false;
        }
        DateTime emptyDate = new();
        TimeSpan day3 = new(3, 0, 0, 0);
        return RssCheckDate == emptyDate || DateTime.Now.Date - RssCheckDate >= day3;
    }
}
