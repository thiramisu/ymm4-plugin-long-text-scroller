using System.Windows.Controls;
using YukkuriMovieMaker.Commons;

namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoButton;

/// <summary>
/// PluginInfoButton.xaml の相互作用ロジック
/// </summary>
public partial class PluginInfoButton : UserControl, IPropertyEditorControl
{
    public event EventHandler? BeginEdit;
    public event EventHandler? EndEdit;

    public PluginInfoButton()
    {
        InitializeComponent();
    }
}
