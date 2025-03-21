// ============================================================================
// 
// ちょちょいとファイル合併 2 共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using Shinta;

using Windows.UI;

namespace CFileMerge2.Models.SharedMisc;

// ====================================================================
// public 列挙子
// ====================================================================

/// <summary>
/// 環境設定ウィンドウのナビゲーション
/// </summary>
public enum Cfm2SettingsNavigationViewItems
{
	Settings,
	Maintenance,
	__End__,
}

/// <summary>
/// 合併作業のステップ
/// </summary>
public enum MergeStep
{
	ParseFile,
	InsertToc,
	Output,
	OutputAnchor,
	__End__,
}

/// <summary>
/// Cfm タグのキー
/// </summary>
public enum TagKey
{
	OutFile,                // 出力先ファイル
	IncludeFolder,          // インクルードフォルダー
	IncludeDefaultExt,      // インクルード拡張子
	Include,                // インクルード
	Set,                    // 変数設定
	Var,                    // 変数使用
	Toc,                    // 目次
	GenerateAnchorFiles,    // アンカーファイルを作成
	AnchorPath,             // 出力先ファイルとアンカーへのパス
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
	public const String APP_VER = "Ver 4.19";
	public const String APP_COPYRIGHT = "Copyright (C) 2022-2025 by SHINTA";
	public const String APP_DISTRIB_WEB = "https://shinta.coresv.com/software_dev/cfilemerge2-jpn/";
	public const String APP_SUPPORT_WEB = APP_DISTRIB_WEB + "#Support";
	public const String APP_TRANSLATION_WEB = "https://docs.google.com/spreadsheets/d/1h3Ea7LamyQ2o5cnRxBxaVrzi6HuOM-EUij6h7mtBnGA/edit?usp=sharing";

	// --------------------------------------------------------------------
	// 作者情報
	// --------------------------------------------------------------------

	public const String AUTHOR_WEB = "https://shinta.coresv.com";
	public const String AUTHOR_TWITTER = "https://twitter.com/shinta0806";
	public const String AUTHOR_CREATOR_SUPPORT = "https://creator-support.nicovideo.jp/registration/9520206";
	public const String AUTHOR_FANTIA = "https://fantia.jp/fanclubs/65509";

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
	// ファイル名
	// --------------------------------------------------------------------

	/// <summary>
	/// 環境設定ファイル
	/// </summary>
	public const String FILE_NAME_CFM2_SETTINGS = "Cfm2Settings" + Common.FILE_EXT_JSON;

	// --------------------------------------------------------------------
	// 拡張子
	// --------------------------------------------------------------------

	/// <summary>
	/// メイクファイル
	/// </summary>
	public const String FILE_EXT_CFM2_MAKE = ".cfm2";

	// --------------------------------------------------------------------
	// バイナリ内部パス
	// --------------------------------------------------------------------

	/// <summary>
	/// アイコン
	/// </summary>
	public const String CONTENT_PATH_ICON = "Assets/WindowIcon.ico";

	// --------------------------------------------------------------------
	// URL
	// --------------------------------------------------------------------

	/// <summary>
	/// よくある質問
	/// </summary>
	public const String URL_FAQ = "https://github.com/shinta0806/CFileMerge2/issues?q=label%3Aquestion+sort%3Aupdated-desc";

	// --------------------------------------------------------------------
	// UI
	// --------------------------------------------------------------------

	/// <summary>
	/// ボタン幅のデフォルト
	/// </summary>
	public const Double BUTTON_WIDTH_DEFAULT = 120.0;

	/// <summary>
	/// ハイパーリンクボタンの余白
	/// </summary>
	public static readonly Thickness HYPERLINK_BUTTON_PADDING_THICKNESS = new(3.0);

	/// <summary>
	/// マージンのデフォルト
	/// </summary>
	public const Double MARGIN_DEFAULT = 20.0;

	/// <summary>
	/// マージンのデフォルト
	/// </summary>
	public static readonly Thickness MARGIN_DEFAULT_THICKNESS = new(MARGIN_DEFAULT);

	/// <summary>
	/// Border をセパレーターとして使う際の厚み
	/// </summary>
	public static readonly Thickness SEPARATOR_THICKNESS = new(1);

	/// <summary>
	/// Border をセパレーターとして使う際の色
	/// </summary>
	public static readonly SolidColorBrush SEPARATOR_BRUSH = new(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));

	/// <summary>
	/// 注釈の文字色
	/// </summary>
	public static readonly SolidColorBrush NOTE_BRUSH = new(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));

	/// <summary>
	/// 要素名
	/// </summary>
	public const String ELEMENT_NAME_BUTTON_OK = "ButtonOk";

	/// <summary>
	/// アイコンに使うフォント
	/// Segoe Fluent Icons は Windows 10 にはデフォルトでは入っていないとのことなので、MDL2 を使う
	/// </summary>
	public const String ICON_FONT = "Segoe MDL2 Assets";

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
	public static readonly String[] CFM_TAG_KEYS = ["outfile", "includefolder", "includedefaultext", "include", "set", "var", "toc", "generateanchorfiles", "anchorpath"];

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
	public static readonly Int32[] MERGE_STEP_AMOUNT = [100, 100, 5, 50];

	/// <summary>
	/// 何行ごとに進捗表示するか
	/// </summary>
	public const Int32 PROGRESS_INTERVAL = 10;

	// --------------------------------------------------------------------
	// その他
	// --------------------------------------------------------------------

	/// <summary>
	/// Microsoft Store での製品 ID
	/// </summary>
	public const String STORE_PRODUCT_ID = "9P71TMZL65WG";

	/// <summary>
	/// 最近使用したメイクファイルの最大数
	/// </summary>
	public const Int32 RECENT_MAKE_PATHES_MAX = 10;
}
