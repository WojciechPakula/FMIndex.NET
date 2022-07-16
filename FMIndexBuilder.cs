using System;
using System.Collections.Generic;
using System.Linq;

namespace FMIndexFast
{
    class FMIndexBuilder
    {
        string[] words; //array of all words
        int[] letterIdToWordId; //stores an information about an original word of a letter
        ushort[] letterIdToWordOffset;    //returns a position of a letter in the original word
        int[] letterIndexes;    //returns sorted indexes of letters
        char[] alphabet;    //sorted characters
        public FMIndexBody BuildStructure(string[] words)//259s
        {
            InitArrays(words);
            Array.Sort(letterIndexes, (a, b) => Compare(a, b)); //very slow
            alphabet = GetAlphabet();
            return InitFmIndex();
        }
        FMIndexBody InitFmIndex()
        {
            var alphabetIndexer = GetAlphabetIndexes();
            char[] bwt = new char[letterIndexes.Length];
            int[] wordIds = new int[letterIndexes.Length];
            ushort[] wordPos = new ushort[letterIndexes.Length];
            for (int i = 0; i < letterIndexes.Length; i++)
            {
                bwt[i] = LetterIdToLastChar(letterIndexes[i]);
                wordIds[i] = letterIdToWordId[letterIndexes[i]];
                wordPos[i] = letterIdToWordOffset[letterIndexes[i]];
            }
            return new FMIndexBody(bwt, wordIds, wordPos, alphabet, alphabetIndexer);
        }
        int[] GetAlphabetIndexes()
        {
            int[] charToLeftIndex = new int[alphabet.Length];
            for (int i = 0, j = 0; i < letterIndexes.Length; i++)
                if (LetterIdToFirstChar(letterIndexes[i]) != alphabet[j])
                    charToLeftIndex[++j] = i;
            return charToLeftIndex;
        }
        char LetterIdToFirstChar(int id) => GetChar(words[letterIdToWordId[id]], letterIdToWordOffset[id]);
        char LetterIdToLastChar(int id) => GetChar(words[letterIdToWordId[id]], letterIdToWordOffset[id] + words[letterIdToWordId[id]].Length);
        char[] GetAlphabet()
        {
            HashSet<char> chars = new HashSet<char>();
            var letters = words.SelectMany(x => x).Where(y => chars.Add(y)).ToList();
            if (!chars.Contains('$'))
                letters.Add('$');
            letters.Sort();
            return letters.ToArray();
        }
        void InitArrays(string[] words)
        {
            this.words = words;
            int letters = 0;
            int[] wordIdToLetterId = new int[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                wordIdToLetterId[i] = letters;
                letters += words[i].Length + 1;
            }
            letterIdToWordId = new int[letters];
            letterIndexes = new int[letters];
            letterIdToWordOffset = new ushort[letters];
            for (int i = 0, j = 0; i < letters; i++)
            {
                if (i >= wordIdToLetterId[j] + words[j].Length + 1)
                    j++;
                letterIndexes[i] = i;
                letterIdToWordId[i] = j;
                letterIdToWordOffset[i] = (ushort)(i - wordIdToLetterId[j]);
            }
        }
        int Compare(int letterIdA, int letterIdB)
        {
            var wordA = words[letterIdToWordId[letterIdA]];
            var wordB = words[letterIdToWordId[letterIdB]];
            var minLength = Math.Min(wordA.Length, wordB.Length) + 1;
            for (int i = 0; i < minLength; i++)
            {
                char A = GetChar(wordA, letterIdToWordOffset[letterIdA] + i);
                char B = GetChar(wordB, letterIdToWordOffset[letterIdB] + i);
                if (A != B)
                    return A.CompareTo(B);
            }
            return wordA.Length.CompareTo(wordB.Length);
        }
        char GetChar(string word, int index)
        {
            index %= word.Length + 1;
            if (index < word.Length)
                return word[index];
            else
                return '$';
        }
    }
}