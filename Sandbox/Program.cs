using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

        static void Bundle(string[] files)
        {
            var allWords = files
                .SelectMany(f => 
                    File.ReadAllTextAsync($"{f}.xml")
                    .ContinueWith<XDocument>(s => XDocument.Parse(s.Result))
                    .ContinueWith<IEnumerable<Jword>>(d => d.Result
                        .Descendants("li")
                        .Where(li =>
                        !li.HasAttributes)
                        .Select(li => Jword.FromLi(li))
                        .Where(j => !string.IsNullOrWhiteSpace(j.Kana)))
                    .Result
                );

            var json = JsonSerializer.Serialize(allWords, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });

            File.WriteAllText($"{files.First()}.json", json);
        }

        static void Main(string[] args)
        {
            //Parse("jlpt2");
            string[] b = { 
                "jlpt1--",
                "jlpt1-a",
                "jlpt1-ka",
                "jlpt1-sa",
                "jlpt1-ta",
                "jlpt1-na",
                "jlpt1-ha",
                "jlpt1-ma",
                "jlpt1-ya",
                "jlpt1-ra",
                "jlpt1-wa"
            };

            Bundle(b);

            Console.WriteLine("Done!");
            //Console.ReadLine();
        }
    }
}
