using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Settings;

namespace LongTextScrollerPlugin.Shape.LongTextScrollerPluginShape
{
    internal class LongTextScrollerPluginShapeSource : IShapeSource
    {
        readonly IGraphicsDevicesAndContext devices;
        readonly LongTextScrollerPluginShapeParameter lightweightTextScrollingShapeParameter;
        readonly DisposeCollector disposer = new();
        readonly IDWriteFactory1 factory;

        ID2D1CommandList? commandList;

        int[]? lineStartIndexes;
        IDWriteTextFormat1? textFormat;
        ID2D1SolidColorBrush? brush;
        string text;
        float y;

        // キャッシュ比較用
        bool isFirst = false;
        // Animation
        double scroll;
        double lineHeight;
        double wordWrappingWidth;
        // Font Data
        Font? font;
        double baseLinePerSize;
        float TextAlignmentRatio => lightweightTextScrollingShapeParameter.TextAlignment switch
        {
            TextAlignment.Leading => 0f,
            TextAlignment.Center => 0.5f,
            TextAlignment.Justified => 0.5f,
            TextAlignment.Trailing => 1f,
            _ => throw new NotImplementedException($"{nameof(TextAlignment)} = {lightweightTextScrollingShapeParameter.TextAlignment}の場合の処理が未実装です。")
        };

        bool shouldUpdateFontData = true;
        bool shouldUpdateTextFormat = true;
        bool shouldUpdateWordWrapping = true;
        bool shouldUpdateAlignment = true;
        bool shouldUpdateLineIndexes = true;
        bool shouldUpdateTextAndY = true;
        bool shouldUpdateBrush = true;

        /// <summary>
        /// 両端揃えのために追加する空白文字。
        /// </summary>
        private const string PaddingSpace = "    \u2063";

        /// <summary>
        /// 描画結果
        /// </summary>
        public ID2D1Image Output => commandList ?? throw new InvalidOperationException($"{nameof(commandList)}がnullです。事前にUpdateを呼び出す必要があります。");

        public LongTextScrollerPluginShapeSource(IGraphicsDevicesAndContext devices, LongTextScrollerPluginShapeParameter lightweightTextScrollingShapeParameter)
        {
            this.devices = devices;
            this.lightweightTextScrollingShapeParameter = lightweightTextScrollingShapeParameter;
            lightweightTextScrollingShapeParameter.PropertyChanged += OnParameterChanged;

            factory = DWrite.DWriteCreateFactory<IDWriteFactory1>();
            disposer.Collect(factory);

            text = "";
        }

        /// <summary>
        /// 図形を更新する
        /// </summary>
        /// <param name="timelineItemSourceDescription"></param>
        public void Update(TimelineItemSourceDescription timelineItemSourceDescription)
        {
            var frame = timelineItemSourceDescription.ItemPosition.Frame;
            var length = timelineItemSourceDescription.ItemDuration.Frame;
            var fps = timelineItemSourceDescription.FPS;

            UpdateFontDataIfNeeded();
            UpdateTextFormatIfNeeded();

            shouldUpdateLineIndexes |= SetProperty(
                ref wordWrappingWidth,
                lightweightTextScrollingShapeParameter.WordWrapping == WordWrappingComboBoxEnum.NoWrap ? 0f : lightweightTextScrollingShapeParameter.WordWrappingWidth.GetValue(frame, length, fps)
            );
            UpdateLineStartIndexesIfNeeded();

            shouldUpdateTextAndY |= SetProperty(ref scroll, lightweightTextScrollingShapeParameter.Scroll.GetValue(frame, length, fps));
            shouldUpdateTextAndY |= SetProperty(ref lineHeight, lightweightTextScrollingShapeParameter.LineHeight.GetValue(frame, length, fps));
            var hasChanged = shouldUpdateTextAndY | shouldUpdateBrush;
            UpdateTextAndYIfNeeded();

            UpdateBrushIfNeeded();

            //パラメーターが変わっていない場合は何もしない
            if (!hasChanged && !isFirst)
                return;

            isFirst = false;

            UpdateCommandList();
        }

