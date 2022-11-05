// ============================================================================
// 
// 言語リソースからの文字列取得
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using Microsoft.Windows.ApplicationModel.Resources;

using Serilog;

namespace CFileMerge2.Helpers;

public static class ResourceExtensions
{
    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 文字列取得
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <returns></returns>
    public static string GetLocalized(this string resourceKey)
    {
        try
        {
            return _resourceLoader.GetString(resourceKey);
        }
        catch (Exception)
        {
            Log.Error("言語リソースが見つかりません：" + resourceKey);
            return resourceKey;
        }
    }

    // ====================================================================
    // private 変数
    // ====================================================================

    private static readonly ResourceLoader _resourceLoader = new();
}
