// ============================================================================
// 
// 進捗ページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Text.RegularExpressions;
using System.Text;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Views.MainWindows;
using CFileMerge2.Views.ProgressWindows;

using CommunityToolkit.Mvvm.ComponentModel;
using Hnx8.ReadJEnc;
using Microsoft.UI.Xaml;
using Shinta;
using Microsoft.UI.Dispatching;

namespace CFileMerge2.ViewModels.ProgressWindows;

public class ProgressPageViewModel : ObservableRecipient
{
	// ====================================================================
	// コンストラクター
	// ====================================================================

	/// <summary>
	/// メインコンストラクター
	/// </summary>
	/// <param name="progressWindow"></param>
	public ProgressPageViewModel(ProgressWindow progressWindow, String makePath)
	{
		// 初期化
		_progressWindow = progressWindow;
		_makePath = makePath;
	}

	// ====================================================================
	// public プロパティー
	// ====================================================================

	// --------------------------------------------------------------------
	// View 通信用のプロパティー
	// --------------------------------------------------------------------

	/// <summary>
	/// 進捗（0～1 の小数）
	/// </summary>
	private Double _progressValue;
	public Double ProgressValue
	{
		get => _progressValue;
		set => SetProperty(ref _progressValue, value);
	}

	// --------------------------------------------------------------------
	// 一般のプロパティー
	// --------------------------------------------------------------------

	/// <summary>
	/// 合併作業用の情報
	/// </summary>
	public MergeInfo MergeInfo
	{
		get;
	} = new();

	// ====================================================================
	// public 関数
	// ====================================================================

	public async void PageLoaded(Object _1, RoutedEventArgs _2)
	{
		try
		{
			Log.Information("合併を開始します：" + _makePath);
			Int32 startTick = Environment.TickCount;

			// 出力
			Exception? outputTaskException = await Task.Factory.StartNew(Merge, TaskCreationOptions.LongRunning);
			if (outputTaskException != null)
			{
				throw outputTaskException;
			}

			// 報告
			if (MergeInfo.Warnings.Any())
			{
				// 警告あり
				String message = "MainPageViewModel_MergeAsync_Warnings".ToLocalized() + "\n";
				for (Int32 i = 0; i < MergeInfo.Warnings.Count; i++)
				{
					message += MergeInfo.Warnings[i] + "\n";
				}
				await _progressWindow.ShowLogMessageDialogAsync(LogEventLevel.Warning, message);
			}
			else
			{
				// 完了
				await _progressWindow.ShowLogMessageDialogAsync(LogEventLevel.Information,
					String.Format("MainPageViewModel_MergeAsync_Done".ToLocalized(), (Environment.TickCount - startTick).ToString("#,0")));
			}
		}
		catch (OperationCanceledException ex)
		{
			SerilogUtils.LogException("合併キャンセル", ex);
		}
		catch (Exception ex)
		{
			await _progressWindow.ShowExceptionLogMessageDialogAsync("MainPageViewModel_MergeAsync_Error".ToLocalized(), ex);
		}
		finally
		{
			_progressWindow.Close();
		}
	}

	// ====================================================================
	// private 定数
	// ====================================================================

	/// <summary>
	/// アンカーファイルの出力先フォルダー
	/// </summary>
	private const String ANCHOR_OUT_DEFAULT = "HelpParts";

	// ====================================================================
	// private 変数
	// ====================================================================

	/// <summary>
	/// メイクファイルのパス（相対パスも可）
	/// </summary>
	private readonly String _makePath;

	/// <summary>
	/// 進捗ウィンドウ
	/// </summary>
	private readonly ProgressWindow _progressWindow;

	// ====================================================================
	// private 関数
	// ====================================================================

	/// <summary>
	/// Anchor タグを実行
	/// </summary>
	private void ExecuteCfmTagAnchorPath(LinkedListNode<String> line, Int32 column)
	{
		MergeInfo.AnchorPositions.Add(new(line, column));
	}

