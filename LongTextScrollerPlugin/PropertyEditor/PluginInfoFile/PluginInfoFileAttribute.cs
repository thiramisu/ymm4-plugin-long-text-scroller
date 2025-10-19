namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoFile;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginInfoFileAttribute(string fileName, string? label = null) : Attribute, IPluginInfoFile
{
    public string FileName { get; } = fileName;
    public string? Label { get; } = label;
}