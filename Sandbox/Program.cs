using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

using WanaKanaNet;

namespace Sandbox
{
    struct Jword
    {
        public string Kana { get; set; }
        public string Kanji { get; set; }
        public string Romaji { get; set; }
        public string Translation { get; set; }

        public static Jword FromLi(XElement li)
        {
            var spans = li.Elements("span");

            var kanji = spans.FirstOrDefault()?.Value?.Trim() ?? string.Empty;
            var kana = spans.LastOrDefault()?.Value?.Trim() ?? string.Empty;

            spans.ToList().ForEach(s => s.Remove());
            var translation = li.Value.Trim();
            translation = translation.Trim('、');
            translation = translation.Trim(',');
            translation = translation.Trim();
            translation = translation.Trim('–');
            translation = translation.Trim('-');
            translation = translation.Trim();

            //var indexOfDash = translation.IndexOf("–") + 1;
            //if (indexOfDash > 0)
            //{
            //    translation = translation.Substring(indexOfDash).Trim();
            //}

            //if (translation.StartsWith('-'))
            //{
            //    translation = translation.Substring(1);
            //}

            return new Jword
            {
                Kana = WanaKana.IsKana(kana) ? kana : string.Empty,
                Kanji = (WanaKana.IsJapanese(kanji) && kanji != kana) ? kanji : string.Empty,
                Romaji = WanaKana.ToRomaji(kana),
                Translation = translation
            };
        }
    }
    class Program
    {
        static void Parse(string listName)
        {
            File.ReadAllTextAsync($"{listName}.xml")
            .ContinueWith<XDocument>(s => XDocument.Parse(s.Result))
            .ContinueWith<IEnumerable<Jword>>(d => d.Result
                .Descendants("li")
                .Where(li =>
                !li.HasAttributes)
                .Select(li => Jword.FromLi(li))
                .Where(j => !string.IsNullOrWhiteSpace(j.Kana)))
            .ContinueWith<string>(l => JsonSerializer.Serialize(l.Result, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            }))
            .ContinueWith(s => File.WriteAllText($"{listName}.json", s.Result))
            .Wait();
        }

        static void Main(string[] args)
        {
            Parse("jlpt5");

            Console.WriteLine("Done!");
            //Console.ReadLine();
        }
    }
}