	/// <summary>
	/// GenerateAnchorFiles タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	private void ExecuteCfmTagGenerateAnchorFiles(CfmTagInfo tagInfo)
	{
		String[] tagValues = tagInfo.Value.Split(',');
		for (Int32 i = 0; i < tagValues.Length; i++)
		{
			tagValues[i] = tagValues[i].Trim();
		}

		// アンカーメイクファイル
		MergeInfo.AnchorMakeFullPath = GetPathByMakeFullPath(tagValues[0], String.Format("MainPageViewModel_SrcName_AnchorMakeFile".ToLocalized(), TagKey.GenerateAnchorFiles.ToString()));

		// アンカー出力先
		String outSrc;
		if (tagValues.Length < 2)
		{
			// デフォルトの出力先
			outSrc = ANCHOR_OUT_DEFAULT;
		}
		else
		{
			if (String.IsNullOrEmpty(tagValues[1]))
			{
				// デフォルトの出力先
				outSrc = ANCHOR_OUT_DEFAULT;
			}
			else
			{
				// タグ値
				outSrc = tagValues[1];
			}
		}
		MergeInfo.AnchorOutFullFolder = GetPath(outSrc, Path.GetDirectoryName(MergeInfo.OutFullPath) ?? String.Empty, "MainPageViewModel_SrcName_AnchorOutFolder".ToLocalized()) + "\\";

		// アンカーファイル作成対象
		if (tagValues.Length >= 3)
		{
			MergeInfo.AnchorTargets = GetTargetRanks(tagValues[2], MergeInfo.AnchorTargets);
		}
	}

	/// <summary>
	/// Include タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <param name="parentLines">親となる行群</param>
	/// <param name="parentLine">挿入位置</param>
	/// <exception cref="Exception"></exception>
	private void ExecuteCfmTagInclude(CfmTagInfo tagInfo, LinkedList<String> parentLines, LinkedListNode<String> parentLine)
	{
		// インクルードパスを取得（IncludeFolder を加味する必要があるため GetPath() は使えない）
		String path;
		if (String.IsNullOrEmpty(tagInfo.Value))
		{
			throw new Exception(String.Format("MainPageViewModel_ExecuteCfmTagInclude_Error_NoPath".ToLocalized(), Cfm2Constants.CFM_TAG_KEYS[(Int32)tagInfo.Key]));
		}
		if (Path.IsPathRooted(tagInfo.Value))
		{
			// 絶対パス
			path = tagInfo.Value;
		}
		else
		{
			// インクルードフォルダーからの相対パス
			Debug.Assert(!String.IsNullOrEmpty(MergeInfo.IncludeFullFolder), "ExecuteTagInclude() IncludeFolder が初期化されていない");
			path = Path.GetFullPath(tagInfo.Value, MergeInfo.IncludeFullFolder);
		}
		if (!Path.HasExtension(path))
		{
			path += MergeInfo.IncludeDefaultExt;
		}

		// インクルード
		ParseFile(path, parentLines, parentLine);
	}

	/// <summary>
	/// IncludeDefaultExt タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	private void ExecuteCfmTagIncludeDefaultExt(CfmTagInfo tagInfo)
	{
		MergeInfo.IncludeDefaultExt = tagInfo.Value;
		if (!String.IsNullOrEmpty(MergeInfo.IncludeDefaultExt) && MergeInfo.IncludeDefaultExt[0] != '.')
		{
			MergeInfo.IncludeDefaultExt = '.' + MergeInfo.IncludeDefaultExt;
		}
	}

	/// <summary>
	/// IncludeFolder タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <exception cref="Exception"></exception>
	private void ExecuteCfmTagIncludeFolder(CfmTagInfo tagInfo)
	{
		MergeInfo.IncludeFullFolder = GetPathByMakeFullPath(tagInfo);
		if (!Directory.Exists(MergeInfo.IncludeFullFolder))
		{
			// 続行可能といえば続行可能であるが、Include できない際に原因が分かりづらくなるのでここでエラーとする
			throw new Exception("MainPageViewModel_ExecuteCfmTagIncludeFolder_Error_NoExist".ToLocalized() + "\n" + tagInfo.Value);
		}
	}

	/// <summary>
	/// OutFile タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <exception cref="Exception"></exception>
	private void ExecuteCfmTagOutFile(CfmTagInfo tagInfo)
	{
		MergeInfo.OutFullPath = GetPathByMakeFullPath(tagInfo);
		Debug.WriteLine("ExexuteTagOutFile() " + MergeInfo.OutFullPath);
		if (String.Compare(MergeInfo.OutFullPath, MergeInfo.MakeFullPath, true) == 0)
		{
			throw new Exception("MainPageViewModel_ExecuteCfmTagOutFile_Error_SameFile".ToLocalized());
		}
	}

