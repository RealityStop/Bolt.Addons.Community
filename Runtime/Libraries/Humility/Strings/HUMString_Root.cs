using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility {
    public static partial class HUMString
    {
        public static Data.Remove Remove(this string text, string remove)
        {
            return new Data.Remove(text, remove);
        }

        public static string RemoveBetween(this string sourceString, string startTag, string endTag)
        {
            Regex regex = new Regex(string.Format("{0}(.*?){1}", Regex.Escape(startTag), Regex.Escape(endTag)), RegexOptions.RightToLeft);
            return regex.Replace(sourceString, startTag + endTag);
        }

        public static string RemoveAfterFirst(this string text, char character)
        {
            var startIndex = text.IndexOf(character);
            var output = text;
            if (startIndex < text.Length - 1) output = text.Remove(startIndex + 1, ((text.Length - 1) - startIndex));
            return output;
        }

        /// <summary>
        /// Begins adding something into text.
        /// </summary>
        public static Data.Add Add(this string text)
        {
            return new Data.Add(text);
        }

        /// <summary>
        /// Begins an operation that capitalizes some text.
        /// </summary>
        public static Data.Capitalize Capitalize(this string text)
        {
            return new Data.Capitalize(text);
        }

        public static string Nice(this string str)
        {
            var split = str.Add().Space().Between().Lowercase().And().Uppercase();
            var firstLetter = split[0].ToString().ToUpper();
            var capitalized = split.Remove(0).Insert(0, firstLetter);
            return capitalized;
        }

        public static string NullReplace(this string text, string replacement)
        {
            return string.IsNullOrEmpty(text) ? replacement : text;
        }

        public static string OnNullOrEmpty(this string textToCheck, string text)
        {
            return string.IsNullOrEmpty(textToCheck) ? text : string.Empty;
        }

        public static string OnNotNullOrEmpty(this string textToCheck, string text)
        {
            return string.IsNullOrEmpty(textToCheck) ? string.Empty : text;
        }

        public static string ReplaceFirstIf(this string text, string first, string replacement)
        {
            if (text[0].ToString() == first)
            {
                text.Remove(0, 0);
                text.Insert(0, replacement);
            }

            return text;
        }

        public static string ReplaceFirstIfNotNumberorDigitExcept(this string text, string[] exceptions)
        {
            var textReplacement = text[0].ToString().RemoveAllButLettersorDigitsExcept(exceptions);
            text.Remove(0, 1);
            text.Insert(0, textReplacement);

            return text;
        }

        public static string RemoveAllButLettersorDigitsExcept(this string member, string[] exceptions)
        {
            var charArray = member.ToCharArray();
            var newString = string.Empty;

            for (int i = 0; i < charArray.Length; i++)
            {
                var charString = charArray[i].ToString();
                var isSafe = true;
                
                for (int j = 0; j < exceptions.Length; j++)
                {
                    if (char.IsLetterOrDigit(charArray[i]) || charString == exceptions[j])
                    {
                        isSafe = true;
                        break;
                    }
                    else
                    {
                        if (j == exceptions.Length - 1)
                        {
                            isSafe = false;
                        }
                    }
                }

                if (isSafe) newString += charString;
            }

            return newString;
        }
    }
}
