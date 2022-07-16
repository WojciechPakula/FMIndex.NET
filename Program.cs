using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FMIndexFast
{
    class Program
    {
        const string fmIndexFileName = "wordsIndex.txt";
        const string wordsFileName = "words.txt";   //source https://sjp.pl/sl/odmiany/
        static void Main(string[] args)
        {
            Console.WriteLine("START");
            string[] words = GetWords();
            if (words == null || words.Length == 0)
                return;
            if (!File.Exists(fmIndexFileName))
                BuildFmIndexFile(words);    //slow
            FMIndexBody fmIndex = LoadFMIndexFile();

            GC.Collect();
            Console.WriteLine();
            Console.WriteLine($"Wyniki są wypisywane w postaci 0|słowo, gdzie 0 oznacza miejsce w słowie gdzie występuje początek wyszukiwanej frazy");
            Console.WriteLine("Napisz coś i wciśnij enter ...");
            while (true)
            {
                string pattern = Console.ReadLine();
                if (string.IsNullOrEmpty(pattern))
                    return;
                foreach (var (wordId, wordPos) in fmIndex.Search(pattern))
                {
                    Console.Write($"{wordPos}|{words[wordId]}, ");
                }
                Console.WriteLine("Done.\n");
            }
        }
        static void BuildFmIndexFile(string[] words)
        {
            Console.WriteLine($"No file {fmIndexFileName}");
            Console.WriteLine($"Building file (5 - 10 min)");
            FMIndexBuilder builder = new FMIndexBuilder();
            var fmIndex = builder.BuildStructure(words);    //slow
            Console.WriteLine($"Building done");
            var bin = fmIndex.Serialize();
            Console.WriteLine($"Writing file");
            File.WriteAllBytes(fmIndexFileName, bin);
        }
        static FMIndexBody LoadFMIndexFile()
        {
            using (FileStream stream = new FileStream(fmIndexFileName, FileMode.Open))
            {
                Console.WriteLine($"Loading: {fmIndexFileName}");
                var fmIndex = FMIndexBody.Deserialize(stream);
                fmIndex.InitCache(1024);
                Console.WriteLine($"Done, total number of letters: {fmIndex.LettersSize}, data structure size: {fmIndex.SizeBytes / 1024f / 1024f} MB, cache size: {fmIndex.CacheSizeBytes / 1024f / 1024f} MB");
                return fmIndex;
            }
        }
        static void TestAction(Action ac)
        {
            Stopwatch w = Stopwatch.StartNew();
            ac?.Invoke();
            w.Stop();
            Console.WriteLine($"{w.Elapsed.TotalMilliseconds}ms");
        }
        static string[] GetWords()
        {
            if (!File.Exists(wordsFileName))
            {
                Console.WriteLine($"There is no file with words: {wordsFileName}");
                return null;
            }
            Console.WriteLine($"Loading: {wordsFileName}");
            var lines = File.ReadAllLines(wordsFileName);
            var result = lines.SelectMany(x => x.Split(',')).Select(y => y.Trim().ToLower()).ToArray();
            return result;
        }
    }
}