using System.IO;
using YukkuriMovieMaker.Commons;

namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoDialog;

public class FileViewModel : Bindable
{
    public string FileName { get => fileName; set { if (Set(ref fileName, value)) { OnPropertyChanged(nameof(Label)); } } }
    public string? FilePath { get; set; }
    public bool IsLoaded { get => isLoaded; set => Set(ref isLoaded, value); }

    string fileContent;
    public string FileContent
    {
        get => fileContent;
        set => Set(ref fileContent, value);
    }

    readonly string? label;
    public string Label => label ?? FileName;

    bool isLoaded;
    string fileName;

    public static FileViewModel Create(string filePath, string? label = null) => new(Path.GetFileName(filePath), filePath, fileContent: "（未読み込み）", label);
    public static FileViewModel CreateErrorFile(string error) => new(fileName: "読み込みエラー", filePath: null, fileContent: error);

    FileViewModel(string fileName, string? filePath, string fileContent, string? label = null)
    {
        FilePath = filePath;
        this.fileName = fileName;
        this.fileContent = fileContent;
        this.label = label;
        isLoaded = filePath is null;
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