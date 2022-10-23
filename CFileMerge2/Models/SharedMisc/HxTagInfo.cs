// ============================================================================
// 
// HTML Hx タグの情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// <h1 id="hoge">見出し</h1>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFileMerge2.Models.SharedMisc;

internal class HxTagInfo
{
    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// 見出しランク（1～6）
    /// </summary>
    public Int32 Rank
    {
        get;
        set;
    }

    /// <summary>
    /// ID 属性
    /// </summary>
    public String Id
    {
        get;
        set;
    } = String.Empty;

    /// <summary>
    /// 見出し文字列
    /// </summary>
    public String Caption
    {
        get;
        set;
    } = String.Empty;
}
