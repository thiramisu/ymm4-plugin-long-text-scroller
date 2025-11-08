using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoFile;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoLink;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoRepo;
using LongTextScrollerPlugin.Utils;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin.Update;

namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoDialog;

internal class PluginInfoDialogViewModel : Bindable
{

    public string PluginName { get => pluginName; set { if (Set(ref pluginName, value)) { OnPropertyChanged(nameof(Title)); OnPropertyChanged(nameof(Header)); } } }
    string pluginName = "未設定のプラグイン名";

    public PluginType PluginType { get => pluginType; set { if (Set(ref pluginType, value)) { OnPropertyChanged(nameof(Title)); OnPropertyChanged(nameof(Header)); } } }
    PluginType pluginType = PluginType.Other;

    public string AuthorName { get => authorName; set { if (Set(ref authorName, value)) { OnPropertyChanged(nameof(Header)); } } }
    string authorName = string.Empty;

    public string PluginFullName => $"「{pluginName}」{pluginType.ToLocalizedDisplayString()}プラグイン";
    public string Title => $"{PluginFullName}について";
    public string Header => $"{PluginFullName} v{CurrentVersion.Version} {CurrentVersion.Suffix}{(AuthorName == string.Empty ? string.Empty : $" by {AuthorName}")}";

    public IPluginInfoLink[] Links { get => links; set => Set(ref links, value); }
    IPluginInfoLink[] links = [
        //new PluginInfoLinkAttribute("配布サイト", "https://example.com/" ),
        //new PluginInfoLinkAttribute("使い方", "https://example.com/howto" ),
        //new PluginInfoLinkAttribute("お問い合わせ", "https://example.com/support" ),
    ];

    public IPluginInfoFile[] Files { get => files; set { if (Set(ref files, value)) { UpdateFileCollection(); } } }
    IPluginInfoFile[] files = [
        //new PluginInfoFileAttribute("readme.txt"),
        //new PluginInfoFileAttribute("howToUse.txt", "使い方"),
    ];

    public IPluginInfoRepo? Repo { get => repo; set { if (Set(ref repo, value)) { FetchAndUpdateLatestVersionDataAsync(); } } }
    IPluginInfoRepo? repo;


    public PluginVersion CurrentVersion { get; } = PluginVersion.FromAssemblyInformationalVersion(typeof(PluginInfoDialogViewModel));

    public PluginVersion LatestVersion { get => latestVersion; private set { if (Set(ref latestVersion, value)) { OnPropertyChanged(nameof(HasNewVersion)); } } }
    PluginVersion latestVersion;

    public string VersionComparingStatus { get => versionComparingStatus; private set => Set(ref versionComparingStatus, value); }
    string versionComparingStatus = "オンラインで最新バージョンを確認しています…";

    // バージョン管理ミス時のために条件を緩めに設定しておく
    public bool HasNewVersion => CurrentVersion.Version != latestVersion.Version || CurrentVersion.Suffix != latestVersion.Suffix;

    public string DllDirectory { get => dllDirectory; private set => Set(ref dllDirectory, value); }
    string dllDirectory = "C:\\";

    public ObservableCollection<FileViewModel> FileViewModels { get; } = [];

    public FileViewModel? SelectedFile
    {
        get => selectedFile;
        set
        {
            if (Set(ref selectedFile, value))
            {
                selectedFile?.LoadContentIfNeeded();
            }
        }
    }
    FileViewModel? selectedFile;

    public PluginInfoDialogViewModel()
    {
        latestVersion = CurrentVersion;
        UpdateLocalData();
        FetchAndUpdateLatestVersionDataAsync();
    }

    void UpdateLocalData()
    {
        var dllPath = Assembly.GetExecutingAssembly().Location;
        var dllDirectory = Path.GetDirectoryName(dllPath);
        if (dllDirectory is null)
        {
            FileViewModels.Clear();
            FileViewModels.Add(FileViewModel.CreateErrorFile($"フォルダ「{dllPath}」が見つかりませんでした。"));
            SelectedFile = FileViewModels.FirstOrDefault();
            return;
        }
        DllDirectory = dllDirectory;
        UpdateFileCollection();
    }

    public void UpdateFileCollection()
    {
        FileViewModels.Clear();

        if (Files.Length == 0)
        {
            // 拡張子・大文字小文字に関係なくreadmeを検索
            var readmeFiles = Directory.EnumerateFiles(DllDirectory, "*", SearchOption.TopDirectoryOnly)
                .Where(f => Path.GetFileName(f).Contains("readme", StringComparison.OrdinalIgnoreCase));
            var txtFiles = Directory.EnumerateFiles(DllDirectory, "*.txt", SearchOption.TopDirectoryOnly);
            var allFiles = readmeFiles.Union(txtFiles).Distinct();
            foreach (var path in allFiles)
            {
                FileViewModels.Add(FileViewModel.Create(path));
            }

            if (FileViewModels.Count == 0)
            {
                FileViewModels.Add(FileViewModel.CreateErrorFile($"フォルダ「{DllDirectory}」内にREADMEまたは.txtファイルが見つかりませんでした。"));
            }
        }
        else
        {
            foreach (var file in Files)
            {
                FileViewModels.Add(FileViewModel.Create(Path.Combine(DllDirectory, file.FileName), file.Label));
            }
        }

        SelectedFile = FileViewModels.FirstOrDefault();
    }

    async void FetchAndUpdateLatestVersionDataAsync()
    {
        if (Repo is null)
        {
            VersionComparingStatus = $"（このプラグインにはバージョンを確認するための情報がありません。）";
            return;
        }
        var result = await Task.Run(() => GitHubUpdateChecker.CheckLatestVersionAsync(Repo.Owner, Repo.Name));
        if (result.LatestTagName is not null)
        {
            if (PluginVersion.TryParse(result.LatestTagName.TrimStart('v'), out var latestVersion))
            {
                this.latestVersion = latestVersion;
            }
            else
            {
                VersionComparingStatus = $"オンラインから取得したバージョンの変換に失敗しました。";
                return;
            }
        }
        VersionComparingStatus = result.ErrorMessage is not null ? $"オンラインでのバージョンの確認に失敗しました。エラー：{result.ErrorMessage}"
            : HasNewVersion ? $"新しいバージョン ({result.LatestTagName}) があります。[ツール]>[プラグインポータル]から更新できます。"
            : "最新のバージョンです。";
    }
}