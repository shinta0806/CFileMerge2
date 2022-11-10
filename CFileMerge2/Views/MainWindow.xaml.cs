﻿// ============================================================================
// 
// メインウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ウィンドウでの MVVM が困難であると思われるので、ウィンドウへの操作はページのビューモデルで行う
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// ToDo: 環境設定、メイクファイル履歴、文字コード・改行コード保全、アンカー文字コード
// コマンドライン起動、関連付け、id 属性なし警告、状況依存ヘルプ実装
// 起動中はアンカーファイルの更新が反映されない？
// ----------------------------------------------------------------------------

using CFileMerge2.Models.SharedMisc;

namespace CFileMerge2.Views;

public sealed partial class MainWindow : WindowEx
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // 初期化
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Content = null;
    }
}