	/// <summary>
	/// Set タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	private void ExecuteCfmTagSet(CfmTagInfo tagInfo)
	{
		// 変数名と変数値に分割
		Int32 eqPos = tagInfo.Value.IndexOf('=');
		if (eqPos < 0)
		{
			MergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagSet_Warning_NoEq".ToLocalized() + tagInfo.Value);
			return;
		}

		String varName = tagInfo.Value[0..eqPos].Trim().ToLower();
		if (String.IsNullOrEmpty(varName))
		{
			MergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagSet_Warning_NoName".ToLocalized() + tagInfo.Value);
			return;
		}

		String varValue = tagInfo.Value[(eqPos + 1)..].Trim();

		MergeInfo.Vars[varName] = varValue;
	}

	/// <summary>
	/// Toc タグを実行
	/// </summary>
	private void ExecuteCfmTagToc(CfmTagInfo tagInfo)
	{
		// 実際に目次を作成するのは、メイクファイルをすべて読み込んだ後なので、ここでは必要性の記録に留める
		MergeInfo.TocNeeded = true;
		MergeInfo.TocTargets = GetTargetRanks(tagInfo.Value, MergeInfo.TocTargets);
	}

	/// <summary>
	/// Var タグを実行
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <param name="line">タグのある行</param>
	/// <param name="column">タグのある桁</param>
	private void ExecuteCfmTagVar(CfmTagInfo tagInfo, LinkedListNode<String> line, Int32 column)
	{
		// 変数名取得
		String varName = tagInfo.Value.Trim().ToLower();
		if (String.IsNullOrEmpty(varName))
		{
			MergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagVar_Warning_NoName".ToLocalized());
			return;
		}
		if (!MergeInfo.Vars.ContainsKey(varName))
		{
			MergeInfo.Warnings.Add("MainPageViewModel_ExecuteCfmTagVar_Warning_NoDeclare".ToLocalized() + tagInfo.Value);
			return;
		}

		line.Value = line.Value.Insert(column, MergeInfo.Vars[varName]);
	}

	/// <summary>
	/// フルパスを取得（絶対パスまたは相対パスより）
	/// </summary>
	/// <param name="path">絶対パスまたは相対パス</param>
	/// <param name="basePath">相対パスの基準フォルダー</param>
	/// <param name="srcName">記述元（エラー表示用）</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private static String GetPath(String path, String basePath, String srcName)
	{
		Log.Debug("GetPath() srcName: " + srcName);
		if (String.IsNullOrEmpty(path))
		{
			throw new Exception(String.Format("MainPageViewModel_GetPath_Error_NoPath".ToLocalized(), srcName));
		}
		if (Path.IsPathRooted(path))
		{
			// 絶対パス
			return path;
		}
		else
		{
			// 相対パス
			return Path.GetFullPath(path, basePath);
		}
	}

	/// <summary>
	/// フルパスを取得（絶対パスまたはメイクファイルからの相対パスより）
	/// </summary>
	/// <param name="path">絶対パスまたはメイクファイルからの相対パス</param>
	/// <param name="srcName">記述元（エラー表示用）</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private String GetPathByMakeFullPath(String path, String srcName)
	{
		return GetPath(path, Path.GetDirectoryName(MergeInfo.MakeFullPath) ?? String.Empty, srcName);
	}

	/// <summary>
	/// タグの値からフルパスを取得（絶対パスまたはメイクファイルからの相対パスより）
	/// </summary>
	/// <param name="tagInfo">タグ情報</param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private String GetPathByMakeFullPath(CfmTagInfo tagInfo)
	{
		return GetPathByMakeFullPath(tagInfo.Value, String.Format("MainPageViewModel_SrcName_Tag".ToLocalized(), Cfm2Constants.CFM_TAG_KEYS[(Int32)tagInfo.Key]));
	}

	/// <summary>
	/// Hx タグの対象ランクを取得
	/// </summary>
	/// <param name="rankString">対象ランクを列挙した文字列（例："13" なら h1 と h3 が対象）</param>
	/// <param name="defaultTargetRanks">取得不可の場合に返すデフォルト対象ランク</param>
	/// <returns></returns>
	private static Boolean[] GetTargetRanks(String rankString, Boolean[] defaultTargetRanks)
	{
		if (String.IsNullOrEmpty(rankString))
		{
			return defaultTargetRanks;
		}

		Boolean[] targetRanks = new Boolean[Cfm2Constants.HX_TAG_RANK_MAX + 1];
		for (Int32 i = 0; i < rankString.Length; i++)
		{
			if (Int32.TryParse(rankString[i..(i + 1)], out Int32 rank) && 0 < rank && rank <= Cfm2Constants.HX_TAG_RANK_MAX)
			{
				targetRanks[rank] = true;
			}
		}

		if (!targetRanks.Contains(true))
		{
			return defaultTargetRanks;
		}

		return targetRanks;
	}

