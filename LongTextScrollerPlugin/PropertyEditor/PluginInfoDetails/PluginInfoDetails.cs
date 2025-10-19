namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoDetails;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginInfoDetailsAttribute(string name) : Attribute, IPluginInfoDetailsAttribute
{
    public string Name { get; } = name;
}