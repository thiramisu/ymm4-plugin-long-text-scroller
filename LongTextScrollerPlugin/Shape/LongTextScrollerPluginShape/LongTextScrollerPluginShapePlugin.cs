using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Shape;
using YukkuriMovieMaker.Project;

namespace LongTextScrollerPlugin.Shape.LongTextScrollerPluginShape;

public class LongTextScrollerPluginShapePlugin : IShapePlugin
{
    /// <summary>
    /// プラグインの名前
    /// 正式名称: 長文テキストの軽量スクロール
    /// </summary>
    public string Name => "長文軽量スクロール";

    /// <summary>
    /// 図形アイテムのexo出力に対応しているかどうか
    /// </summary>
    public bool IsExoShapeSupported => false;

    /// <summary>
    /// マスク系（図形切り抜きエフェクト、エフェクトアイテム）のexo出力に対応しているかどうか
    /// </summary>
    public bool IsExoMaskSupported => false;

    /// <summary>
    /// 図形パラメーターを作成する
    /// </summary>
    /// <param name="sharedData">共有データ。図形の種類を切り替えたときに元の設定項目を復元するために必要。</param>
    /// <returns>図形パラメータ</returns>
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LongTextScrollerPluginShapeParameter(sharedData);

    public PluginDetailsAttribute Details { get; } = new PluginDetailsAttribute
    {
        AuthorName = "oTATo",
        // 不使用
        //ContentId = "sm123456"
    };
}