        public void Dispose()
        {
            lightweightTextScrollingShapeParameter.PropertyChanged -= OnParameterChanged;
            disposer.DisposeAndClear();
        }

        void OnParameterChanged(object? _, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(lightweightTextScrollingShapeParameter.FontWin32FamilyName):
                    //case nameof(lightweightTextScrollingShapeParameter.IsDebug):
                    shouldUpdateFontData = true;
                    break;
                case nameof(lightweightTextScrollingShapeParameter.FontSize):
                case nameof(lightweightTextScrollingShapeParameter.IsBold):
                case nameof(lightweightTextScrollingShapeParameter.IsItalic):
                    shouldUpdateTextFormat = true;
                    break;
                case nameof(lightweightTextScrollingShapeParameter.WordWrapping):
                    shouldUpdateWordWrapping = true;
                    break;
                case nameof(lightweightTextScrollingShapeParameter.Alignment):
                    shouldUpdateAlignment = true;
                    break;
                case nameof(lightweightTextScrollingShapeParameter.CharacterSpacing):
                case nameof(lightweightTextScrollingShapeParameter.Text):
                    shouldUpdateLineIndexes = true;
                    break;
                case nameof(lightweightTextScrollingShapeParameter.LineCount):
                case nameof(lightweightTextScrollingShapeParameter.ShouldShiftHalfLine):
                case nameof(lightweightTextScrollingShapeParameter.ScrollMeasurementUnit):
                    shouldUpdateTextAndY = true;
                    break;
                case nameof(lightweightTextScrollingShapeParameter.FontColor):
                    shouldUpdateBrush = true;
                    break;
            }
        }

        void UpdateFontDataIfNeeded()
        {
            if (!shouldUpdateFontData)
            {
                return;
            }
            shouldUpdateFontData = false;

            shouldUpdateTextFormat = true;

            font = FontGetter.GetFont(lightweightTextScrollingShapeParameter.FontWin32FamilyName);

            using var iDWriteFont = FontGetter.GetIDWriteFont(factory, font);
            var metrics = iDWriteFont.Metrics;
            baseLinePerSize = metrics.Ascent / metrics.DesignUnitsPerEm;
        }
        void UpdateTextFormatIfNeeded()
        {
            if (!shouldUpdateTextFormat && textFormat is not null)
            {
                if (shouldUpdateWordWrapping)
                {
                    shouldUpdateWordWrapping = false;

                    shouldUpdateLineIndexes = true;

                    textFormat.WordWrapping = (WordWrapping)lightweightTextScrollingShapeParameter.WordWrapping;
                }
                if (shouldUpdateAlignment)
                {
                    shouldUpdateAlignment = false;

                    shouldUpdateLineIndexes = true;

                    textFormat.TextAlignment = lightweightTextScrollingShapeParameter.TextAlignment;
                }
                return;
            }
            shouldUpdateTextFormat = false;
            shouldUpdateWordWrapping = false;
            shouldUpdateAlignment = false;

            shouldUpdateLineIndexes = true;

            if (font is null)
                throw new InvalidOperationException($"{nameof(font)} is null");
            if (textFormat is not null)
            {
                disposer.RemoveAndDispose(ref textFormat);
            }
            textFormat = factory.CreateTextFormat(
                fontFamilyName: font.CanonicalFontName,
                fontWeight: lightweightTextScrollingShapeParameter.IsBold ? Vortice.DirectWrite.FontWeight.Bold : (Vortice.DirectWrite.FontWeight)font.CanonicalFontWeight,
                fontStyle: lightweightTextScrollingShapeParameter.IsItalic ? Vortice.DirectWrite.FontStyle.Italic : (Vortice.DirectWrite.FontStyle)font.CanonicalFontStyle,
                fontStretch: (Vortice.DirectWrite.FontStretch)font.CanonicalFontStretch,
                fontSize: lightweightTextScrollingShapeParameter.FontSize
            ).QueryInterface<IDWriteTextFormat1>();
            disposer.Collect(textFormat);
            textFormat.WordWrapping = (WordWrapping)lightweightTextScrollingShapeParameter.WordWrapping;
            textFormat.TextAlignment = lightweightTextScrollingShapeParameter.TextAlignment;
        }

