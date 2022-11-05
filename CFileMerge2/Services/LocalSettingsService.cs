// ============================================================================
// 
// 環境設定の保存
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 保存先ファイル
//   MSIX パッケージ時：%LocalAppData%\Packages\22724SHINTA.ChochoitoFileMerge2_7y6dzca6yqjvw\Settings\settings.dat
//   非 MSIX パッケージ時：%LocalAppData%\SHINTA\CFileMerge2\ApplicationData\LocalSettings.json
// ----------------------------------------------------------------------------

using CFileMerge2.Contracts.Services;
using CFileMerge2.Core.Contracts.Services;
using CFileMerge2.Core.Helpers;
using CFileMerge2.Helpers;
using CFileMerge2.Models.SharedMisc;

using Microsoft.Extensions.Options;

using Windows.Storage;

namespace CFileMerge2.Services;

public class LocalSettingsService : ILocalSettingsService
{
    // ====================================================================
    // コンストラクター
    // ====================================================================

    /// <summary>
    /// メインコンストラクター
    /// </summary>
    /// <param name="fileService"></param>
    /// <param name="options"></param>
    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService;
        _options = options.Value;

        _applicationDataFolder = Path.Combine(_localApplicationData, _options.ApplicationDataFolder ?? _defaultApplicationDataFolder);
        _localsettingsFile = _options.LocalSettingsFile ?? _defaultLocalSettingsFile;

        _settings = new Dictionary<string, object>();
    }

    // ====================================================================
    // public 関数
    // ====================================================================

    /// <summary>
    /// 保存先フォルダー（末尾 '\\'）
    /// </summary>
    /// <returns></returns>
    public String Folder()
    {
        if (RuntimeHelper.IsMSIX)
        {
            return Path.GetDirectoryName(ApplicationData.Current.LocalFolder.Path) + "\\" + FOLDER_NAME_SETTINGS;
        }
        else
        {
            return _applicationDataFolder + "\\";
        }
    }

    /// <summary>
    /// 環境設定読み込み
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }
        else
        {
            await InitializeAsync();

            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }

        return default;
    }

    /// <summary>
    /// 環境設定保存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SaveSettingAsync<T>(string key, T value)
    {
        if (RuntimeHelper.IsMSIX)
        {
            ApplicationData.Current.LocalSettings.Values[key] = await Json.StringifyAsync(value);
        }
        else
        {
            await InitializeAsync();

            _settings[key] = await Json.StringifyAsync(value);

            await Task.Run(() => _fileService.Save(_applicationDataFolder, _localsettingsFile, _settings));
        }
    }

    // ====================================================================
    // private 定数
    // ====================================================================

    /// <summary>
    /// MSIX パッケージ時の保存先
    /// </summary>
    private const String FOLDER_NAME_SETTINGS = "Settings\\";

    /// <summary>
    /// 非 MSIX パッケージ時のデフォルトの保存先（appsettings.json と合わせておく）
    /// </summary>
    private const string _defaultApplicationDataFolder = "SHINTA/CFileMerge2/ApplicationData";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";

    // ====================================================================
    // private 変数
    // ====================================================================

    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions _options;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;
    private readonly string _localsettingsFile;

    private IDictionary<string, object> _settings;

    private bool _isInitialized;

    // ====================================================================
    // private 関数
    // ====================================================================

    /// <summary>
    /// 非 MSIX パッケージ時の初期化
    /// </summary>
    /// <returns></returns>
    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _settings = await Task.Run(() => _fileService.Read<IDictionary<string, object>>(_applicationDataFolder, _localsettingsFile)) ?? new Dictionary<string, object>();

            _isInitialized = true;
        }
    }
}
