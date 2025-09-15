using System.Globalization;
using Vortice.DirectWrite;

namespace LongTextScrollerPlugin.Shape.LongTextScrollerPluginShape
{
    internal static class FontGetter
    {
        /// <summary>
        /// フォントの完全名からフォントを取得します。見つからなかった場合、例外をスローします。
        /// </summary>
        public static IDWriteFont GetFontByWin32FamilyName(IDWriteFactory factory, string targetFontName)
        {
            var collection = factory.GetSystemFontCollection(false);
            string?[] locales = [
                CultureInfo.CurrentUICulture.Name,
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                "en-us",
                "en",
                null
            ];

            // 「メイリオ」「游ゴシック」などのループ
            for (int i = 0; i < collection.FontFamilyCount; i++)
            {
                var family = collection.GetFontFamily(i);
                // 「Medium」「イタリック」などのループ
                for (int j = 0; j < family.FontCount; j++)
                {
                    var font = family.GetFont(j);

                    font.GetInformationalStrings(InformationalStringId.Win32FamilyNames, out IDWriteLocalizedStrings? win32FamilyName, out _);
                    // 全フォントにあると思われるが、念のため
                    if (win32FamilyName == null)
                        continue;

                    var localizedName = GetLocalizedName(win32FamilyName);
                    if (localizedName == targetFontName)
                    {
                        // for debug
                        // すべてのInformationalStringIdをログ出力する。
                        /*/
                        foreach (InformationalStringId id in Enum.GetValues<InformationalStringId>().Distinct())
                        {
                            font.GetInformationalStrings(id, out IDWriteLocalizedStrings? informationalStrings, out _);

                            if (informationalStrings != null)
                            {
                                var localizedInformationalStrings = GetLocalizedName(informationalStrings);
                                Console.WriteLine($"{(int)id} ({id}): {localizedInformationalStrings}");
                            }
                        }
                        //*/
                        return font;
                    }
                }
            }

            throw new Exception($"Win32FamilyName {targetFontName} に対応するフォントが見つかりませんでした。");
        }


        public static string GetLocalizedName(IDWriteLocalizedStrings localizedStrings)
        {
            foreach (var locale in Locales.Distinct())
            {
                if (localizedStrings.FindLocaleName(locale, out int index))
                {
                    return localizedStrings.GetString(index);
                }
            }

            // 見つけられなかった場合、最初の名前を返す
            return localizedStrings.GetString(0);
        }

        private static string?[] Locales => [
            CultureInfo.CurrentUICulture.Name,
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "en-us",
            "en",
            null
        ];
    }
}