// ============================================================================
// 
// 合併作業用の情報
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
internal class MergeInfo
{
    // ====================================================================
    // public プロパティー
    // ====================================================================

    // メイクファイルのフルパス
    public String MakeFullPath
    {
        get;
        set;
    } = String.Empty;

    // 出力先ファイル
    public String OutPath
    {
        get;
        set;
    } = String.Empty;

    // インクルードフォルダー（末尾に '\\' はあってもなくても良い）
    public String IncludeFolder
    {
        get;
        set;
    } = String.Empty;

    // インクルード拡張子（先頭にピリオド）
    private String _includeDefaultExt = Cfm2Constants.FILE_EXT_CFM2_MAKE;
    public String IncludeDefaultExt
    {
        get => _includeDefaultExt;
        set
        {
            _includeDefaultExt = value;
            if (!String.IsNullOrEmpty(_includeDefaultExt) && _includeDefaultExt[0] != '.')
            {
                _includeDefaultExt = '.' + _includeDefaultExt;
            }
        }
    }

    // 合併後の内容
    public LinkedList<String> Lines
    {
        get;
    } = new();

    // 作業中に発生したエラー
    public List<String> Errors
    {
        get;
    } = new();
}
