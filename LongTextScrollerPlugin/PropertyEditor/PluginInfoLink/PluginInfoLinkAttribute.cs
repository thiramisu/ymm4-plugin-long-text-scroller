namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoLink;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginInfoLinkAttribute(string label, string url) : Attribute, IPluginInfoLink
{
    public string Label { get; } = label;
    public string Url { get; } = url;
}