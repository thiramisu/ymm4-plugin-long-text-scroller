using System.IO.Hashing;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace LongTextScrollerPlugin.Shape.LongTextScrollerPluginShape
{
    internal class LongTextScrollerPluginShapeSource : IShapeSource
    {
        readonly IGraphicsDevicesAndContext devices;
        readonly LongTextScrollerPluginShapeParameter lightweightTextScrollingShapeParameter;
        readonly DisposeCollector disposer = new();
        readonly IDWriteFactory factory;
        private readonly XxHash3 hasher = new();

        ID2D1CommandList? commandList;

        int[]? lineStartIndexes;
        IDWriteTextFormat? textFormat;
        ID2D1SolidColorBrush? brush;
        string text;
        float y;

        // キャッシュ比較用
        bool isFirst = false;
        double scroll;
        ScrollMeasurementUnitComboBoxEnum scrollMeasurementUnit;
        double lineHeight;
        int lineCount;
        bool shouldShiftHalfLine;
        ulong hashedText;
        string fontWin32FamilyName;
        string fontFamilyName;
        FontWeight fontWeightOfSelectedFont;
        FontWeight fontWeight;
        FontStyle fontStyleOfSelectedFont;
        FontStyle fontStyle;
        float fontSize;
        WordWrappingComboBoxEnum wordWrapping;
        double wordWrappingWidth;
        AlignmentComboBoxEnum alignment;
        System.Windows.Media.Color fontColor;
        double baseLinePerSize;
        //bool isDebug;

        /// <summary>
        /// 両端揃えのために追加する空白文字。
        /// </summary>
        private const string PaddingSpace = "    \u2063";

        /// <summary>
        /// 描画結果
        /// </summary>
        public ID2D1Image Output => commandList ?? throw new Exception($"{nameof(commandList)}がnullです。事前にUpdateを呼び出す必要があります。");

        public LongTextScrollerPluginShapeSource(IGraphicsDevicesAndContext devices, LongTextScrollerPluginShapeParameter lightweightTextScrollingShapeParameter)
        {
            this.devices = devices;
            this.lightweightTextScrollingShapeParameter = lightweightTextScrollingShapeParameter;

            factory = DWrite.DWriteCreateFactory<IDWriteFactory>();
            disposer.Collect(factory);

            fontWin32FamilyName = "";
            fontFamilyName = "";
            text = "";
        }

        /// <summary>
        /// 図形を更新する
        /// </summary>
        /// <param name="timelineItemSourceDescription"></param>
        public void Update(TimelineItemSourceDescription timelineItemSourceDescription)
        {
            //パラメーターが変わっていない場合は何もしない
            if (!UpdateCacheIfNeeded(timelineItemSourceDescription) && !isFirst)
                return;

            isFirst = false;
            UpdateCommandList();
        }

        public void Dispose()
        {
            disposer.DisposeAndClear();
        }

        bool UpdateCacheIfNeeded(TimelineItemSourceDescription timelineItemSourceDescription)
        {
            var frame = timelineItemSourceDescription.ItemPosition.Frame;
            var length = timelineItemSourceDescription.ItemDuration.Frame;
            var fps = timelineItemSourceDescription.FPS;

            var shouldUpdateTextFormat = false;
            //shouldUpdateTextFormat |= SetProperty(ref isDebug, lightweightTextScrollingShapeParameter.IsDebug);
            shouldUpdateTextFormat |= SetProperty(ref fontSize, lightweightTextScrollingShapeParameter.FontSize);
            if (SetProperty(ref fontWin32FamilyName, lightweightTextScrollingShapeParameter.FontWin32FamilyName))
            {
                shouldUpdateTextFormat = true;
                var font = FontGetter.GetFontByWin32FamilyName(factory, fontWin32FamilyName);
                fontFamilyName = font.FontFamily.FamilyNames.GetString(0);
                fontWeightOfSelectedFont = font.Weight;
                fontStyleOfSelectedFont = font.Style;

                var metrics = font.Metrics;
                baseLinePerSize = metrics.Ascent / metrics.DesignUnitsPerEm;
            }
            shouldUpdateTextFormat |= SetProperty(ref fontWeight, lightweightTextScrollingShapeParameter.IsBold ? FontWeight.Bold : fontWeightOfSelectedFont);
            shouldUpdateTextFormat |= SetProperty(ref fontStyle, lightweightTextScrollingShapeParameter.IsItalic ? FontStyle.Italic : fontStyleOfSelectedFont);

            if (shouldUpdateTextFormat || textFormat == null)
            {
                UpdateTextFormat();
                if (textFormat == null)
                    throw new NullReferenceException($"{nameof(textFormat)}がnullです。");

                SetProperty(ref wordWrapping, lightweightTextScrollingShapeParameter.WordWrapping);
                textFormat.WordWrapping = (WordWrapping)wordWrapping;

                SetProperty(ref alignment, lightweightTextScrollingShapeParameter.Alignment);
                textFormat.TextAlignment = AlignmentComboBoxUtil.GetTextAlignment(alignment);
            }
            else
            {
                if (SetProperty(ref wordWrapping, lightweightTextScrollingShapeParameter.WordWrapping))
                {
                    shouldUpdateTextFormat = true;
                    textFormat.WordWrapping = (WordWrapping)wordWrapping;
                }
                if (SetProperty(ref alignment, lightweightTextScrollingShapeParameter.Alignment))
                {
                    shouldUpdateTextFormat = true;
                    textFormat.TextAlignment = AlignmentComboBoxUtil.GetTextAlignment(alignment);
                }
            }

            var shouldUpdateLineIndexes = false;
            shouldUpdateLineIndexes |= shouldUpdateTextFormat;
            shouldUpdateLineIndexes |= SetProperty(ref hashedText, GetXxHash3FromText(lightweightTextScrollingShapeParameter.Text.AsSpan()));
            shouldUpdateLineIndexes |= SetProperty(ref fontSize, lightweightTextScrollingShapeParameter.FontSize);
            shouldUpdateLineIndexes |= SetProperty(ref wordWrappingWidth, lightweightTextScrollingShapeParameter.WordWrappingWidth.GetValue(frame, length, fps));
            if (shouldUpdateLineIndexes || lineStartIndexes == null)
            {
                UpdateLineStartIndexes();
            }

            var hasChanged = false;
            hasChanged |= shouldUpdateLineIndexes;
            hasChanged |= SetProperty(ref scroll, lightweightTextScrollingShapeParameter.Scroll.GetValue(frame, length, fps));
            hasChanged |= SetProperty(ref scrollMeasurementUnit, lightweightTextScrollingShapeParameter.ScrollMeasurementUnit);
            hasChanged |= SetProperty(ref lineHeight, lightweightTextScrollingShapeParameter.LineHeight.GetValue(frame, length, fps));
            hasChanged |= SetProperty(ref shouldShiftHalfLine, lightweightTextScrollingShapeParameter.ShouldShiftHalfLine);
            hasChanged |= SetProperty(ref lineCount, lightweightTextScrollingShapeParameter.LineCount);
            if (hasChanged)
            {
                UpdateTextAndY();
            }

            if (SetProperty(ref fontColor, lightweightTextScrollingShapeParameter.FontColor) || brush == null)
            {
                hasChanged = true;
                UpdateBrush();
            }

            return hasChanged;
        }

        void UpdateTextFormat()
        {
            if (textFormat != null)
            {
                disposer.RemoveAndDispose(ref textFormat);
            }
            textFormat = factory.CreateTextFormat(
                fontFamilyName: fontFamilyName,
                fontWeight: fontWeight,
                fontStyle: fontStyle,
                fontSize: fontSize
            );
            disposer.Collect(textFormat);
        }

        void UpdateLineStartIndexes()
        {
            using var textLayout = factory.CreateTextLayout(
                text: lightweightTextScrollingShapeParameter.Text,
                textFormat: textFormat,
                maxWidth: wordWrapping == WordWrappingComboBoxEnum.NoWrap ? 0f : (float)wordWrappingWidth,
                maxHeight: float.MaxValue
            );
            // 1行の文字数が分かれば良いのでLineSpacingは不要

            int sum = 0;
            lineStartIndexes = [0, .. textLayout.LineMetrics.Select(line => sum += line.Length)];
        }

        void UpdateTextAndY()
        {
            if (lineStartIndexes == null)
                throw new NullReferenceException($"{nameof(lineStartIndexes)}がnullです。");

            var absLineHeight = Math.Abs(lineHeight);
            // lineHeight == 0dの場合を考慮して、pxではなく行単位で計算
            var scrollLine = scrollMeasurementUnit switch
            {
                ScrollMeasurementUnitComboBoxEnum.Px => scroll / absLineHeight,
                ScrollMeasurementUnitComboBoxEnum.Line => scroll,
                ScrollMeasurementUnitComboBoxEnum.FromLine => scroll - 1,
                _ => throw new NotImplementedException()
            };
            int startLine = Math.Max(1, (int)Math.Floor(scrollLine + (shouldShiftHalfLine ? 0.5d : 1d)));
            int endLine = Math.Min(lineStartIndexes.Length, startLine + lineCount);
            double paragraphAlignment = AlignmentComboBoxUtil.GetParagraphAlignment(alignment) switch
            {
                ParagraphAlignment.Near => 0d,
                ParagraphAlignment.Center => (1d - lineCount) / 2,
                ParagraphAlignment.Far => 1d - lineCount,
                _ => throw new NotImplementedException()
            };
            var isJustified = AlignmentComboBoxUtil.GetTextAlignment(alignment) == TextAlignment.Justified;
            if (startLine >= endLine)
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

        void UpdateBrush()
        {
            if (brush != null)
            {
                disposer.RemoveAndDispose(ref brush);
            }
            brush = devices.DeviceContext.CreateSolidColorBrush(
                new Color4(
                    (float)fontColor.R / 255,
                    (float)fontColor.G / 255,
                    (float)fontColor.B / 255,
                    (float)fontColor.A / 255
                )
            );
            disposer.Collect(brush);
        }

        void UpdateCommandList()
        {
            if (brush == null)
                throw new Exception($"{nameof(brush)}がnullです。");

            var dc = devices.DeviceContext;

            disposer.RemoveAndDispose(ref commandList);
            commandList = dc.CreateCommandList();
            disposer.Collect(commandList);

            dc.Target = commandList;
            dc.BeginDraw();
            dc.Clear(null);

            using var textLayout = factory.CreateTextLayout(
                text: text,
                textFormat: textFormat,
                maxWidth: wordWrapping == WordWrappingComboBoxEnum.NoWrap ? 0f : (float)wordWrappingWidth,
                maxHeight: float.MaxValue
            );
            var x = AlignmentComboBoxUtil.GetTextAlignment(alignment) switch
            {
                TextAlignment.Leading => 0f,
                TextAlignment.Center => -(float)wordWrappingWidth / 2,
                TextAlignment.Justified => -(float)wordWrappingWidth / 2,
                TextAlignment.Trailing => -(float)wordWrappingWidth,
                _ => throw new NotImplementedException()
            };
            var absLineHeight = Math.Abs(lineHeight);
            textLayout.SetLineSpacing(LineSpacingMethod.Uniform, (float)absLineHeight, (float)(baseLinePerSize * fontSize));
            // SetCharacterSpacing(); を使いたいがIDWriteTextLayout1に対応する方法が不明なので諦め
            dc.DrawTextLayout(new Vector2(x, y), textLayout, brush);

            dc.EndDraw();
            dc.Target = null;//Targetは必ずnullに戻す。
            commandList.Close();//CommandListはEndDraw()の後に必ずClose()を呼んで閉じる必要がある
        }
        private ulong GetXxHash3FromText(ReadOnlySpan<char> text)
        {
            hasher.Append(MemoryMarshal.AsBytes(text));
            Span<byte> hashBytes = stackalloc byte[8];
            hasher.GetHashAndReset(hashBytes);

            return BitConverter.ToUInt64(hashBytes);
        }

        /// <returns>値が変更された場合は true、それ以外は false。</returns>
        static bool SetProperty<T>(ref T field, T newValue)
        {
            if (field == null || !EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                return true;
            }
            return false;
        }
    }
}