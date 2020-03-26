using System.Collections.Generic;

public partial class Algorithm
{
    /// <summary>
    /// 使用DP思想判定字符串是否可按单词分割。
    /// </summary>
    /// <param name="s">需要匹配的字符串</param>
    /// <param name="wordSet">用于分割字符串的单词集</param>
    /// <returns>
    ///     true:字符串可按单词集中单词分割
    ///     false:字符串不可按单词集中单词分割
    /// </returns>
    /// <example>
    ///     eg. Input: s="castlejoycastlecatjoy" , wordSet=["joy","castle","cat"]
    ///         Output: true
    ///         Input: s="castlejoyecastlecatjoy" , wordSet=["joy","castle","cat"]
    ///         Output: false
    /// 
    /// </example>
    public bool WordSplit(string s, List<string> wordSet)
    {
        //输入合法性验证
        if (string.IsNullOrEmpty(s))
        {
            return true;
        }
        if (wordSet == null || wordSet.Count == 0)
        {
            return false;
        }
        //字符串长度
        int strLength = s.Length;
        //统计最小单词长度
        int minWordLength = strLength;
        for (int i = 0; i < wordSet.Count; i++)
        {
            if (wordSet[i].Length < minWordLength)
            {
                minWordLength = wordSet[i].Length;
            }
        }
        //特殊情况1：字符串与最小单词长度相等，则判定字符串是否与最小单词相同
        if (minWordLength.Equals(strLength))
        {
            return wordSet.Contains(s);
        }
        //特殊情况2：字符串短于最小单词长度，则字符串必不可按单词分割
        if (minWordLength>strLength)
        {
            return false;
        }
        //DP：若字符串可被分词，则必须满足
        bool[] dp = new bool[strLength + 1];
        dp[0] = true;
        int wordLength = 0;
        for (int splitIndex = 1; splitIndex <= strLength; splitIndex++)
        {
            /*改进思路：
            * 按单词集中的单词长度，从当前字符串分割处索引向前匹配，若连续匹配皆成功，则字符串符合分割规则。
            * 减少匹配次数，直接通过单词长度比较，效果优于原方式。*/
            for (int wordIndex = 0; wordIndex < wordSet.Count; wordIndex++)
            {
                wordLength = wordSet[wordIndex].Length;
                if (wordLength <= splitIndex)
                {
                    if (s.Substring(splitIndex - wordLength, wordLength).Equals(wordSet[wordIndex]) && dp[splitIndex - wordLength])
                    {
                        dp[splitIndex] = true;
                        break;
                    }
                }
            }
            //原始思路：从字符串头部向后按位匹配单词，截取长度小于最小单词长度则不匹配
            /*for (int j = 0; j < splitIndex; j++)
            {
                if (splitIndex - j < minWordLength)
                {
                    break;
                }
                dp[splitIndex] = dp[j] && wordDict.Contains(s.Substring(j, splitIndex - j));
                if (dp[splitIndex])
                {
                    break;
                }
            }*/
        }
        return dp[strLength];
    }
}

