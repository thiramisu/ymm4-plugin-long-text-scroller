using System.ComponentModel.DataAnnotations;
using Vortice.DirectWrite;

namespace LongTextScrollerPlugin.Shape.LongTextScrollerPluginShape
{
    internal enum ScrollMeasurementUnitComboBoxEnum
    {
        [Display(Name = "px", Description = "px")]
        Px,
        [Display(Name = "行ぶん", Description = "行ぶん")]
        Line,
        [Display(Name = "行目から表示", Description = "行目から表示")]
        FromLine,
    }

    //internal enum TextAlignmentComboBoxEnum
    //{
    //    [Display(Order = 1, Name = "左揃え", Description = "左揃え")]
    //    Leading = TextAlignment.Leading,
    //    [Display(Order = 2, Name = "中央揃え", Description = "中央揃え")]
    //    Center = TextAlignment.Center,
    //    [Display(Order = 3, Name = "右揃え", Description = "右揃え")]
    //    Trailing = TextAlignment.Trailing,
    //    [Display(Order = 4, Name = "両端揃え", Description = "自動改行された行の文字間隔を折り返し幅まで引き伸ばします")]
    //    Justified = TextAlignment.Justified,
    //}
    //internal enum ParagraphAlignmentComboBoxEnum
    //{
    //    [Display(Order = 1, Name = "上揃え", Description = "上揃え")]
    //    Near = ParagraphAlignment.Near,
    //    [Display(Order = 2, Name = "中揃え", Description = "中揃え")]
    //    Center = ParagraphAlignment.Center,
    //    [Display(Order = 3, Name = "下揃え", Description = "下揃え")]
    //    Far = ParagraphAlignment.Far,
    //}
    internal enum AlignmentComboBoxEnum
    {
        [Display(Order = 1, Name = "左揃え　[上]", Description = "左揃え　[上]")]
        LeadingNear = TextAlignment.Leading + (ParagraphAlignment.Near << 4),

        [Display(Order = 2, Name = "中央揃え[上]", Description = "中央揃え[上]")]
        CenterNear = TextAlignment.Center + (ParagraphAlignment.Near << 4),

        [Display(Order = 3, Name = "右揃え　[上]", Description = "右揃え　[上]")]
        TrailingNear = TextAlignment.Trailing + (ParagraphAlignment.Near << 4),

        [Display(Order = 4, Name = "両端揃え[上]", Description = "両端揃え[上] 自動改行された行の文字間隔を折り返し幅まで引き伸ばします")]
        JustifiedNear = TextAlignment.Justified + (ParagraphAlignment.Near << 4),

        [Display(Order = 5, Name = "左揃え　[中]", Description = "左揃え　[中]")]
        LeadingCenter = TextAlignment.Leading + (ParagraphAlignment.Center << 4),

        [Display(Order = 6, Name = "中央揃え[中]", Description = "中央揃え[中]")]
        CenterCenter = TextAlignment.Center + (ParagraphAlignment.Center << 4),

        [Display(Order = 7, Name = "右揃え　[中]", Description = "右揃え　[中]")]
        TrailingCenter = TextAlignment.Trailing + (ParagraphAlignment.Center << 4),

        [Display(Order = 8, Name = "両端揃え[中]", Description = "両端揃え[中] 自動改行された行の文字間隔を折り返し幅まで引き伸ばします")]
        JustifiedCenter = TextAlignment.Justified + (ParagraphAlignment.Center << 4),

        [Display(Order = 9, Name = "左揃え　[下]", Description = "左揃え　[下]")]
        LeadingFar = TextAlignment.Leading + (ParagraphAlignment.Far << 4),

        [Display(Order = 10, Name = "中央揃え[下]", Description = "中央揃え[下]")]
        CenterFar = TextAlignment.Center + (ParagraphAlignment.Far << 4),

        [Display(Order = 11, Name = "右揃え　[下]", Description = "右揃え　[下]")]
        TrailingFar = TextAlignment.Trailing + (ParagraphAlignment.Far << 4),

        [Display(Order = 12, Name = "両端揃え[下]", Description = "両端揃え[下] 自動改行された行の文字間隔を折り返し幅まで引き伸ばします")]
        JustifiedFar = TextAlignment.Justified + (ParagraphAlignment.Far << 4),
    }
    internal class AlignmentComboBoxUtil
    {
        public static TextAlignment GetTextAlignment(AlignmentComboBoxEnum alignmentComboBoxEnum)
        {
            return (TextAlignment)((int)alignmentComboBoxEnum & 0x0F);
        }
        public static ParagraphAlignment GetParagraphAlignment(AlignmentComboBoxEnum alignmentComboBoxEnum)
        {
            return (ParagraphAlignment)((int)alignmentComboBoxEnum >> 4);
        }
    }

    internal enum WordWrappingComboBoxEnum
    {
        [Display(Order = 1, Name = "折り返さない", Description = "折り返さない")]
        NoWrap = WordWrapping.NoWrap,
        [Display(Order = 2, Name = "単語単位で折り返す", Description = "単語単位で折り返す")]
        WholeWord = WordWrapping.WholeWord,
        [Display(Order = 3, Name = "文字単位で折り返す", Description = "文字単位で折り返す")]
        Character = WordWrapping.Character,
        [Display(Order = 4, Name = "超過時のみ文字単位", Description = "基本は単語単位ですが、1単語で折り返し幅を超過する場合は文字単位で折り返します")]
        EmergencyBreak = WordWrapping.EmergencyBreak,
    }
}
