// ============================================================================
// 
// ちょちょいとファイル合併 2 共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

namespace CFileMerge2.Models.SharedMisc;

// ====================================================================
// public 列挙子
// ====================================================================

/// <summary>
/// 合併作業のステップ
/// </summary>
public enum MergeStep
{
    ParseFile,
    InsertToc,
    Output,
    __End__,
}

/// <summary>
/// Cfm タグのキー
/// </summary>
public enum TagKey
{
    OutFile,            // 出力先ファイル
    IncludeFolder,      // インクルードフォルダー
    IncludeDefaultExt,  // インクルード拡張子
    Include,            // インクルード
    Set,                // 変数設定
    Var,                // 変数使用
    Toc,                // 目次
    __End__,
}

internal class Cfm2Constants
{
    // ====================================================================
    // public 定数
    // ====================================================================

    // --------------------------------------------------------------------
    // アプリの基本情報
    // --------------------------------------------------------------------

    public const String APP_ID = "CFileMerge2";
    public const String APP_NAME_J = "ちょちょいとファイル合併 2 ";
    public const String APP_VER = "Ver 1.00";
    public const String COPYRIGHT_J = "Copyright (C) 2022 by SHINTA";
#if DISTRIB_STORE
		public const String APP_DISTRIB = "ストア版";
#else
    public const String APP_DISTRIB = "zip 版";
#endif

    // --------------------------------------------------------------------
    // フォルダー名
    // --------------------------------------------------------------------

    /// <summary>
    /// ドキュメントフォルダー名
    /// </summary>
    public const String FOLDER_NAME_DOCUMENTS = "Documents\\";

    /// <summary>
    /// ヘルプ部品フォルダー名
    /// </summary>
    public const String FOLDER_NAME_HELP_PARTS = "HelpParts\\";

    /// <summary>
    /// サンプルフォルダー
    /// </summary>
    public const String FOLDER_NAME_SAMPLE = "Samples\\";

    // --------------------------------------------------------------------
    // 拡張子
    // --------------------------------------------------------------------

    /// <summary>
    /// メイクファイル
    /// </summary>
    public const String FILE_EXT_CFM2_MAKE = ".cfm2";

    // --------------------------------------------------------------------
    // UI
    // --------------------------------------------------------------------

    /// <summary>
    /// よく使うラベル
    /// </summary>
    public const String LABEL_CONFIRM = "確認";
    public const String LABEL_ERROR = "エラー";
    public const String LABEL_INFORMATION = "情報";
    public const String LABEL_NO = "いいえ";
    public const String LABEL_WARNING = "警告";
    public const String LABEL_YES = "はい";

    // --------------------------------------------------------------------
    // 設定
    // --------------------------------------------------------------------

    /// <summary>
    /// 環境設定
    /// </summary>
    public const String SETTINGS_KEY_CFM2_SETTINGS = "Cfm2Settings";

    // --------------------------------------------------------------------
    // Cfm タグ
    // --------------------------------------------------------------------

    /// <summary>
    /// TagKey に対応するタグキー文字列（小文字にする）
    /// </summary>
    public static readonly String[] CFM_TAG_KEYS = { "outfile", "includefolder", "includedefaultext", "include", "set", "var", "toc" };

    // --------------------------------------------------------------------
    // HTML タグ
    // --------------------------------------------------------------------

    /// <summary>
    /// <hx> タグの最小ランク <h1>
    /// </summary>
    public const Int32 HX_TAG_RANK_MIN = 1;

    /// <summary>
    /// <hx> タグの最大ランク <h6>
    /// </summary>
    public const Int32 HX_TAG_RANK_MAX = 6;

    /// <summary>
    /// Toc エリアクラス
    /// </summary>
    public const String TOC_AREA_CLASS_NAME = "TocArea";

    /// <summary>
    /// Toc アイテムクラスプレフィックス
    /// </summary>
    public const String TOC_ITEM_CLASS_NAME_PREFIX = "TocItemH";

    // --------------------------------------------------------------------
    // 合併作業量
    // --------------------------------------------------------------------

    /// <summary>
    /// MergeStep ごとの概算作業量（ParseFile を 100 とする）
    /// </summary>
    public static readonly Int32[] MERGE_STEP_AMOUNT = { 100, 100, 5 };

    /// <summary>
    /// 何行ごとに進捗表示するか
    /// </summary>
    public const Int32 PROGRESS_INTERVAL = 10;
}
