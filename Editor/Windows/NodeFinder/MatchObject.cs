using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.Community.NodeFinderWindow;
using UnityObject = UnityEngine.Object;

namespace Unity.VisualScripting.Community
{
    public class MatchObject
    {
        public IGraphElement Element { get; }
        public GraphReference Reference { get; private set; }
        public string DisplayName { get; private set; }
        public string GraphPath { get; private set; }
        public UnityObject Target
        {
            get
            {
                var Object = Reference.rootObject;
                return Object;
            }
        }

        public bool IsErrorUnit { get; set; }

        public MatchObject(IGraphElement element, string displayName)
        {
            Element = element;
            DisplayName = displayName;
        }

        public string MatchString(string query, SearchMode searchMode, Unit unit = null)
        {
            var name = DisplayName;

            if (string.IsNullOrEmpty(query))
                return name;

            var normalizedName = new System.Text.StringBuilder();
            var indexMap = new List<int>();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c != ' ')
                {
                    normalizedName.Append(c);
                    indexMap.Add(i);
                }
            }

            var groups = query.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var highlights = new List<(int start, int length)>();

            foreach (var group in groups)
            {
                var parts = group.Split('>', 2, StringSplitOptions.RemoveEmptyEntries);
                var unitQuery = Normalize(parts[0].Trim());
                string portQuery = parts.Length > 1 ? Normalize(parts[1].Trim()) : null;

                string portTag = null;
                if (unitQuery.EndsWith("@CI", StringComparison.OrdinalIgnoreCase)) { portTag = "CI"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@CO", StringComparison.OrdinalIgnoreCase)) { portTag = "CO"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@VI", StringComparison.OrdinalIgnoreCase)) { portTag = "VI"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@VO", StringComparison.OrdinalIgnoreCase)) { portTag = "VO"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@I", StringComparison.OrdinalIgnoreCase)) { portTag = "I"; unitQuery = unitQuery[..^2]; }
                else if (unitQuery.EndsWith("@O", StringComparison.OrdinalIgnoreCase)) { portTag = "O"; unitQuery = unitQuery[..^2]; }

                bool unitMatch = unitQuery == "*" || searchMode switch
                {
                    SearchMode.Contains => normalizedName.ToString().IndexOf(unitQuery, StringComparison.OrdinalIgnoreCase) >= 0,
                    SearchMode.StartsWith => normalizedName.ToString().StartsWith(unitQuery, StringComparison.OrdinalIgnoreCase),
                    // SearchMode.Exact => normalizedName.ToString().Equals(unitQuery, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };

                if (!unitMatch) continue;

                if (portQuery != null && unit != null)
                {
                    List<IUnitPort> ports = null;
                    var matchingPorts = unit.validPorts
                        .Where(p => p.hasValidConnection)
                        .Where(p => SearchUtility.PortTagMatches(p, portTag))
                        .Where(p => SearchUtility.PortMatches(p, portQuery, searchMode, out ports))
                        .ToList();

                    if (matchingPorts.Count == 0)
                        continue;
                    ports?.Reverse();
                    name += $" <color=grey>{string.Join(", ", matchingPorts.Select(p => p.Description<UnitPortDescription>()?.label ?? p.key))}>{string.Join(">", ports.Select(p => p.Description<UnitPortDescription>()?.label ?? p.key))}</color>";
                }

                if (unitQuery == "*")
                {
                    if (name.Contains("<color=grey>"))
                        return "<b>" + name.Insert(name.IndexOf("<color=grey>"), "</b>");
                    else
                        return "<b>" + name + "</b>";
                }

                int matchIndex = searchMode switch
                {
                    SearchMode.Contains => normalizedName.ToString().IndexOf(unitQuery, StringComparison.OrdinalIgnoreCase),
                    SearchMode.StartsWith => normalizedName.ToString().StartsWith(unitQuery, StringComparison.OrdinalIgnoreCase) ? 0 : -1,
                    _ => -1
                };

                if (matchIndex >= 0)
                {
                    int matchLength = unitQuery.Length;
                    if (matchIndex + matchLength > indexMap.Count)
                        matchLength = indexMap.Count - matchIndex;

                    int originalStart = indexMap[matchIndex];
                    int originalEnd = indexMap[matchIndex + matchLength - 1];
                    highlights.Add((originalStart, originalEnd - originalStart + 1));
                    break;
                }

            }

            if (highlights.Count == 0)
                return name;

            highlights = highlights.OrderBy(h => h.start).ToList();
            var result = new System.Text.StringBuilder();
            int lastIndex = 0;

            foreach (var (start, length) in highlights)
            {
                if (start > lastIndex)
                    result.Append(name.Substring(lastIndex, start - lastIndex));

                result.Append("<b>");
                result.Append(name.Substring(start, Math.Min(length, name.Length - start)));
                result.Append("</b>");
                lastIndex = start + length;
            }

            if (lastIndex < name.Length)
                result.Append(name.Substring(lastIndex));

            return result.ToString();
        }

        private string Normalize(string input)
        {
            return input.Replace(" ", string.Empty);
        }

        public void Initialize(GraphReference reference)
        {
            Reference = reference;
            GraphPath = GetGraphPath(reference);
        }

        private string GetGraphPath(GraphReference reference)
        {
            if (ShouldUseFilePath(reference, out var macro))
            {
                string assetPath = AssetDatabase.GetAssetPath(macro as UnityObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetPath = assetPath.Replace("\\", "/");

                    var parts = assetPath.Split('/');
                    string fileName = parts.Last();

                    if (parts.Length == 2)
                        return parts.First() + "/" + Path.GetFileNameWithoutExtension(fileName);

                    string folder = parts[^2];

                    if (parts.Length == 3)
                        return parts.First() + "/" + folder + "/" + Path.GetFileNameWithoutExtension(fileName);

                    string name = Path.GetFileNameWithoutExtension(fileName);
                    return $"Assets/.../{folder}/{name}";
                }
            }
            else if (EditorUnityObjectUtility.IsSceneBound(reference.rootObject))
            {
                GameObject go = reference.rootObject as GameObject;
                if (go == null && reference.rootObject is Component c)
                {
                    go = c.gameObject;
                }

                if (go != null)
                {
                    string sceneName = go.scene.name;

                    var hierarchy = new List<string>();
                    var current = go.transform;
                    while (current != null)
                    {
                        hierarchy.Insert(0, current.name);
                        current = current.parent;
                    }

                    string path;
                    if (hierarchy.Count == 1)
                    {
                        path = $"{sceneName}/{hierarchy[0]}/{GraphTraversal.GetElementPath(reference)}";
                    }
                    else
                    {
                        path = $"{sceneName}/.../{hierarchy[^1]}/{GraphTraversal.GetElementPath(reference)}";
                    }

                    return path;
                }
            }
            return GetGraphTitle(reference);
        }

        private bool ShouldUseFilePath(GraphReference reference, out IMacro macro)
        {
            if (reference.machine != null && reference.machine.nest.source == GraphSource.Macro && reference.machine.nest.macro != null)
            {
                macro = reference.machine.nest.macro;
                return true;
            }
            else if (reference.macro != null)
            {
                macro = reference.macro;
                return true;
            }
            else
            {
                var parent = Element is INesterUnit nesterUnit ? nesterUnit : reference.isChild ? reference.parent : null;
                if (parent is INesterUnit nester && nester?.nest?.source == GraphSource.Macro && nester?.nest?.macro != null)
                {
                    macro = nester?.nest?.macro;
                    return true;
                }
            }
            macro = null;
            return false;
        }

        private static string GetGraphTitle(GraphReference reference)
        {
            if (!string.IsNullOrEmpty(reference.graph.title)) return reference.graph.title;

            if (reference.isRoot)
            {
                if (reference.scene == null) return reference.rootObject.name;
                if (reference.machine != null && reference.machine.nest.source == GraphSource.Macro && reference.machine.nest.macro is UnityObject @object)
                    return @object.name;
            }

            if (reference.isChild)
            {
                return GraphTraversal.GetParentName(reference);
            }

            return "Embed Machine";
        }
    }
}