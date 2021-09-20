/* Traducir Lab
 * Copyright (c) 2021,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight)
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Text;

namespace Traducir.Core.TextAnalysis
{
    public static class StringDiff
    {
        private static bool CollectionContainsSubsequence<T>(IList<T> coll, IList<T> subsequence)
        {
            if (coll.Count == 0 || subsequence.Count == 0)
            {
                return false;
            }

            if (coll.Count < subsequence.Count)
            {
                return false;
            }

            int coll_counter = 0;
            int subsequence_counter = 0;

            int count_equal = 0;

            while (true)
            {
                if (coll_counter >= coll.Count)
                {
                    break;
                }

                if (subsequence_counter >= subsequence.Count)
                {
                    break;
                }

                if (coll[coll_counter].Equals(subsequence[subsequence_counter]))
                {
                    coll_counter++;
                    subsequence_counter++;
                    count_equal++;

                    if (count_equal == subsequence.Count)
                    {
                        return true;
                    }

                    continue;
                }
                else
                {
                    coll_counter++;
                }
            }

            return count_equal == subsequence.Count;
        }

        private static IList<T> GetSubCollection<T>(IList<T> coll, int index, int length)
        {
            if (index < 0)
            {
                index = 0;
            }

            List<T> ret = new List<T>(coll.Count);

            for (int i = index; i < index + length; i++)
            {
                if (i >= coll.Count)
                {
                    break;
                }

                ret.Add(coll[i]);
            }

            return ret.ToArray();
        }

        private static IList<T> GetCollectionsLongestCommonSubsequence<T>(IList<T> left, IList<T> right)
        {
            if (left.Count == 0 || right.Count == 0)
            {
                return Array.Empty<T>();
            }

            if (CollectionContainsSubsequence(right, left))
            {
                return left;
            }

            if (CollectionContainsSubsequence(left, right))
            {
                return right;
            }

            if (left.Count == 1 || right.Count == 1)
            {
                return Array.Empty<T>();
            }

            IList<T> ret = Array.Empty<T>();
            T x = left[left.Count - 1];
            T y = right[right.Count - 1];

            if (x.Equals(y))
            {
                List<T> list = new List<T>(GetCollectionsLongestCommonSubsequence(
                    GetSubCollection(left, 0, left.Count - 1),
                    GetSubCollection(right, 0, right.Count - 1)));
                list.Add(x);
                return list.ToArray();
            }
            else
            {
                IList<T> lcs1 = GetCollectionsLongestCommonSubsequence(
                    left,
                    GetSubCollection(right, 0, right.Count - 1));

                if (lcs1.Count > right.Count)
                {
                    return lcs1;
                }

                IList<T> lcs2 = GetCollectionsLongestCommonSubsequence(
                    GetSubCollection(left, 0, left.Count - 1),
                    right);

                if (lcs1.Count > lcs2.Count)
                {
                    return lcs1;
                }
                else
                {
                    return lcs2;
                }
            }
        }

        private static string[] BreakString(string src, int maxlen)
        {
            List<string> ret = new List<string>();

            int i = 0;
            int len;

            while (true)
            {
                if (i >= src.Length)
                {
                    break;
                }

                if (i + maxlen > src.Length)
                {
                    len = src.Length - i;
                }
                else
                {
                    len = maxlen;
                }

                string s = src.Substring(i, len);
                ret.Add(s);
                i += len;
            }

            return ret.ToArray();
        }

        private static double CalcSimilarity(IList<string> common, IList<string> left, IList<string> right)
        {
            return ((common.Count / (double)left.Count) + (common.Count / (double)right.Count)) * 0.5;
        }

        /// <summary>
        /// Gets the similarity estimate of two strings (from 0.0 to 1.0)
        /// </summary>
        /// <param name="left">The first string</param>
        /// <param name="right">The second string</param>
        /// <returns>The similarity value (from 0.0 to 1.0)</returns>
        public static double GetStringSimilarity(string left, string right)
        {
            string[] left_arr = left.Split(' ');
            string[] right_arr = right.Split(' ');
            double ratio;

            if (left.Length > 80 || right.Length > 80)
            {
                string[] strings1 = BreakString(left, 40);
                string[] strings2 = BreakString(right, 40);

                int n = Math.Min(strings1.Length, strings2.Length);
                ratio = 0.0;

                for (int i = 0; i < n; i++)
                {
                    ratio += GetStringSimilarity(strings1[i], strings2[i]);
                }

                ratio = ratio / n;
                return ratio;
            }

            if (left_arr.Length <= 3 && right_arr.Length <= 3 && left.Length <= 20
                && right.Length <= 20)
            {
                IList<char> res = GetCollectionsLongestCommonSubsequence(left.ToCharArray(), right.ToCharArray());
                ratio = ((res.Count / (double)left.Length) + (res.Count / (double)right.Length)) * 0.5;
            }
            else
            {
                IList<string> arr_res = GetCollectionsLongestCommonSubsequence(left_arr, right_arr);
                ratio = CalcSimilarity(arr_res, left_arr, right_arr);
            }

            return ratio;
        }
    }
}