	/// <summary>
	/// 合併タスク
	/// 非同期に実行されることを想定している
	/// 続行不可能なエラーは直ちに Exception で中止する
	/// 続行可能なエラーは MergeInfo.Errors に貯めて最後に表示する
	/// </summary>
	/// <returns>続行不可能なエラー発生時は例外を返す</returns>
	private Exception? Merge()
	{
		Exception? exception = null;
		try
		{
			MergeCore();

			// 目次作成
			SetProgressValue(MergeStep.InsertToc, 0.0);
			if (MergeInfo.TocNeeded)
			{
				ParseCfmTagsForToc();
			}

			// 出力
			SetProgressValue(MergeStep.Output, 0.0);
			Directory.CreateDirectory(Path.GetDirectoryName(MergeInfo.OutFullPath) ?? String.Empty);
			Write(MergeInfo.OutFullPath, MergeInfo.Lines);

			// アンカー出力
			SetProgressValue(MergeStep.OutputAnchor, 0.0);
			if (!String.IsNullOrEmpty(MergeInfo.AnchorMakeFullPath))
			{
				OutputAnchors();
			}
			SetProgressValue(MergeStep.OutputAnchor, 100.0);

#if DEBUGz
                Thread.Sleep(5 * 1000);
#endif

		}
		catch (Exception ex)
		{
			exception = ex;
		}
		return exception;
	}

	/// <summary>
	/// 合併コア
	/// 出力は行わない
	/// </summary>
	private void MergeCore()
	{
		// デフォルト値を設定
		MergeInfo.MakeFullPath = Path.GetFullPath(_makePath, Cfm2Model.Instance.EnvModel.ExeFullFolder);
		MergeInfo.IncludeFullFolder = Path.GetDirectoryName(MergeInfo.MakeFullPath) ?? String.Empty;
		MergeInfo.OutFullPath = Path.GetDirectoryName(MergeInfo.MakeFullPath) + "\\" + Path.GetFileNameWithoutExtension(MergeInfo.MakeFullPath) + "Output" + Common.FILE_EXT_HTML;
		for (Int32 i = 0; i < MergeInfo.TocTargets.Length; i++)
		{
			MergeInfo.TocTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.TocTargets[i];
			MergeInfo.AnchorTargets[i] = Cfm2Model.Instance.EnvModel.Cfm2Settings.AnchorTargets[i];
		}

		// メイクファイル読み込み（再帰）
		(MergeInfo.Encoding, MergeInfo.NewLine) = ParseFile(MergeInfo.MakeFullPath, MergeInfo.Lines, null);
	}

	/// <summary>
	/// アンカーファイル群を出力
	/// </summary>
	private void OutputAnchors()
	{
		// アンカー出力先フォルダー作成
		Directory.CreateDirectory(MergeInfo.AnchorOutFullFolder);

		// アンカーファイル作成対象の Hx タグを検索
		List<HxTagInfo> hxTagInfos = ParseHxTags(MergeInfo.AnchorTargets);

		// アンカーメイクファイル読み込み（再帰）
		MergeInfo.AnchorPositions.Clear();
		LinkedList<String> lines = new();
		ParseFile(MergeInfo.AnchorMakeFullPath, lines, null);

		// アンカー挿入位置の行番号を計算
		List<KeyValuePair<Int32, Int32>> anchorPositionIndexes = new();
		for (Int32 i = 0; i < MergeInfo.AnchorPositions.Count; i++)
		{
			Int32 lineIndex = -1;
			LinkedListNode<String>? line = MergeInfo.AnchorPositions[i].Key;
			while (line != null)
			{
				lineIndex++;
				line = line.Previous;
			}
			anchorPositionIndexes.Add(new(lineIndex, MergeInfo.AnchorPositions[i].Value));
		}

		// アンカーファイルのフォルダーを基準とした時の出力ファイルの相対パス
		String relativePath = Path.GetRelativePath(MergeInfo.AnchorOutFullFolder, MergeInfo.OutFullPath).Replace('\\', '/');

		// アンカーファイル出力
		for (Int32 i = 0; i < hxTagInfos.Count; i++)
		{
			String anchorPath = MergeInfo.AnchorOutFullFolder + Path.GetFileNameWithoutExtension(MergeInfo.OutFullPath) + "_" + hxTagInfos[i].Id + Common.FILE_EXT_HTML;
			if (!Cfm2Model.Instance.EnvModel.Cfm2Settings.OverwriteAnchorFiles && File.Exists(anchorPath))
			{
				// 既にアンカーファイルが存在していて上書きしない場合は何もしない
			}
			else
			{
				List<String> anchorFileContents = new(lines);

				// アンカー置換
				for (Int32 j = 0; j < anchorPositionIndexes.Count; j++)
				{
					anchorFileContents[anchorPositionIndexes[j].Key] = anchorFileContents[anchorPositionIndexes[j].Key].Insert(anchorPositionIndexes[j].Value, relativePath + "#" + hxTagInfos[i].Id);
				}

				Write(anchorPath, anchorFileContents);
			}

			if (i % Cfm2Constants.PROGRESS_INTERVAL == 0)
			{
				SetProgressValue(MergeStep.OutputAnchor, (Double)i / hxTagInfos.Count);
#if DEBUGz
                Thread.Sleep(500);
#endif
			}
		}
	}

