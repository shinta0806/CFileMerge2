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

    // タイトルバーの高さ
    public const Double TITLE_BAR_HEIGHT = 32.0;

    // よく使うラベル
    public const String LABEL_CONFIRM = "確認";
    public const String LABEL_ERROR = "エラー";
    public const String LABEL_NO = "いいえ";
    public const String LABEL_YES = "はい";
}
