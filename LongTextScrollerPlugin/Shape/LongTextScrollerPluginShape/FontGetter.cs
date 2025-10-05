using System.Globalization;
using Vortice.DirectWrite;
using YukkuriMovieMaker.Settings;

namespace LongTextScrollerPlugin.Shape.LongTextScrollerPluginShape
{
    internal static class FontGetter
    {
        public static Font GetFont(string targetFontName)
        {
            return FontSettings.Default.SystemFonts.FindByFontName(targetFontName)
                ?? FontSettings.Default.CustomFonts.FindByFontName(targetFontName)
                ?? new Font();
        }

        static Font? FindByFontName(this IEnumerable<Font> fonts, string targetFontName)
        {
            foreach (var font in fonts)
            {
                if (font.FontName == targetFontName)
                {
                    return font;
                }
            }
            return null;
        }

        public static IDWriteFont GetIDWriteFont(IDWriteFactory factory, Font font)
        {
            var collection = factory.GetSystemFontCollection(false);
            if (!collection.FindFamilyName(font.CanonicalFontName, out int index))
                throw new Exception($"Win32FamilyName {font.CanonicalFontName} に対応するフォントが見つかりませんでした。");
            return collection.GetFontFamily(index).GetFirstMatchingFont(
                 (Vortice.DirectWrite.FontWeight)font.CanonicalFontWeight,
                 (Vortice.DirectWrite.FontStretch)font.CanonicalFontStretch,
                 (Vortice.DirectWrite.FontStyle)font.CanonicalFontStyle);
        }
    }
}