	/// <summary>
	/// Cfm タグがあれば抽出する
	/// 抽出したタグは line から削除する
	/// </summary>
	/// <param name="line">解析対象行</param>
	/// <param name="column">解析開始桁</param>
	/// <param name="removeTocTag">抽出したタグが Toc タグだった場合に削除するかどうか</param>
	/// <returns>抽出した Cfm タグ, 次の解析開始桁</returns>
	private (CfmTagInfo? tagInfo, Int32 column) ParseCfmTag(LinkedListNode<String> line, Int32 column, Boolean removeTocTag)
	{
		if (column >= line.Value.Length)
		{
			// 行末まで解析した
			return (null, column);
		}

		Match match = Regex.Match(line.Value[column..], @"\<\!\-\-\s*?cfm\/(.+?)\-\-\>", RegexOptions.IgnoreCase);
		if (!match.Success)
		{
			// Cfm タグが無い
			return (null, line.Value.Length);
		}

		Debug.Assert(match.Groups.Count >= 2, "ParseCfmTag() match.Groups が不足");
		Int32 addColumn = match.Index + match.Length;
		String tagContent = match.Groups[1].Value;
		Int32 colon = tagContent.IndexOf(':');
		if (colon < 0)
		{
			// キーと値を区切る ':' が無い
			Log.Debug("ParseTag() 区切りコロン無し, " + tagContent + ", add: " + addColumn);
			MergeInfo.Warnings.Add("MainPageViewModel_ParseCfmTag_Warning_NoColon".ToLocalized() + tagContent);
			return (null, addColumn);
		}

		Int32 key = Array.IndexOf(Cfm2Constants.CFM_TAG_KEYS, tagContent[0..colon].Trim().ToLower());
		if (key < 0)
		{
			Log.Debug("ParseTag() サポートされていないキー, " + tagContent + ", add: " + addColumn);
			MergeInfo.Warnings.Add("MainPageViewModel_ParseCfmTag_Warning_NotSupported" + tagContent);
			return (null, addColumn);
		}

		CfmTagInfo tagInfo = new()
		{
			Key = (TagKey)key,
			Value = tagContent[(colon + 1)..].Trim(),
		};

		if (!removeTocTag && tagInfo.Key == TagKey.Toc)
		{
		}
		else
		{
#if DEBUG
			if (tagInfo.Key == TagKey.Var)
			{
			}
#endif
			// タグは出力しないので削除する（Replace だと複数回削除してしまうので Remove）
			line.Value = line.Value.Remove(column + match.Index, match.Length);

			// タグの分、追加位置を差し引く
			addColumn -= match.Length;
		}

		return (tagInfo, addColumn);
	}

