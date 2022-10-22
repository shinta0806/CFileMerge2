// ============================================================================
// 
// Cfm タグの情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// <!-- Cfm/Key: Value -->
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFileMerge2.Models.SharedMisc;
internal class TagInfo
{
    // キー
    public TagKey Key
    {
        get;
        set;
    }

    // 値
    public String Value
    {
        get;
        set;
    } = String.Empty;
}
