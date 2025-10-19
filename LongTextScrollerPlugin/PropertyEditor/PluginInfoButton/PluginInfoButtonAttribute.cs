using LongTextScrollerPlugin.PropertyEditor.PluginInfoDetails;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoFile;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoLink;
using LongTextScrollerPlugin.PropertyEditor.PluginInfoRepo;
using System.Reflection;
using System.Windows;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Brush;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Plugin.FileSource;
using YukkuriMovieMaker.Plugin.FileWriter;
using YukkuriMovieMaker.Plugin.Shape;
using YukkuriMovieMaker.Plugin.Tachie;
using YukkuriMovieMaker.Plugin.TextCompletion;
using YukkuriMovieMaker.Plugin.Transcription;
using YukkuriMovieMaker.Plugin.Transition;
using YukkuriMovieMaker.Plugin.Update;
using YukkuriMovieMaker.Plugin.Voice;

namespace LongTextScrollerPlugin.PropertyEditor.PluginInfoButton;

internal class PluginInfoButtonAttribute : PropertyEditorAttribute2
{
    public override FrameworkElement Create() => new PluginInfoButton();

    public override void SetBindings(FrameworkElement control, ItemProperty[] itemProperties)
    {
        if (control is not PluginInfoButton editor)
        {
            return;
        }

        var owner = itemProperties[0].PropertyOwner;
        var ownerType = owner.GetType();
        var pluginName = ownerType.GetCustomAttribute<PluginInfoDetailsAttribute>()?.Name ?? owner switch
        {
            IPlugin plugin => plugin.Name,
            VideoEffectBase => ownerType.GetCustomAttribute<VideoEffectAttribute>()?.Name,
            AudioEffectBase => ownerType.GetCustomAttribute<AudioEffectAttribute>()?.Name,
            _ => null,
        } ?? throw new Exception($"プラグイン名の取得に失敗しました。");
        var pluginType = owner switch
        {
            IVideoEffect => PluginType.VideoEffect,
            IAudioEffect => PluginType.AudioEffect,
            IVoicePlugin => PluginType.Voice,
            IVoiceParameter => PluginType.Voice,
            IVideoFileWriterPlugin => PluginType.VideoWriter,
            IVideoFileSourcePlugin => PluginType.VideoSource,
            IAudioFileSourcePlugin => PluginType.AudioSource,
            IImageFileSourcePlugin => PluginType.ImageSource,
            ITransitionPlugin => PluginType.Transition,
            IShapePlugin => PluginType.Shape,
            IShapeParameter => PluginType.Shape,
            ITachiePlugin => PluginType.Tachie,
            ITachieCharacterParameter => PluginType.Tachie,
            IToolPlugin => PluginType.Tool,
            ITextCompletionPlugin => PluginType.TextCompletion,
            IBrushParameter => PluginType.Brush,
            ITranscriptionPlugin => PluginType.Transcription,

            IAudioSpectrumPlugin => PluginType.Other,
            IAudioSpectrumParameter => PluginType.Other,

            IPlugin => PluginType.Other,
            _ => throw new Exception($"プラグインタイプの取得に失敗しました。"),
        };
        var links = ownerType.GetCustomAttributes(typeof(PluginInfoLinkAttribute), false).OfType<PluginInfoLinkAttribute>().ToArray();
        var files = ownerType.GetCustomAttributes(typeof(PluginInfoFileAttribute), false).OfType<PluginInfoFileAttribute>().ToArray();
        var repo = ownerType.GetCustomAttribute<PluginInfoRepoAttribute>();
        var authorName = ownerType.GetCustomAttribute<PluginDetailsAttribute>()?.AuthorName;
        editor.DataContext = new PluginInfoButtonViewModel(pluginName, pluginType, authorName ?? string.Empty, links, files, repo);
    }
    public override void ClearBindings(FrameworkElement control)
    {
        if (control is not PluginInfoButton editor)
        {
            return;
        }

        var vm = editor.DataContext as PluginInfoButtonViewModel;
        vm?.Dispose();
        editor.DataContext = null;
    }
}
