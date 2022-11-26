﻿// ============================================================================
// 
// ナビゲーションページの ViewModel 基底クラス
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
using CFileMerge2.Models.Cfm2Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CFileMerge2.ViewModels;
public class NavigationPageViewModel : ObservableRecipient
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public NavigationPageViewModel()
    {
    }

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 入力されているプロパティーの妥当性を確認
    /// ＜例外＞ Exception
    /// </summary>
    public virtual void CheckProperties()
    {
    }

    /// <summary>
    /// プロパティーから設定に反映
    /// </summary>
    public virtual void PropertiesToSettings()
    {
    }

    /// <summary>
    /// 設定をプロパティーに反映
    /// </summary>
    public virtual void SettingsToProperties()
    {
    }
}
