using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FMIndexFast
{
    class FMIndexBody
    {
        char[] bwt; //big - Burrows–Wheeler transform
        int[] wordId; //big - Gets a word id from a bwt id
        ushort[] wordPosition; //big - Gets a position in a word from a bwt id
        char[] alphabet;    //small - all used characters / alphabet identifier
        int[] alphabetIndexer;  //small - Gets the first index in bwt, where column F begins with the given alphabet identifier
        Dictionary<char, int> alphabetToId; //Converts char into an alphabet identifier
        int segmentSize;
        int[,] rankCache;
        public int LettersSize => bwt.Length;
        public int SizeBytes => LettersSize * 8 + alphabet.Length * 2 + alphabetIndexer.Length * 4;
        public int CacheSizeBytes => rankCache.GetLength(0) * rankCache.GetLength(1) * 4;
        private FMIndexBody() { }
        public FMIndexBody(char[] bwt, int[] wordId, ushort[] wordPosition, char[] alphabet, int[] alphabetIndexer) => 
            (this.bwt, this.wordId, this.wordPosition, this.alphabet, this.alphabetIndexer) = (bwt, wordId, wordPosition, alphabet, alphabetIndexer);
        public void InitCache(int segmentSize = 128)
        {
            alphabetToId = new Dictionary<char, int>();
            for (int i = 0; i < alphabet.Length; i++)
                alphabetToId.Add(alphabet[i], i);
            this.segmentSize = segmentSize;
            rankCache = GetRankCache(segmentSize);
        }
        public IEnumerable<(int wordId, ushort wordPosition)> Search(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                yield break;
            for (int i = 0; i < pattern.Length; i++)
                if (!alphabetToId.ContainsKey(pattern[i]))
                    yield break;
            Stopwatch w = Stopwatch.StartNew();
            int alphabetId = alphabetToId[pattern[pattern.Length - 1]];
            int lowerIndex = alphabetIndexer[alphabetId];
            int higherIndex = alphabetId + 1 < alphabetIndexer.Length ? alphabetIndexer[alphabetId + 1] : bwt.Length;
            for (int i = pattern.Length - 2; i >= 0 && lowerIndex < higherIndex; i--)
            {
                var letterId = alphabetToId[pattern[i]];
                var lowerRank = GetRank(lowerIndex - 1, pattern[i]);
                var higherRank = GetRank(higherIndex - 1, pattern[i]);
                lowerIndex = alphabetIndexer[letterId] + lowerRank;
                higherIndex = alphabetIndexer[letterId] + higherRank;
            }
            w.Stop();
            Console.WriteLine($"found {higherIndex - lowerIndex} results in {w.Elapsed.TotalMilliseconds}ms {w.ElapsedTicks} ticks");
            for (int i = lowerIndex; i < higherIndex; i++)
                yield return (wordId[i], wordPosition[i]);
        }
        int GetRank(int bwtId, char letter)
        {
            int result = 0;
            if (bwtId < 0)
                return result;
            int letterId = alphabetToId[letter];
            int lowerIndex = bwtId / segmentSize;
            int higherIndex = lowerIndex + 1;
            if (higherIndex < rankCache.GetLength(0) && higherIndex * segmentSize - bwtId <= segmentSize / 2)
            {
                result = rankCache[higherIndex, letterId];
                for (int i = higherIndex * segmentSize; i != bwtId;)
                    if (letter == bwt[i--])
                        result--;
            } else
            {
                result = rankCache[lowerIndex, letterId];
                for (int i = lowerIndex * segmentSize; i != bwtId;)
                    if (letter == bwt[++i])
                        result++;
            }
            return result;
        }
        int[,] GetRankCache(int segmentSize)
        {
            int[] rightRanks = new int[alphabet.Length];
            Dictionary<char, int> charToId = new Dictionary<char, int>();
            int[,] rankCache = new int[(bwt.Length - 1) / segmentSize + 1, alphabet.Length];
            for (int i = 0; i < alphabet.Length; i++)
                charToId.Add(alphabet[i], i);
            for (int i = 0, k = 0, c = 0; i < bwt.Length; i++, k--)
            {
                rightRanks[charToId[bwt[i]]]++;
                if (k <= 0)
                {
                    for (int a = 0; a < alphabet.Length; a++)
                        rankCache[c, a] = rightRanks[a];
                    c++;
                    k += segmentSize;
                }
            }
            return rankCache;
        }
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream(8 * bwt.Length + 6 * alphabet.Length + 8))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(alphabet.Length);
                for (int i = 0; i < alphabet.Length; i++)
                    bw.Write(alphabet[i]);
                for (int i = 0; i < alphabet.Length; i++)
                    bw.Write(alphabetIndexer[i]);
                bw.Write(bwt.Length);
                for (int i = 0; i < bwt.Length; i++)
                    bw.Write(bwt[i]);
                for (int i = 0; i < bwt.Length; i++)
                    bw.Write(wordId[i]);
                for (int i = 0; i < bwt.Length; i++)
                    bw.Write(wordPosition[i]);
                return ms.ToArray();
            }
        }
        public static FMIndexBody Deserialize(Stream stream)
        {
            FMIndexBody fm = new FMIndexBody();
            using (BinaryReader br = new BinaryReader(stream))
            {
                var alphabetLength = br.ReadInt32();
                fm.alphabet = new char[alphabetLength];
                fm.alphabetIndexer = new int[alphabetLength];
                for (int i = 0; i < alphabetLength; i++)
                    fm.alphabet[i] = br.ReadChar();
                for (int i = 0; i < alphabetLength; i++)
                    fm.alphabetIndexer[i] = br.ReadInt32();
                var length = br.ReadInt32();
                fm.bwt = new char[length];
                fm.wordId = new int[length];
                fm.wordPosition = new ushort[length];
                for (int i = 0; i < length; i++)
                    fm.bwt[i] = (char)br.ReadChar();
                for (int i = 0; i < length; i++)
                    fm.wordId[i] = br.ReadInt32();
                for (int i = 0; i < length; i++)
                    fm.wordPosition[i] = br.ReadUInt16();
                return fm;
            }
        }
    }
}