        void UpdateLineStartIndexesIfNeeded()
        {
            if (!shouldUpdateLineIndexes && lineStartIndexes is not null)
            {
                return;
            }
            shouldUpdateLineIndexes = false;

            shouldUpdateTextAndY = true;

            using var textLayout = CreateTextLayout(lightweightTextScrollingShapeParameter.Text);
            // 1行の文字数が分かれば良いのでLineSpacingは不要
            SetCharacterSpacing(textLayout, lightweightTextScrollingShapeParameter.Text.Length);

            int sum = 0;
            lineStartIndexes = [0, .. textLayout.LineMetrics.Select(line => sum += line.Length)];
        }

        void UpdateTextAndYIfNeeded()
        {
            if (!shouldUpdateTextAndY)
            {
                return;
            }
            shouldUpdateTextAndY = false;

            if (lineStartIndexes is null)
                throw new InvalidOperationException($"{nameof(lineStartIndexes)}がnullです。");

            var absLineHeight = Math.Abs(lineHeight);
            // lineHeight == 0dの場合を考慮して、pxではなく行単位で計算
            var scrollLine = lightweightTextScrollingShapeParameter.ScrollMeasurementUnit switch
            {
                ScrollMeasurementUnitComboBoxEnum.Px => scroll / absLineHeight,
                ScrollMeasurementUnitComboBoxEnum.Line => scroll,
                ScrollMeasurementUnitComboBoxEnum.FromLine => scroll - 1,
                _ => throw new NotImplementedException($"{nameof(ScrollMeasurementUnitComboBoxEnum)} = {lightweightTextScrollingShapeParameter.ScrollMeasurementUnit}の場合の処理が未実装です。")
            };
            var lineCount = lightweightTextScrollingShapeParameter.LineCount;
            int startLine = Math.Max(1, (int)Math.Floor(scrollLine + (lightweightTextScrollingShapeParameter.ShouldShiftHalfLine ? 0.5d : 1d)));
            int endLine = Math.Min(lineStartIndexes.Length, startLine + lineCount);
            double paragraphAlignment = lightweightTextScrollingShapeParameter.ParagraphAlignment switch
            {
                ParagraphAlignment.Near => 0d,
                ParagraphAlignment.Center => (1d - lineCount) / 2,
                ParagraphAlignment.Far => 1d - lineCount,
                _ => throw new NotImplementedException($"{nameof(ParagraphAlignment)} = {lightweightTextScrollingShapeParameter.ParagraphAlignment}の場合の処理が未実装です。")
            };
            var isJustified = lightweightTextScrollingShapeParameter.TextAlignment == TextAlignment.Justified;
            if (startLine >= endLine || lightweightTextScrollingShapeParameter.Text.Length == 0)
            {
                text = "";
                y = 0f;
            }
            else if (lineHeight >= 0d)
            {
                var isBeforeBreak = lightweightTextScrollingShapeParameter.Text[lineStartIndexes[endLine - 1] - 1] == '\n';
                text = lightweightTextScrollingShapeParameter.Text[lineStartIndexes[startLine - 1]..lineStartIndexes[endLine - 1]] +
                    (!isBeforeBreak && isJustified ? PaddingSpace : "");
                y = (float)((startLine - 1 - scrollLine + paragraphAlignment) * lineHeight);
            }
            // 負のlineHeightを渡すとエラーになるので、手動で実装
            else
            {
                var isBeforeBreak = lightweightTextScrollingShapeParameter.Text[lineStartIndexes[startLine] - 1] == '\n';
                // 行ごとの文字列を逆順で結合
                text = string.Join("",
                    Enumerable.Range(startLine, endLine - startLine)
                        .Select(lineNumber => lightweightTextScrollingShapeParameter.Text[lineStartIndexes[lineNumber - 1]..lineStartIndexes[lineNumber]])
                        .Reverse()
                ) +
                 (!isBeforeBreak && isJustified ? PaddingSpace : "");
                y = (float)((startLine - 1 - scrollLine - paragraphAlignment) * lineHeight);
            }
        }

