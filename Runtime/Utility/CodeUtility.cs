using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class CodeUtility
    {
        private static readonly Regex RemoveAllTagsRegex = new(@"\[CommunityAddonsCodeSelectable(?:[^\]]*)\](.*?)\[CommunityAddonsCodeSelectableEnd\([^\]]*\)\]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex RemoveStartTagsRegex = new(@"\[CommunityAddonsCodeSelectable(?:[^\]]*)\]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex RemoveHighlightsRegex = new(@"<b class='highlight'>(.*?)<\/b>", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly ConcurrentDictionary<string, string> RemoveAllCache = new();

        private static readonly Dictionary<string, Regex> HighlightCodeRegexCache = new();

        public static string HighlightCode(string code, string unitId)
        {
            var regex = GetOrAddRegex(HighlightCodeRegexCache, unitId, id =>
            {
                var pattern = $@"\[CommunityAddonsCodeSelectable\({Regex.Escape(id)}\)\](.*?)(\[CommunityAddonsCodeSelectableEnd\({Regex.Escape(id)}\)\])";
                return new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);
            });

            var lines = code.Split(new[] { '\n' }, StringSplitOptions.None);
            var highlightedLines = new List<string>(lines.Length);

            foreach (var line in lines)
            {
                if (line.Contains($"[CommunityAddonsCodeSelectable({unitId})]"))
                {
                    var result = regex.Replace(line, match =>
                    {
                        var content = match.Groups[1].Value;
                        var highlightedContent = $"<b class='highlight'>{content}</b>";
                        return $"{match.Groups[0].Value.Replace(content, highlightedContent)}";
                    });
                    highlightedLines.Add(result);
                }
                else
                {
                    highlightedLines.Add(line);
                }
            }

            return string.Join("\n", highlightedLines);
        }

        private static TValue GetOrAddRegex<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dictionary[key] = value;
            }
            return value;
        }

        public static string RemoveAllSelectableTags(string code)
        {
            if (RemoveAllCache.TryGetValue(code, out string result))
            {
                return result;
            }

            result = RemoveStartTagsRegex.Replace(RemoveAllTagsRegex.Replace(code, "$1"), string.Empty);
            RemoveAllCache[code] = result;
            return result;
        }

        public static string RemovePattern(string input, string startPattern, string endPattern)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(startPattern) || string.IsNullOrEmpty(endPattern))
                return input;

            StringBuilder result = new();
            int index = 0;

            while (index < input.Length)
            {
                int startIndex = input.IndexOf(startPattern, index);
                if (startIndex == -1)
                {
                    // No more patterns, append the rest of the string
                    result.Append(input[index..]);
                    break;
                }

                // Append text before the pattern
                result.Append(input[index..startIndex]);

                // Find the end of the pattern
                int endIndex = input.IndexOf(endPattern, startIndex + startPattern.Length);
                if (endIndex == -1)
                {
                    // If end pattern not found, treat it as invalid and append the rest of the string
                    result.Append(input[startIndex..]);
                    break;
                }

                // Skip the pattern
                index = endIndex + endPattern.Length;
            }

            return result.ToString();
        }
        public static string MakeSelectable(Unit unit, string code)
        {
            return $"[CommunityAddonsCodeSelectable({unit})]{code}[CommunityAddonsCodeSelectableEnd({unit})]";
        }

        /// <summary>
        /// Used for the csharp preview to generate a tooltip
        /// </summary>
        /// <returns></returns>
        public static string ToolTip(string ToolTip, string notifyString, string code, bool highlight = true)
        {
            return CSharpPreviewSettings.ShouldGenerateTooltips ? $"[CommunityAddonsCodeToolTip({ToolTip})]{(highlight ? $"/* {notifyString} (Hover for more info) */".WarningHighlight() : $"/* {notifyString} (Hover for more info) */")}[CommunityAddonsCodeToolTipEnd] {code}" : code;
        }

        private static readonly Dictionary<string, string> ToolTipCache = new();
        private static readonly Dictionary<string, string> AllToolTipCache = new();
        private static readonly Regex ToolTipRegex = new(@"\[CommunityAddonsCodeToolTip\((.*?)\)\](.*?)\[CommunityAddonsCodeToolTipEnd\]", RegexOptions.Compiled);

        public static string RemoveAllToolTipTags(string code)
        {
            if (ToolTipCache.TryGetValue(code, out string result))
            {
                return result;
            }

            result = ToolTipRegex.Replace(code, "$2");
            ToolTipCache[code] = result;
            return result;
        }

        public static string RemoveAllToolTipTagsEntirely(string code)
        {
            if (AllToolTipCache.TryGetValue(code, out string result))
            {
                return result;
            }

            result = ToolTipRegex.Replace(code, string.Empty);
            AllToolTipCache[code] = result;
            return result;
        }

        public static string ExtractTooltip(string code, out string tooltip)
        {
            var match = ToolTipRegex.Match(code);
            if (match.Success)
            {
                tooltip = match.Groups[1].Value;
                return ToolTipRegex.Replace(code, "$2");
            }
            tooltip = string.Empty;
            return code;
        }

        private static readonly Regex RecommendationRegex = new(@"/\*\(Recommendation\) .*?\*/", RegexOptions.Compiled);

        public static string RemoveRecommendations(string code)
        {
            return RecommendationRegex.Replace(code, string.Empty);
        }

        public static string RemoveCustomHighlights(string highlightedCode)
        {
            return RemoveHighlightsRegex.Replace(highlightedCode, "$1");
        }

        public static string CleanCode(string code)
        {
            string result = code;
            result = RemoveAllSelectableTags(result);
            result = RemoveAllToolTipTags(result);
            result = RemoveRecommendations(result);
            result = RemoveCustomHighlights(result);
            return result;
        }


        private static readonly Regex SelectableRegex = new(@"\[CommunityAddonsCodeSelectable\((.*?)\)\]|\[CommunityAddonsCodeSelectableEnd\((.*?)\)\]", RegexOptions.Compiled);

        private static readonly Dictionary<string, List<ClickableRegion>> clickableRegionsCache = new();

        public static List<ClickableRegion> ExtractAndPopulateClickableRegions(string input)
        {
            if (clickableRegionsCache.TryGetValue(input, out var cachedRegions))
            {
                return cachedRegions;
            }

            var clickableRegions = new List<ClickableRegion>();
            var lineBreaks = PrecomputeLineBreaks(input.AsSpan(0, input.Length));
            int index = 0;

            while (index < input.Length)
            {
                int startSelectable = input.IndexOf("[CommunityAddonsCodeSelectable(", index);
                if (startSelectable == -1) break;
                int endSelectable = input.IndexOf(")]", startSelectable);
                if (endSelectable == -1) break;
                string unitId = input.Substring(startSelectable + "[CommunityAddonsCodeSelectable(".Length, endSelectable - (startSelectable + "[CommunityAddonsCodeSelectable(".Length));
                int startSelectableEnd = input.IndexOf($"[CommunityAddonsCodeSelectableEnd({unitId})]", endSelectable);
                if (startSelectableEnd == -1) break;
                int innerContentStart = endSelectable + 2;
                string code = input[innerContentStart..startSelectableEnd];

                int startLine = GetLineNumber(lineBreaks, startSelectable);
                int endLine = GetLineNumber(lineBreaks, startSelectableEnd);

                var clickableRegion = new ClickableRegion(unitId, code, startLine, endLine);

<<<<<<< Updated upstream
                // If there's a previous region with the same unitId and it's adjacent, merge them
                if (clickableRegions.Count > 0)
=======
                ReadOnlySpan<char> codeSpan = span.Slice(codeStart, codeLength);
                string code = codeSpan.ToString();

                int startLine = GetLineNumber(lineBreaks, openIdx);
                int endLine = GetLineNumber(lineBreaks, closeIdx);

                int lineStartIdx = (startLine == 0) ? 0 : lineBreaks[startLine - 1] + 1;
                int startIndex = openIdx - lineStartIdx;

                int endLineStartIdx = (endLine == 0) ? 0 : lineBreaks[endLine - 1] + 1;
                int endIndex = closeIdx - endLineStartIdx;

                var newRegion = new ClickableRegion(unitId, code, startLine, endLine, startIndex, endIndex);

                if (regions.Count > 0)
>>>>>>> Stashed changes
                {
                    var lastRegion = clickableRegions[clickableRegions.Count - 1];
                    if (lastRegion.unitId == unitId && lastRegion.endLine == startLine)
                    {
                        // Merge regions
                        lastRegion.code += code;
                        lastRegion.endLine = endLine;
                        clickableRegions[clickableRegions.Count - 1] = lastRegion;
                    }
                    else
                    {
                        clickableRegions.Add(clickableRegion);
                    }
                }
                else
                {
                    clickableRegions.Add(clickableRegion);
                }

                // Move index forward
                index = startSelectableEnd + $"[CommunityAddonsCodeSelectableEnd({unitId})]".Length;
            }

            clickableRegionsCache[input] = clickableRegions;
            return clickableRegions;
        }

        private static List<int> PrecomputeLineBreaks(ReadOnlySpan<char> span)
        {
            var lineBreaks = new List<int> { 0 };
            int start = 0;
            int index;
            while ((index = span[start..].IndexOf('\n')) != -1)
            {
                start += index + 1;
                lineBreaks.Add(start);
            }

            return lineBreaks;
        }

        private static int GetLineNumber(List<int> lineBreaks, int charIndex)
        {
            int line = lineBreaks.BinarySearch(charIndex);
            return line >= 0 ? line : ~line - 1;
        }
    }
}
