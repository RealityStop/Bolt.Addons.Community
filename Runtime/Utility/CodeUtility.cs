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
        private static readonly Regex RemoveHighlightsRegex = new(@"<b class='highlight'>(.*?)<\/b>", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Dictionary<string, string> RemoveAllCache = new();


        public static string RemoveAllClickableTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (RemoveAllCache.TryGetValue(input, out var cached)) return cached;

            const string openTag = "⟦";
            const string midTag = "⟧";
            const string closeTag = "⟧⟧";

            var result = new StringBuilder(input.Length);
            int index = 0;

            while (index < input.Length)
            {
                int openIdx = input.IndexOf(openTag, index);
                if (openIdx == -1)
                {
                    result.Append(input, index, input.Length - index);
                    break;
                }

                result.Append(input, index, openIdx - index);

                int midIdx = input.IndexOf(midTag, openIdx + openTag.Length);
                if (midIdx == -1)
                {
                    result.Append(input, openIdx, input.Length - openIdx);
                    break;
                }

                int closeIdx = input.IndexOf(closeTag, midIdx + midTag.Length);
                if (closeIdx == -1)
                {
                    result.Append(input, openIdx, input.Length - openIdx);
                    break;
                }

                int codeStart = midIdx + midTag.Length;
                int codeLength = closeIdx - codeStart;
                if (codeLength > 0)
                {
                    result.Append(input, codeStart, codeLength);
                }

                index = closeIdx + closeTag.Length;
            }

            var final = result.ToString();
            RemoveAllCache[input] = final;
            return final;
        }

        public static string MakeClickable(Unit unit, string code)
        {
            return $"⟦{unit}⟧{code}⟧⟧";
        }

        /// <summary>
        /// Used for the csharp preview to generate a tooltip.
        /// Do not use '\n' (newline) in tooltip or notify string it will break the tooltip extraction
        /// </summary>
        /// <returns>The string with the ToolTip tags</returns>
        public static string ErrorTooltip(string ToolTip, string notifyString, string code, bool highlight = true)
        {
            return CSharpPreviewSettings.ShouldGenerateTooltips ? $"[CommunityAddonsCodeToolTip({ToolTip})]{(highlight ? $"/* {notifyString} (Hover for more info) */".WarningHighlight() : $"/* {notifyString} (Hover for more info) */")}[CommunityAddonsCodeToolTipEnd] {code}" : (highlight ? $"/* {notifyString} */".WarningHighlight() : $"/* {notifyString} */") + code;
        }

        /// <summary>
        /// Used for the csharp preview to generate a tooltip.
        /// Do not use '\n' (newline) in tooltip it will break the tooltip extraction
        /// </summary>
        /// <returns>The string with the ToolTip tags</returns>
        public static string InfoTooltip(string ToolTip, string code, bool highlight = true)
        {
            return CSharpPreviewSettings.ShouldGenerateTooltips ? $"[CommunityAddonsCodeToolTip({ToolTip})]{(highlight ? $"/* Note: (Hover for more info) */".CommentHighlight() : $"/* Note: (Hover for more info) */")}[CommunityAddonsCodeToolTipEnd] {code}" : code;
        }

        private static readonly Dictionary<string, string> AllToolTipCache = new();
        private static readonly Regex ToolTipRegex = new(@"\[CommunityAddonsCodeToolTip\((.*?)\)\](.*?)\[CommunityAddonsCodeToolTipEnd\]", RegexOptions.Compiled);

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

        public static string CleanCode(string code, bool removeRecommendations = true)
        {
            return RemoveAllClickableTags(RemoveAllToolTipTagsEntirely(removeRecommendations ? RemoveRecommendations(code) : code));
        }

        private static readonly Dictionary<string, List<ClickableRegion>> clickableRegionsCache = new();

        public static List<ClickableRegion> ExtractAndPopulateClickableRegions(string input)
        {
            if (clickableRegionsCache.TryGetValue(input, out var cachedRegions))
                return cachedRegions;

            const string openTag = "⟦";
            const string midTag = "⟧";
            const string closeTag = "⟧⟧";

            var regions = new List<ClickableRegion>();
            ReadOnlySpan<char> span = input;
            var lineBreaks = PrecomputeLineBreaks(span);

            int index = 0;
            while (index < span.Length)
            {
                int openIdx = span.Slice(index).IndexOf(openTag);
                if (openIdx == -1) break;
                openIdx += index;

                int midIdx = span.Slice(openIdx + openTag.Length).IndexOf(midTag);
                if (midIdx == -1) break;
                midIdx += openIdx + openTag.Length;

                var unitIdSpan = span.Slice(openIdx + openTag.Length, midIdx - (openIdx + openTag.Length));
                string unitId = unitIdSpan.ToString();

                int closeIdx = span.Slice(midIdx + midTag.Length).IndexOf(closeTag);
                if (closeIdx == -1) break;
                closeIdx += midIdx + midTag.Length;

                int codeStart = midIdx + midTag.Length;
                int codeLength = closeIdx - codeStart;

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
                {
                    var last = regions[^1];
                    if (last.unitId == unitId && last.endLine == startLine)
                    {
                        last.code += code;
                        last.endLine = endLine;
                        regions[^1] = last;
                    }
                    else
                    {
                        regions.Add(newRegion);
                    }
                }
                else
                {
                    regions.Add(newRegion);
                }

                index = closeIdx + closeTag.Length;
            }

            clickableRegionsCache[input] = regions;
            return regions;
        }

        public static List<int> PrecomputeLineBreaks(ReadOnlySpan<char> span)
        {
            var lineBreaks = new List<int>(128) { 0 };
            int length = span.Length;

            for (int i = 0; i < length; i++)
            {
                if (span[i] == '\n')
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