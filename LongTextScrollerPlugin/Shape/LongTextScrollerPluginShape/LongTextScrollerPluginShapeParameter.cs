using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Shape;
using YukkuriMovieMaker.Project;

namespace LongTextScrollerPlugin.Shape.LongTextScrollerPluginShape
{
    internal class LongTextScrollerPluginShapeParameter(SharedDataStore? sharedData) : ShapeParameterBase(sharedData)
    {
        [Display(Name = "スクロール", Description = "スクロールの量")]
        [AnimationSlider("F1", "", -500, 500)]
        public Animation Scroll { get; } = new Animation(0, -1_000_000, 1_000_000);

        [Display(Name = "スクロール単位", Description = "スクロール単位")]
        [EnumComboBox]
        public ScrollMeasurementUnitComboBoxEnum ScrollMeasurementUnit { get => scrollMeasurementUnit; set => Set(ref scrollMeasurementUnit, value); }
        ScrollMeasurementUnitComboBoxEnum scrollMeasurementUnit = ScrollMeasurementUnitComboBoxEnum.Px;

        [Display(Name = "行の高さ", Description = "行の高さ。\r\nアニメーションする際は、[スクロール単位]を[行]のどちらかに（[px]にすると行の高さが0pxの場合に何行目を描画すべきかが求められず描画できないため）、\r\n[折り返し]を[折り返さない]か[文字単位で折り返す]に（他だとアルゴリズムの関係で0を跨いだ際に改行位置が変わるため）することをオススメします。")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation LineHeight { get; } = new Animation(34, -100_000, 100_000);

        [Display(Name = "表示行数", Description = "表示行数")]
        [TextBoxSlider("F0", "", 1, 50)]
        [DefaultValue(3)]
        public int LineCount { get => lineCount; set => Set(ref lineCount, value); }
        int lineCount = 3;

        [Display(Name = "半行ずらす", Description = "出現・消滅位置を半行ぶん上にずらします")]
        [ToggleSlider]
        public bool ShouldShiftHalfLine { get => shouldShiftHalfLine; set => Set(ref shouldShiftHalfLine, value); }
        bool shouldShiftHalfLine = false;

        [Display(Name = "テキスト", Description = "文字の大きさ")]
        [TextEditor(AcceptsReturn = true)]
        public string Text { get => text; set => Set(ref text, value); }
        string text = string.Empty;

        [Display(Name = "フォント", Description = "フォント")]
        [FontComboBox]
        public string FontWin32FamilyName { get => fontWin32FamilyName; set => Set(ref fontWin32FamilyName, value); }
        string fontWin32FamilyName = "メイリオ";

        [Display(Name = "サイズ", Description = "文字の大きさ")]
        [TextBoxSlider("F1", "px", 1, 500)]
        [DefaultValue(34f)]
        public float FontSize { get => fontSize; set => Set(ref fontSize, value); }
        float fontSize = 34;

        [Display(Name = "折り返し", Description = "テキストの折り返し")]
        [EnumComboBox]
        public WordWrappingComboBoxEnum WordWrapping { get => wordWrapping; set => Set(ref wordWrapping, value); }
        WordWrappingComboBoxEnum wordWrapping = WordWrappingComboBoxEnum.NoWrap;

        [Display(Name = "折り返し幅", Description = "テキストを折り返す幅")]
        [AnimationSlider("F1", "px", 0, 2000)]
        public Animation WordWrappingWidth { get; } = new Animation(1920, 0, 100_000);

        [Display(Name = "文字揃え", Description = "文字揃え")]
        [EnumComboBox]
        public AlignmentComboBoxEnum Alignment { get => alignment; set => Set(ref alignment, value); }
        AlignmentComboBoxEnum alignment = AlignmentComboBoxEnum.LeadingNear;

        [Display(Name = "文字色", Description = "文字の色")]
        [ColorPicker]
        public Color FontColor { get => fontColor; set => Set(ref fontColor, value); }
        Color fontColor = Colors.White;

        [Display(Name = "太字", Description = "太字")]
        [ToggleSlider]
        public bool IsBold { get => isBold; set => Set(ref isBold, value); }
        bool isBold = false;

        [Display(Name = "イタリック", Description = "斜体・イタリック")]
        [ToggleSlider]
        public bool IsItalic { get => isItalic; set => Set(ref isItalic, value); }
        bool isItalic = false;

        // for debug
        //[Display(Name = "デバッグ用", Description = "デバッグ用")]
        //[ToggleSlider]
        //public bool IsDebug { get => isDebug; set => Set(ref isDebug, value); }
        //bool isDebug = false;

        //必ず引数なしのコンストラクタを定義してください。
        //これがないとプロジェクトファイルの読み込みに失敗します。
        public LongTextScrollerPluginShapeParameter() : this(null)
        {

        }

        /// <summary>
        /// マスクのExoFilterを生成する。
        /// </summary>
        /// <param name="keyFrameIndex">キーフレーム番号</param>
        /// <param name="desc">exo出力に必要な各種パラメーター</param>
        /// <param name="shapeMaskDesc">マスクのexo出力に必要な各種パラメーター</param>
        /// <returns>exoフィルタ</returns>
        public override IEnumerable<string> CreateMaskExoFilter(int keyFrameIndex, ExoOutputDescription desc, ShapeMaskExoOutputDescription shapeMaskDesc)
        {
            return [];
        }

        /// <summary>
        /// 図形アイテムのExoFilterを生成する。
        /// </summary>
        /// <param name="keyFrameIndex">キーフレーム番号</param>
        /// <param name="desc">exo出力に必要な各種パラメーター</param>
        /// <returns>exoフィルタ</returns>
        public override IEnumerable<string> CreateShapeItemExoFilter(int keyFrameIndex, ExoOutputDescription desc)
        {
            return [];
        }

        /// <summary>
        /// 図形ソースを生成する。
        /// </summary>
        /// <param name="devices">デバイス</param>
        /// <returns>図形ソース</returns>
        public override IShapeSource CreateShapeSource(IGraphicsDevicesAndContext devices)
        {
            return new LongTextScrollerPluginShapeSource(devices, this);
        }

        /// <summary>
        /// このクラス内のIAnimatable一覧を返す。
        /// </summary>
        /// <returns>IAnimatable一覧</returns>
        protected override IEnumerable<IAnimatable> GetAnimatables() => [Scroll, LineHeight, WordWrappingWidth];

        /// <summary>
        /// 設定を一時的に保存する。
        /// 図形の種類を切り替えたときに元の設定項目を復元するために必要。
        /// </summary>
        /// <param name="store"></param>
        protected override void LoadSharedData(SharedDataStore store)
        {
            var sharedData = store.Load<SharedData>();
            if (sharedData is null)
                return;

            sharedData.CopyTo(this);
        }

        /// <summary>
        /// 設定を復元する。
        /// 図形の種類を切り替えたときに元の設定項目を復元するために必要。
        /// </summary>
        /// <param name="store"></param>
        protected override void SaveSharedData(SharedDataStore store)
        {
            store.Save(new SharedData(this));
        }

        /// <summary>
        /// 設定の一時保存用クラス
        /// </summary>
        public class SharedData
        {
            public Animation Scroll { get; } = new Animation(0, -1_000_000, 1_000_000);
            public ScrollMeasurementUnitComboBoxEnum ScrollMeasurementUnit { get; }
            public Animation LineHeight { get; } = new Animation(34, -100, 100_000);
            public int LineCount { get; }
            public bool ShouldShiftHalfLine { get; }
            public string Text { get; }
            public string FontWin32FamilyName { get; }
            public float FontSize { get; }

            public WordWrappingComboBoxEnum WordWrapping { get; }
            public Animation WordWrappingWidth { get; } = new Animation(1920, 0, 100_000);
            public AlignmentComboBoxEnum Alignment { get; }
            public Color FontColor { get; }
            public bool IsBold { get; }
            public bool IsItalic { get; }

            public SharedData(LongTextScrollerPluginShapeParameter parameter)
            {
                Scroll.CopyFrom(parameter.Scroll);
                ScrollMeasurementUnit = parameter.ScrollMeasurementUnit;
                LineHeight.CopyFrom(parameter.LineHeight);
                LineCount = parameter.LineCount;
                ShouldShiftHalfLine = parameter.ShouldShiftHalfLine;
                Text = parameter.Text;
                FontWin32FamilyName = parameter.FontWin32FamilyName;
                FontSize = parameter.FontSize;
                WordWrapping = parameter.WordWrapping;
                WordWrappingWidth.CopyFrom(parameter.WordWrappingWidth);
                Alignment = parameter.Alignment;
                FontColor = parameter.FontColor;
                IsBold = parameter.IsBold;
                IsItalic = parameter.IsItalic;
            }

            public void CopyTo(LongTextScrollerPluginShapeParameter parameter)
            {
                parameter.Scroll.CopyFrom(Scroll);
                parameter.ScrollMeasurementUnit = ScrollMeasurementUnit;
                parameter.LineHeight.CopyFrom(LineHeight);
                parameter.LineCount = LineCount;
                parameter.ShouldShiftHalfLine = ShouldShiftHalfLine;
                parameter.Text = Text;
                parameter.FontWin32FamilyName = FontWin32FamilyName;
                parameter.FontSize = FontSize;
                parameter.WordWrapping = WordWrapping;
                parameter.WordWrappingWidth.CopyFrom(WordWrappingWidth);
                parameter.Alignment = Alignment;
                parameter.FontColor = FontColor;
                parameter.IsBold = IsBold;
                parameter.IsItalic = IsItalic;

            }
        }
    }
}