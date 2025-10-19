namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoRepo;

[AttributeUsage(AttributeTargets.Class)]
public class PluginInfoRepoAttribute(string owner, string name) : Attribute, IPluginInfoRepo
{
    public string Owner { get; } = owner;
    public string Name { get; } = name;
}