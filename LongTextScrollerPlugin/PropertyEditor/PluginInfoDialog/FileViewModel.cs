using System.IO;
using YukkuriMovieMaker.Commons;

namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoDialog;

public class FileViewModel : Bindable
{
    public string FileName { get => _fileName; set { if (Set(ref _fileName, value)) { OnPropertyChanged(nameof(Label)); } } }
    public string? FilePath { get; set; }
    public bool IsLoaded { get => _isLoaded; set => Set(ref _isLoaded, value); }

    string _fileContent;
    public string FileContent
    {
        get => _fileContent;
        set => Set(ref _fileContent, value);
    }

    readonly string? _label;
    public string Label => _label ?? FileName;

    bool _isLoaded;
    string _fileName;

    public static FileViewModel Create(string filePath, string? label = null) => new(Path.GetFileName(filePath), filePath, fileContent: "（未読み込み）", label);
    public static FileViewModel CreateErrorFile(string error) => new(fileName: "読み込みエラー", filePath: null, fileContent: error);

    FileViewModel(string fileName, string? filePath, string fileContent, string? label = null)
    {
        FilePath = filePath;
        _fileName = fileName;
        _fileContent = fileContent;
        _label = label;
        _isLoaded = filePath is null;
    }

    public void LoadContentIfNeeded()
    {
        if (IsLoaded || FilePath is null)
        {
            return;
        }

        try
        {
            FileContent = File.ReadAllText(FilePath);
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            FileContent = $"読み込みエラー: {ex.Message}";
            IsLoaded = true;
        }
    }

}