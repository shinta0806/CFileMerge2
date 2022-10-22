﻿// ============================================================================
// 
// メインウィンドウのコードビハインド
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using CFileMerge2.Helpers;
using CFileMerge2.ViewModels;
using Microsoft.UI.Xaml;

using System.Diagnostics;

namespace CFileMerge2;

public sealed partial class MainWindow : WindowEx
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    // --------------------------------------------------------------------
    // メインコンストラクター
    // --------------------------------------------------------------------
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        //Title = "AppDisplayName".GetLocalized();

        Activated += WindowActivated;
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    // 初期化済
    private Boolean _initialized;

    // ====================================================================
    // private 関数
    // ====================================================================

    // --------------------------------------------------------------------
    // 必要に応じて初期化
    // --------------------------------------------------------------------
    private void InitializeIfNeeded()
    {
        if (_initialized)
        {
            return;
        }

        // なぜか MainWindow.xaml で Width, Height を指定しても効かないので、ここで指定する
        Width = 800;

        // Height は後で MainPageViewModel により指定されるはずなので、ここでは仮指定
        // 小さいと本来の高さを測定できないため、多少大きめに指定しておく
        Height = 200;

        // 初期化完了
        _initialized = true;
    }

    // --------------------------------------------------------------------
    // Activated / Deactivated
    // --------------------------------------------------------------------
    private void WindowActivated(Object sender, WindowActivatedEventArgs e)
    {
        if (e.WindowActivationState == WindowActivationState.CodeActivated)
        {
            InitializeIfNeeded();
        }
    }
}
