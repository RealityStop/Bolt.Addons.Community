using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public static string MakeSelectable(Unit unit, string code)
        {
            return $"[CommunityAddonsCodeSelectable({unit})]{code}[CommunityAddonsCodeSelectableEnd({unit})]";
        }

        public static string ToolTip(string ToolTip, string code)
        {
            return $"[CommunityAddonsCodeToolTip({ToolTip})]{code}[CommunityAddonsCodeToolTipEnd]";
        }

        private static readonly Regex ToolTipRegex = new(@"\[CommunityAddonsCodeToolTip\((.*?)\)\](.*?)\[CommunityAddonsCodeToolTipEnd\]", RegexOptions.Compiled);

        public static string ExtractTooltip(string code, out string tooltip)
        {
            var match = ToolTipRegex.Match(code);
            if (match.Success)
            {
                tooltip = match.Groups[1].Value;
                // Remove only the tooltip tags but keep the inner code content
                return ToolTipRegex.Replace(code, "$2");
            }
            tooltip = string.Empty;
            return code;
        }


        public static string RemoveCustomHighlights(string highlightedCode)
        {
            return RemoveHighlightsRegex.Replace(highlightedCode, "$1");
        }

        private static readonly Regex SelectableRegex = new Regex(@"\[CommunityAddonsCodeSelectable\((.*?)\)\]|\[CommunityAddonsCodeSelectableEnd\((.*?)\)\]", RegexOptions.Compiled);

        private static readonly Dictionary<string, List<ClickableRegion>> clickableRegionsCache = new();

        public static List<ClickableRegion> ExtractClickableRegions(string code)
        {
            if (clickableRegionsCache.TryGetValue(code, out var cachedRegions))
            {
                return cachedRegions;
            }

            var clickableRegions = new List<ClickableRegion>();
            var stack = new Stack<(int startIndex, string unitId)>();
            var lineBreaks = PrecomputeLineBreaks(code);

            foreach (Match match in SelectableRegex.Matches(code))
            {
                if (match.Groups[1].Success) // Start tag
                {
                    string unitId = match.Groups[1].Value;
                    stack.Push((match.Index + match.Length, unitId));
                }
                else if (match.Groups[2].Success && stack.Count > 0) // End tag
                {
                    var (startIndex, unitId) = stack.Pop();
                    int length = match.Index - startIndex;
                    var codePart = code.AsSpan(startIndex, length);

                    int startLine = GetLineNumber(lineBreaks, startIndex);
                    int endLine = GetLineNumber(lineBreaks, match.Index);

                    var newRegion = new ClickableRegion(unitId, codePart.ToString(), startLine, endLine);

                    // Check if the last region can be merged with the new one
                    if (clickableRegions.Count > 0 && clickableRegions[^1].unitId == unitId)
                    {
                        var lastRegion = clickableRegions[^1];

                        // Check if they are adjacent or overlap
                        if (lastRegion.endLine == newRegion.endLine)
                        {
                            // Merge by extending the last region's code and end line
                            lastRegion.code += codePart.ToString();
                            lastRegion.endLine = newRegion.endLine;

                            continue; // Skip adding newRegion as it's merged
                        }
                    }

                    clickableRegions.Add(newRegion);
                }
            }

            clickableRegionsCache[code] = clickableRegions;
            return clickableRegions;
        }

        private static List<int> PrecomputeLineBreaks(string code)
        {
            var lineBreaks = new List<int> { 0 };
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\n')
                {
                    lineBreaks.Add(i + 1);
                }
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