	/// <summary>
	/// Cfm タグを解析して内容を更新する（全体用）
	/// </summary>
	/// <param name="lines">解析対象の行群</param>
	/// <param name="startLine">解析開始行</param>
	private void ParseCfmTagsForMain(LinkedList<String> lines, LinkedListNode<String> startLine)
	{
		LinkedListNode<String>? line = startLine;

		// 行をたどるループ
		for (; ; )
		{
			Int32 column = 0;
			LinkedListNode<String>? removeLine = null;

			// 列をたどるループ
			for (; ; )
			{
				(CfmTagInfo? tagInfo, Int32 addColumn) = ParseCfmTag(line, column, false);
				if (tagInfo != null)
				{
					// 有効なタグが見つかった（タグに対応する文字列は削除されている）
					switch (tagInfo.Key)
					{
						case TagKey.OutFile:
							ExecuteCfmTagOutFile(tagInfo);
							break;
						case TagKey.IncludeFolder:
							ExecuteCfmTagIncludeFolder(tagInfo);
							break;
						case TagKey.IncludeDefaultExt:
							ExecuteCfmTagIncludeDefaultExt(tagInfo);
							break;
						case TagKey.Include:
							Int32 prevLines = lines.Count;
							ExecuteCfmTagInclude(tagInfo, lines, line);
							ReserveRemoveIfNeeded(line, ref removeLine);
							Int32 deltaLines = lines.Count - prevLines;
							for (Int32 i = 0; i < deltaLines; i++)
							{
								line = line.Next;
								Debug.Assert(line != null, "ParseCfmTagsForMain() インクルードしているのに line が足りない");
							}
							break;
						case TagKey.Set:
							ExecuteCfmTagSet(tagInfo);
							break;
						case TagKey.Var:
							ExecuteCfmTagVar(tagInfo, line, column + addColumn);
							break;
						case TagKey.Toc:
							ExecuteCfmTagToc(tagInfo);
							break;
						case TagKey.GenerateAnchorFiles:
							ExecuteCfmTagGenerateAnchorFiles(tagInfo);
							break;
						case TagKey.AnchorPath:
							ExecuteCfmTagAnchorPath(line, column + addColumn);
							break;
						default:
							Debug.Assert(false, "ParseCfmTagsForMain() Cfm タグ捕捉漏れ");
							break;
					}
					ReserveRemoveIfNeeded(line, ref removeLine);
				}

				// 解析位置（列）を進める
				column += addColumn;
				if (column >= line.Value.Length)
				{
					break;
				}
			}

			// 次の行へ
			line = line.Next;

			// 予約されている場合は行削除
			// line.Next より後で実行する
			if (removeLine != null)
			{
				lines.Remove(removeLine);
			}

			MergeInfo.NumProgressLines++;
			if (MergeInfo.NumProgressLines % Cfm2Constants.PROGRESS_INTERVAL == 0)
			{
				SetProgressValue(MergeStep.ParseFile, (Double)MergeInfo.NumProgressLines / MergeInfo.NumTotalLines);
#if DEBUGz
                Thread.Sleep(100);
#endif
			}
			if (line == null)
			{
				break;
			}
			Cfm2Model.Instance.EnvModel.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}
	}

	/// <summary>
	/// Cfm タグを解析して内容を更新する（目次作成用）
	/// </summary>
	private void ParseCfmTagsForToc()
	{
		LinkedListNode<String>? line = MergeInfo.Lines.First;
		Debug.Assert(line != null, "ParseHxTags() _mergeInfo.Lines が空");

		// 行をたどるループ
		for (; ; )
		{
			Int32 column = 0;

			// 列をたどるループ
			for (; ; )
			{
				(CfmTagInfo? tagInfo, Int32 addColumn) = ParseCfmTag(line, column, true);
				if (tagInfo != null)
				{
					// 有効なタグが見つかった（タグに対応する文字列は削除されている）
					if (tagInfo.Key == TagKey.Toc)
					{
						StringBuilder stringBuilder = new();
						stringBuilder.Append("<div class=\"" + Cfm2Constants.TOC_AREA_CLASS_NAME + "\">\n");
						List<HxTagInfo> hxTagInfos = ParseHxTags(MergeInfo.TocTargets);
						for (Int32 i = 0; i < hxTagInfos.Count; i++)
						{
							stringBuilder.Append("  <div class=\"" + Cfm2Constants.TOC_ITEM_CLASS_NAME_PREFIX + hxTagInfos[i].Rank + "\"><a href=\"#" +
									hxTagInfos[i].Id + "\">" + hxTagInfos[i].Caption + "</a></div>\n");
						}
						stringBuilder.Append("</div>");
						line.Value = line.Value.Insert(column + addColumn, stringBuilder.ToString());
					}
				}

				// 解析位置（列）を進める
				column += addColumn;
				if (column >= line.Value.Length)
				{
					break;
				}
			}

			// 次の行へ
			line = line.Next;
			if (line == null)
			{
				break;
			}
		}
	}