        void UpdateBrushIfNeeded()
        {
            if (!shouldUpdateBrush && brush is not null)
            {
                return;
            }
            shouldUpdateBrush = false;

            if (brush is not null)
            {
                disposer.RemoveAndDispose(ref brush);
            }
            brush = devices.DeviceContext.CreateSolidColorBrush(
                new Color4(
                    (float)lightweightTextScrollingShapeParameter.FontColor.R / 255,
                    (float)lightweightTextScrollingShapeParameter.FontColor.G / 255,
                    (float)lightweightTextScrollingShapeParameter.FontColor.B / 255,
                    (float)lightweightTextScrollingShapeParameter.FontColor.A / 255
                )
            );
            disposer.Collect(brush);
        }

        void UpdateCommandList()
        {
            if (brush is null)
                throw new InvalidOperationException($"{nameof(brush)}がnullです。");

            var dc = devices.DeviceContext;

            disposer.RemoveAndDispose(ref commandList);
            commandList = dc.CreateCommandList();
            disposer.Collect(commandList);

            dc.Target = commandList;
            dc.BeginDraw();
            dc.Clear(null);

            using var textLayout = CreateTextLayout(text);
            var x = lightweightTextScrollingShapeParameter.TextAlignment switch
            {
                TextAlignment.Leading => 0f,
                TextAlignment.Center => -(float)wordWrappingWidth / 2,
                TextAlignment.Justified => -(float)wordWrappingWidth / 2 - lightweightTextScrollingShapeParameter.CharacterSpacing / 2,
                TextAlignment.Trailing => -(float)wordWrappingWidth,
                _ => throw new NotImplementedException($"{nameof(TextAlignment)} = {lightweightTextScrollingShapeParameter.TextAlignment}の場合の処理が未実装です。")
            };
            var absLineHeight = Math.Abs(lineHeight);
            textLayout.SetLineSpacing(LineSpacingMethod.Uniform, (float)absLineHeight, (float)(baseLinePerSize * lightweightTextScrollingShapeParameter.FontSize));
            SetCharacterSpacing(textLayout, text.Length);
            dc.DrawTextLayout(new Vector2(x, y), textLayout, brush);

            dc.EndDraw();
            dc.Target = null;//Targetは必ずnullに戻す。
            commandList.Close();//CommandListはEndDraw()の後に必ずClose()を呼んで閉じる必要がある
        }

        IDWriteTextLayout1 CreateTextLayout(string text)
        {
            float maxWidth
                = lightweightTextScrollingShapeParameter.WordWrapping == WordWrappingComboBoxEnum.NoWrap
                ? 0f
                : lightweightTextScrollingShapeParameter.TextAlignment == TextAlignment.Justified
                ? (float)wordWrappingWidth + lightweightTextScrollingShapeParameter.CharacterSpacing
                : (float)wordWrappingWidth;
            return factory.CreateTextLayout(text, textFormat, maxWidth, maxHeight: float.MaxValue).QueryInterface<IDWriteTextLayout1>();
        }

        void SetCharacterSpacing(IDWriteTextLayout1 textLayout, int textLength)
        {
            var characterSpacing = lightweightTextScrollingShapeParameter.CharacterSpacing;
            textLayout.SetCharacterSpacing(characterSpacing * TextAlignmentRatio, characterSpacing * (1f - TextAlignmentRatio), 0f, new TextRange(0, textLength));
        }

        /// <returns>値が変更された場合は true、それ以外は false。</returns>
        static bool SetProperty<T>(ref T field, T newValue)
        {
            if (field is null || !EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                return true;
            }
            return false;
        }
    }
}