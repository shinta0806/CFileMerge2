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

    // 出力先ファイルのフルパス
    public String OutPath
    {
        get;
        set;
    } = String.Empty;

    // インクルードフォルダーのフルパス（末尾に '\\' はあってもなくても良い）
    public String IncludeFolder
    {
        get;
        set;
    } = String.Empty;

    // インクルード拡張子（先頭にピリオド）
    public String IncludeDefaultExt
    {
        get;
        set;
    } = Cfm2Constants.FILE_EXT_CFM2_MAKE;

    // インクルード履歴
    public List<String> IncludeStack
    {
        get;
        set;
    } = new();

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