	/// <summary>
	/// ファイルの内容を読み込み解析する
	/// 解析後の内容を parentLine の直後に追加（parentLine が null の場合は先頭に追加）
	/// </summary>
	/// <param name="path">読み込むファイルのフルパス</param>
	/// <param name="parentLines">親の行群</param>
	/// <param name="parentLine">読み込んだ内容を挿入する位置</param>
	/// <exception cref="Exception"></exception>
	private (Encoding encoding, String newLine) ParseFile(String path, LinkedList<String> parentLines, LinkedListNode<String>? parentLine)
	{
		if (String.IsNullOrEmpty(path))
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_NotSpecified".ToLocalized());
		}
		if (!File.Exists(path))
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_NoFile".ToLocalized() + "\n" + path);
		}
		if (MergeInfo.IncludeStack.Any(x => String.Compare(x, path, true) == 0))
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_CyclicInclude".ToLocalized() + "\n" + path);
		}

		// インクルード履歴プッシュ
		MergeInfo.IncludeStack.Add(path);

		// 文字コード自動判別
		FileInfo fileInfo = new(path);
		using FileReader reader = new(fileInfo);
		Encoding encoding = reader.Read(fileInfo).GetEncoding() ?? throw new Exception("MainPageViewModel_ParseFile_Error_Encoding".ToLocalized() + "\n" + path);

		// 改行コード自動判定
		String newLine;
		if (reader.Text.Contains("\r\n"))
		{
			newLine = "\r\n";
		}
		else if (reader.Text.Contains('\r'))
		{
			newLine = "\r";
		}
		else if (reader.Text.Contains('\n'))
		{
			newLine = "\n";
		}
		else
		{
			newLine = "\r\n";
		}

		// このファイルの階層以下の内容
		String[] childLineStrings = reader.Text.Split(new String[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		if (childLineStrings.Length == 0)
		{
			throw new Exception("MainPageViewModel_ParseFile_Error_Empty".ToLocalized() + "\n" + path);
		}
		MergeInfo.NumTotalLines += childLineStrings.Length;
		LinkedList<String> childLines = new(childLineStrings);

		// このファイルの階層以下を解析
		Debug.Assert(childLines.First != null, "ParseFile() 内容が空");
		ParseCfmTagsForMain(childLines, childLines.First);

		// 先頭行の追加
		LinkedListNode<String>? childLine = childLines.First;
		if (parentLine == null)
		{
			// 先頭に追加
			parentLine = parentLines.AddFirst(childLine.Value);
		}
		else
		{
			// parentLine に追加
			parentLine = parentLines.AddAfter(parentLine, childLine.Value);
		}
		childLine = childLine.Next;

		// 残りの行の追加
		while (childLine != null)
		{
			parentLine = parentLines.AddAfter(parentLine, childLine.Value);
			childLine = childLine.Next;
		}

		// インクルード履歴ポップ
		Debug.Assert(MergeInfo.IncludeStack.Last() == path, "ParseFile() インクルード履歴破損");
		MergeInfo.IncludeStack.RemoveAt(MergeInfo.IncludeStack.Count - 1);
		Log.Debug("ParseFile() end: " + MergeInfo.NumProgressLines + " / " + MergeInfo.NumTotalLines);

		return (encoding, newLine);
	}

	/// <summary>
	/// HTML Hx タグがあれば抽出する
	/// </summary>
	/// <param name="line">解析対象行</param>
	/// <param name="column">解析開始桁</param>
	/// <returns></returns>
	private (Int32 addColumn, HxTagInfo? hxTagInfo) ParseHxTag(LinkedListNode<String> line, Int32 column, Boolean[] targetRanks)
	{
		if (column >= line.Value.Length)
		{
			// 行末まで解析した
			return (column, null);
		}

		Match hxMatch = Regex.Match(line.Value[column..], @"\<h([1-6])\s.+?\>", RegexOptions.IgnoreCase);
		if (!hxMatch.Success)
		{
			// Hx タグが無い
			return (line.Value.Length, null);
		}

		Debug.Assert(hxMatch.Groups.Count >= 2, "ParseHxTag() hxMatch.Groups が不足");
		Int32 addColumn = hxMatch.Index + hxMatch.Length;

		// ランク確認
		_ = Int32.TryParse(hxMatch.Groups[1].Value, out Int32 rank);
		if (rank < Cfm2Constants.HX_TAG_RANK_MIN || rank > Cfm2Constants.HX_TAG_RANK_MAX)
		{
			MergeInfo.Warnings.Add("MainPageViewModel_ParseHxTag_Warning_HxRank".ToLocalized() + hxMatch.Value);
			return (addColumn, null);
		}
		if (!targetRanks[rank])
		{
			// 環境設定により対象外
			return (addColumn, null);
		}

		// ID 属性を抽出する
		Match idMatch = Regex.Match(hxMatch.Value, @"id\s*?=\s*?" + "\"" + "(.+?)" + "\"", RegexOptions.IgnoreCase);
		if (!idMatch.Success)
		{
			// ID 属性が無い
			MergeInfo.Warnings.Add("MainPageViewModel_ParseHxTag_Warning_HxId".ToLocalized() + hxMatch.Value);
			return (addColumn, null);
		}
		Debug.Assert(idMatch.Groups.Count >= 2, "ParseHxTag() idMatch.Groups が不足");
		String id = idMatch.Groups[1].Value;

		// 見出しを抽出する
		Int32 captionBeginPos = column + hxMatch.Index + hxMatch.Length;
		Int32 captionEndPos = line.Value.IndexOf("</h", captionBeginPos, StringComparison.OrdinalIgnoreCase);
		if (captionEndPos < 0)
		{
			MergeInfo.Warnings.Add("MainPageViewModel_ParseHxTag_Warning_HxNotClose".ToLocalized() + hxMatch.Value);
			return (addColumn, null);
		}
		String caption = line.Value[captionBeginPos..captionEndPos].Trim();

		// 目次情報追加
		HxTagInfo hxTagInfo = new()
		{
			Rank = rank,
			Id = id,
			Caption = caption,
		};

		return (addColumn, hxTagInfo);
	}

	/// <summary>
	/// HTML Hx タグを解析して目次情報を収集する
	/// </summary>
	private List<HxTagInfo> ParseHxTags(Boolean[] targetRanks)
	{
		LinkedListNode<String>? line = MergeInfo.Lines.First;
		Debug.Assert(line != null, "ParseHxTags() _mergeInfo.Lines が空");
		Int32 numProgressLines = 0;
		List<HxTagInfo> hxTagInfos = new();

		// 行をたどるループ
		for (; ; )
		{
			Int32 column = 0;

			// 列をたどるループ
			for (; ; )
			{
				(Int32 addColumn, HxTagInfo? hxTagInfo) = ParseHxTag(line, column, targetRanks);
				if (hxTagInfo != null)
				{
					hxTagInfos.Add(hxTagInfo);
				}

				// 解析位置（列）を進める
				column += addColumn;
				if (column >= line.Value.Length)
				{
					break;
				}
			}

			// 次の行へ
			line = line.Next;
			numProgressLines++;
			if (numProgressLines % Cfm2Constants.PROGRESS_INTERVAL == 0)
			{
				SetProgressValue(MergeStep.InsertToc, (Double)numProgressLines / MergeInfo.Lines.Count);
#if DEBUGz
                Thread.Sleep(20);
#endif
			}
			if (line == null)
			{
				break;
			}
		}
		return hxTagInfos;
	}

	/// <summary>
	/// 空行なら削除予約
	/// </summary>
	/// <param name="line"></param>
	/// <param name="removeLine"></param>
	private static void ReserveRemoveIfNeeded(LinkedListNode<String> line, ref LinkedListNode<String>? removeLine)
	{
		if (String.IsNullOrEmpty(line.Value))
		{
			removeLine = line;
		}
	}

	/// <summary>
	/// プログレスバーの進捗率を設定
	/// </summary>
	/// <param name="mergeStep">現在のステップ</param>
	/// <param name="currentProgress">現在のステップの中での進捗率</param>
	private void SetProgressValue(MergeStep mergeStep, Double currentProgress)
	{
		Double amount = 0;

		// 今までのステップの作業量
		for (Int32 i = 0; i < (Int32)mergeStep; i++)
		{
			amount += Cfm2Constants.MERGE_STEP_AMOUNT[i];
		}

		// 今のステップの作業量
		amount += Cfm2Constants.MERGE_STEP_AMOUNT[(Int32)mergeStep] * currentProgress;

		// 全作業量
		Int32 total = 0;
		for (Int32 i = 0; i < (Int32)MergeStep.__End__; i++)
		{
			total += Cfm2Constants.MERGE_STEP_AMOUNT[i];
		}

		// 進捗率
		Double progress = amount / total;

		_progressWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
		{
			// インクルードにより一時的に進捗率が下がる場合があるが、表示上は下げない
			if (progress > ProgressValue)
			{
				ProgressValue = progress;
			}
		});
	}

	/// <summary>
	/// ファイル出力
	/// </summary>
	private void Write(String path, IEnumerable<String> lines)
	{
		File.WriteAllText(path, String.Join(MergeInfo.NewLine, lines), MergeInfo.Encoding);
	}
}
