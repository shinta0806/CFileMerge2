// ============================================================================
// 
// メンテナンスページの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.IO.Compression;
using System.Windows.Input;

using CFileMerge2.Contracts.Services;
using CFileMerge2.Core.Helpers;
using CFileMerge2.Models.Cfm2Models;
using CFileMerge2.Models.SharedMisc;
using CFileMerge2.Services;
using CFileMerge2.Views;

using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Dispatching;

using Serilog;
using Serilog.Events;

using Shinta;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

using WinUIEx;

namespace CFileMerge2.ViewModels.Cfm2SettingsWindows;

public class Cfm2SettingsNavigationMaintenancePageViewModel : Cfm2SettingsNavigationPageViewModel
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    public Cfm2SettingsNavigationMaintenancePageViewModel(WindowEx3 window, Cfm2SettingsPageViewModel cfm2SettingsPageViewModel)
            : base(window, cfm2SettingsPageViewModel)
    {
        // コマンド
        ButtonCheckRssClickedCommand = new RelayCommand(ButtonCheckRssClicked);
        ButtonBackupClickedCommand = new RelayCommand(ButtonBackupClicked);
        ButtonRestoreClickedCommand = new RelayCommand(ButtonRestoreClicked);
    }

    // ====================================================================
    // public プロパティー
    // ====================================================================

    // --------------------------------------------------------------------
    // View 通信用のプロパティー
    // --------------------------------------------------------------------

    public String CheckBoxCheckRssContent
    {
        get => String.Format("Cfm2SettingsNavigationMaintenancePage_CheckBoxCheckRss_Content".ToLocalized(), Cfm2Constants.LK_GENERAL_APP_NAME.ToLocalized());
    }

    /// <summary>
    /// 最新情報を自動的に確認する
    /// </summary>
    private Boolean _checkRss;
    public Boolean CheckRss
    {
        get => _checkRss;
        set
        {
            _ = Task.Run(async () =>
            {
                if (_checkRss && !value)
                {
                    MessageDialog messageDialog = _window.CreateMessageDialog("最新情報の確認を無効にすると、" + Cfm2Constants.LK_GENERAL_APP_NAME.ToLocalized()
                            + "の新版がリリースされた際の更新内容などが表示されません。\n\n本当に無効にしてもよろしいですか？", Cfm2Constants.LABEL_WARNING);
                    messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_YES));
                    messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_NO));
                    IUICommand cmd = await messageDialog.ShowAsync();
                    if (cmd.Label != Cfm2Constants.LABEL_YES)
                    {
                        _window.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                        {
                            OnPropertyChanged(nameof(CheckRss));
                        });
                        return;
                    }
                }

                SetProperty(ref _checkRss, value);
            });
        }
    }

    /// <summary>
    /// 最新情報確認中
    /// </summary>
    private Boolean _isProgressRingActive;
    public Boolean IsProgressRingActive
    {
        get => _isProgressRingActive;
        set => SetProperty(ref _isProgressRingActive, value);
    }

    /// <summary>
    /// 今すぐ最新情報を確認ボタンの有効性
    /// </summary>
    private Boolean _isButtonCheckRssEnabled = true;
    public Boolean IsButtonCheckRssEnabled
    {
        get => _isButtonCheckRssEnabled;
        set => SetProperty(ref _isButtonCheckRssEnabled, value);
    }

    // --------------------------------------------------------------------
    // コマンド
    // --------------------------------------------------------------------

    #region 今すぐ最新情報を確認ボタンの制御
    public ICommand ButtonCheckRssClickedCommand
    {
        get;
    }

    private async void ButtonCheckRssClicked()
    {
        try
        {
            IsButtonCheckRssEnabled = false;
            IsProgressRingActive = true;
            await Cfm2Common.CheckLatestInfoAsync(true, _window);
        }
        catch (Exception ex)
        {
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "今すぐ最新情報を確認時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
        finally
        {
            IsProgressRingActive = false;
            IsButtonCheckRssEnabled = true;
        }
    }
    #endregion

    #region 設定のバックアップボタンの制御
    public ICommand ButtonBackupClickedCommand
    {
        get;
    }

    private async void ButtonBackupClicked()
    {
        Debug.WriteLine("ButtonBackupClicked()");
        try
        {
            _cfm2SettingsPageViewModel.CheckPropertiesAndPropertiesToSettings();

            FileSavePicker fileSavePicker = _window.CreateSaveFilePicker();
            fileSavePicker.FileTypeChoices.Add("設定ファイル", new List<String>() { Common.FILE_EXT_SETTINGS_ARCHIVE });
            fileSavePicker.SuggestedFileName = Cfm2Constants.APP_ID + "Settings_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");

            StorageFile? file = await fileSavePicker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            File.Delete(file.Path);
            Cfm2Common.LogEnvironmentInfo();
            await CreateBackupAsync(file.Path, Cfm2Common.TempPath() + "\\" + Cfm2Constants.APP_ID + "\\");
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Information, "設定のバックアップが完了しました。");
        }
        catch (Exception ex)
        {
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "設定のバックアップボタンクリック時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion

    #region 設定の復元ボタンの制御
    public ICommand ButtonRestoreClickedCommand
    {
        get;
    }

    private async void ButtonRestoreClicked()
    {
        try
        {
            FileOpenPicker fileOpenPicker = _window.CreateOpenFilePicker();
            fileOpenPicker.FileTypeFilter.Add(Common.FILE_EXT_SETTINGS_ARCHIVE);
            fileOpenPicker.FileTypeFilter.Add("*");

            StorageFile? file = await fileOpenPicker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            await LoadSettingsArchiveAsync(file.Path);
            _cfm2SettingsPageViewModel.SettingsToProperties();
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Information, "設定を復元しました。");
        }
        catch (OperationCanceledException)
        {
            Log.Information("設定の復元を中止しました。");
        }
        catch (Exception ex)
        {
            await _window.ShowLogMessageDialogAsync(LogEventLevel.Error, "設定の復元ボタンクリック時エラー：\n" + ex.Message);
            Log.Information("スタックトレース：\n" + ex.StackTrace);
        }
    }
    #endregion

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// プロパティーから設定に反映
    /// </summary>
    public override void PropertiesToSettings()
    {
        Cfm2Model.Instance.EnvModel.Cfm2Settings.CheckRss = CheckRss;
    }

    /// <summary>
    /// 設定をプロパティーに反映
    /// </summary>
    public override void SettingsToProperties()
    {
        CheckRss = Cfm2Model.Instance.EnvModel.Cfm2Settings.CheckRss;
    }

    // ====================================================================
    // private 定数
    // ====================================================================

    /// <summary>
    /// 設定ファイル名
    /// </summary>
    private const String FILE_NAME_SETTINGS = "settings" + Common.FILE_EXT_JSON;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 設定フォルダー内の指定拡張子のファイルをテンポラリフォルダーにコピー
    /// </summary>
    /// <param name="ext"></param>
    /// <param name="tempFolderPath"></param>
    private void CopyFiles(String ext, String tempFolderPath)
    {
        String[] files = Directory.GetFiles(((LocalSettingsService)App.GetService<ILocalSettingsService>()).Folder(), "*" + ext);
        foreach (String file in files)
        {
            File.Copy(file, tempFolderPath + Path.GetFileName(file));
        }
    }

    /// <summary>
    /// バックアップ作成
    /// </summary>
    private async Task CreateBackupAsync(String destPath, String tempFolderPath)
    {
        Directory.CreateDirectory(tempFolderPath);

        // 設定をファイル化
        Cfm2Model.Instance.EnvModel.Cfm2Settings.PrevLaunchVer = Cfm2Constants.APP_VER;
        String settings = await Json.StringifyAsync(Cfm2Model.Instance.EnvModel.Cfm2Settings);
        File.WriteAllText(tempFolderPath + FILE_NAME_SETTINGS, settings);

        // 最新情報確認履歴
        CopyFiles(Common.FILE_EXT_CONFIG, tempFolderPath);

        // ログ
        CopyFiles(Common.FILE_EXT_TXT, tempFolderPath);

        ZipFile.CreateFromDirectory(tempFolderPath, destPath, CompressionLevel.Optimal, true);
    }

    /// <summary>
    /// バックアップから設定を読み込む
    /// </summary>
    /// <param name=""></param>
    private async Task LoadSettingsArchiveAsync(String archivePath)
    {
        // 解凍
        String unzipFolder = Cfm2Common.TempPath() + "\\";
        Directory.CreateDirectory(unzipFolder);
        try
        {
            ZipFile.ExtractToDirectory(archivePath, unzipFolder);
        }
        catch (Exception ex)
        {
            throw new Exception("設定ファイルを読み込めません。", ex);
        }

        // 設定読み込み
        String extractPath = unzipFolder + Cfm2Constants.APP_ID + "\\" + FILE_NAME_SETTINGS;
        if (!File.Exists(extractPath))
        {
            throw new Exception("設定ファイル内の設定を読み込めません。");
        }
        String settings = File.ReadAllText(extractPath);
        Cfm2Settings cfm2Settings = await Json.ToObjectAsync<Cfm2Settings>(settings);

        // バージョンチェック
        if (cfm2Settings.PrevLaunchVer != Cfm2Constants.APP_VER)
        {
            MessageDialog messageDialog = _window.CreateMessageDialog("異なるバージョンの設定を復元しようとしています。\n正常に復元できない可能性がありますが、復元しますか？",
                    Cfm2Constants.LABEL_WARNING);
            messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_YES));
            messageDialog.Commands.Add(new UICommand(Cfm2Constants.LABEL_NO));
            IUICommand cmd = await messageDialog.ShowAsync();
            if (cmd.Label != Cfm2Constants.LABEL_YES)
            {
                throw new OperationCanceledException();
            }
        }

        // 復元
        Cfm2Model.Instance.EnvModel.Cfm2Settings = cfm2Settings;
    }
}
