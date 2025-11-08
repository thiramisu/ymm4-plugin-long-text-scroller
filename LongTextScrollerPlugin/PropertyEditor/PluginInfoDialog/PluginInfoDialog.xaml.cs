using System.Diagnostics;
using System.Windows;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoFile;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoLink;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoRepo;
using YukkuriMovieMaker.Plugin.Update;

namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoDialog;

public partial class PluginInfoDialog : Window
{
    public PluginInfoDialog(string pluginName, PluginType pluginType, string authorName, IPluginInfoLink[] links, IPluginInfoFile[] files, IPluginInfoRepo? repo)
    {
        InitializeComponent();

        // 既にDataContextがXAMLで生成されているので、プロパティに値を渡す
        if (DataContext is PluginInfoDialogViewModel vm)
        {
            vm.PluginName = pluginName;
            vm.PluginType = pluginType;
            vm.AuthorName = authorName;
            vm.Links = links;
            vm.Files = files;
            vm.Repo = repo;
        }
    }

    void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        using (var _ = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
        {
            UseShellExecute = true
        }))
        { }

        e.Handled = true;
    }

    void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
