using LongTextScrollerPlugin.PropertyEditor.PluginInfoFile;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoLink;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoRepo;
using System.Windows;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin.Update;

namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoButton;

internal class PluginInfoButtonViewModel : Bindable, IDisposable
{
    public static bool IsUpdateAvailable = true;

    public ActionCommand ShowPluginInfoDialogCommand { get; }
    public PluginType PluginType { get; }
    public string PluginName { get; }
    public string AuthorName { get; }
    public IPluginInfoLink[] Links { get; }
    public IPluginInfoFile[] Files { get; }
    public IPluginInfoRepo? Repo { get; }

    PluginInfoDialog.PluginInfoDialog? dialog;

    public PluginInfoButtonViewModel(string pluginName, PluginType pluginType, string authorName, IPluginInfoLink[] links, IPluginInfoFile[] files, IPluginInfoRepo? repo)
    {
        ShowPluginInfoDialogCommand = new ActionCommand(_ => true, ShowOrActivatePluginInfoDialog);
        PluginName = pluginName;
        PluginType = pluginType;
        AuthorName = authorName;
        Links = links;
        Files = files;
        Repo = repo;
    }

    public void Dispose() => dialog = null;

    void ShowOrActivatePluginInfoDialog(object? obj)
    {
        if (dialog is null || !dialog.IsLoaded)
        {
            dialog = new(PluginName, PluginType, AuthorName, Links, Files, Repo);
            dialog.Show();
        }
        else
        {
            if (dialog.WindowState == WindowState.Minimized)
            {
                dialog.WindowState = WindowState.Normal;
            }
        }
        // 同期実行だとボタンを離したタイミングで元のウィンドウにフォーカスを奪い返されるため遅延実行
        _ = dialog.Dispatcher.BeginInvoke(() => dialog.Activate());
    }
}
