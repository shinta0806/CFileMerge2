// ============================================================================
// 
// 合併作業用の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

namespace CFileMerge2.Models.SharedMisc;

internal class MergeInfo
{
    // ====================================================================
    // public プロパティー
    // ====================================================================

    /// <summary>
    /// メイクファイルのフルパス
    /// </summary>
    public String MakeFullPath
    {
        get;
        set;
    } = String.Empty;

    /// <summary>
    /// 出力先ファイルのフルパス
    /// </summary>
    public String OutFullPath
    {
        get;
        set;
    } = String.Empty;

    /// <summary>
    /// インクルードフォルダーのフルパス（末尾に '\\' はあってもなくても良い）
    /// </summary>
    public String IncludeFullFolder
    {
        get;
        set;
    } = String.Empty;

    /// <summary>
    /// インクルード拡張子（先頭にピリオド）
    /// </summary>
    public String IncludeDefaultExt
    {
        get;
        set;
    } = Cfm2Constants.FILE_EXT_CFM2_MAKE;

    /// <summary>
    /// インクルード履歴
    /// </summary>
    public List<String> IncludeStack
    {
        get;
        set;
    } = new();

    /// <summary>
    /// メイクファイルで定義された変数（変数名、変数値）
    /// 変数名は小文字で格納
    /// </summary>
    public Dictionary<String, String> Vars
    {
        get;
        set;
    } = new();

    /// <summary>
    /// 目次作成が必要か（Cfm Toc タグがメイクファイルに存在するか）
    /// </summary>
    public Boolean TocNeeded
    {
        get;
        set;
    }

    /// <summary>
    /// 目次情報（HTML タグ）
    /// </summary>
    public List<String> Toc
    {
        get;
        set;
    } = new();

    /// <summary>
    /// 合併後の内容
    /// </summary>
    public LinkedList<String> Lines
    {
        get;
    } = new();

    /// <summary>
    /// 作業済の行数
    /// </summary>
    public Int32 NumProgressLines
    {
        get;
        set;
    }

    /// <summary>
    /// 合併後のトータル行数
    /// </summary>
    public Int32 NumTotalLines
    {
        get;
        set;
    }

    /// <summary>
    /// 作業中に発生した警告
    /// </summary>
    public List<String> Warnings
    {
        get;
    } = new();
}
