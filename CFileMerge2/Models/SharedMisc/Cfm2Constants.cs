﻿// ============================================================================
// 
// ちょちょいとファイル合併 2 共通で使用する定数
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

namespace CFileMerge2.Models.SharedMisc;

// ====================================================================
// public 列挙子
// ====================================================================

// Cfm タグのキー
public enum TagKey
{
    OutFile,            // 出力先ファイル
    IncludeFolder,      // インクルードフォルダー
    IncludeDefaultExt,  // インクルード拡張子
    Include,            // インクルード
    Set,                // 変数設定
    Var,                // 変数使用
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
    // 拡張子
    // --------------------------------------------------------------------
    public const String FILE_EXT_CFM2_MAKE = ".cfm2";

    // --------------------------------------------------------------------
    // UI
    // --------------------------------------------------------------------

    // よく使うラベル
    public const String LABEL_CONFIRM = "確認";
    public const String LABEL_ERROR = "エラー";
    public const String LABEL_NO = "いいえ";
    public const String LABEL_YES = "はい";

    // --------------------------------------------------------------------
    // 設定
    // --------------------------------------------------------------------

    // メイクファイル
    public const String SETTINGS_KEY_MAKE_PATH = "MakePath";

    // --------------------------------------------------------------------
    // タグ
    // --------------------------------------------------------------------

    // TagKey に対応するタグキー文字列
    public static readonly String[] TAG_KEYS = { "outfile", "includefolder", "includedefaultext", "include", "set", "var" };

}
