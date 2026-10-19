using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace FrameShift
{
    /// <summary>
    /// Plugin strings in the language selected in Playnite. Dictionaries are embedded as
    /// FrameShift.Localization.&lt;code&gt;.xaml and kept private (not merged into the application
    /// resources), so short keys such as "Off" cannot clash with themes or other extensions.
    /// </summary>
    internal static class Loc
    {
        private const string Prefix = "FrameShift.Localization.";
        private const string DefaultLanguage = "en_US";
        private static readonly string[] RightToLeft = { "ar_", "fa_", "he_" };

        private static ResourceDictionary english;
        private static ResourceDictionary current;

        public static FlowDirection FlowDirection { get; private set; } = FlowDirection.LeftToRight;

        public static void Init(string language, Action<Exception> onError)
        {
            try
            {
                english = Load(DefaultLanguage);
                string code = Resolve(language);
                current = code == DefaultLanguage ? english : Load(code) ?? english;
                FlowDirection = RightToLeft.Any(p => code.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }
            catch (Exception ex) { onError?.Invoke(ex); }
        }

        public static string Get(string key)
        {
            return current?[key] as string ?? english?[key] as string ?? key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        // "pl_PL" -> pl_PL; unknown region ("es_MX") -> first dictionary for the language ("es_ES").
        private static string Resolve(string language)
        {
            if (string.IsNullOrWhiteSpace(language)) return DefaultLanguage;
            string code = language.Trim().Replace('-', '_');
            var names = typeof(Loc).Assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith(Prefix, StringComparison.Ordinal) && n.EndsWith(".xaml", StringComparison.Ordinal))
                .Select(n => n.Substring(Prefix.Length, n.Length - Prefix.Length - ".xaml".Length))
                .ToList();

            string exact = names.FirstOrDefault(n => string.Equals(n, code, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;
            string lang = code.Split('_')[0] + "_";
            return names.FirstOrDefault(n => n.StartsWith(lang, StringComparison.OrdinalIgnoreCase)) ?? DefaultLanguage;
        }

        private static ResourceDictionary Load(string code)
        {
            using (Stream stream = typeof(Loc).Assembly.GetManifestResourceStream(Prefix + code + ".xaml"))
                return stream == null ? null : XamlReader.Load(stream) as ResourceDictionary;
        }
    }
}
