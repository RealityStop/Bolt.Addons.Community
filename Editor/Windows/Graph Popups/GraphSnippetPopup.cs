using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public class GraphSnippetsPopup : PopupWindowContent
    {
        private Vector2 scrollPosition;
        private readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        private struct SnippetInfo
        {
            public string Name { get; }
            public string Parameters { get; }

            public SnippetInfo(string name, string parameters)
            {
                Name = name;
                Parameters = parameters;
            }
        }

        private readonly List<SnippetInfo> controlSnippets;
        private readonly List<SnippetInfo> valueSnippets;

        public GraphSnippetsPopup()
        {
            controlSnippets = AssetDatabase.FindAssets($"t:{typeof(ControlGraphSnippet)}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<ControlGraphSnippet>(path))
                .Where(snippet => snippet != null)
                .Select(snippet => new SnippetInfo(
                    snippet.SnippetName,
                    string.Join(", ", snippet.snippetArguments.Select(a => a.argumentType.DisplayName() + " : " + a.argumentName))
                ))
                .ToList();

            valueSnippets = AssetDatabase.FindAssets($"t:{typeof(ValueGraphSnippet)}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<ValueGraphSnippet>(path))
                .Where(snippet => snippet != null)
                .Select(snippet => new SnippetInfo(
                    snippet.SnippetName,
                    string.Join(", ", snippet.snippetArguments.Select(a => a.argumentType.DisplayName() + " : " + a.argumentName))
                ))
                .ToList();
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(350, Mathf.Min(500, (controlSnippets.Count + valueSnippets.Count) * 20));
        }

        public override void OnGUI(Rect rect)
        {
            HUMEditor.Vertical().Box(
                CommunityStyles.backgroundColor,
                Color.black,
                new RectOffset(4, 4, 4, 4),
                new RectOffset(1, 1, 1, 1),
                () =>
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    GUILayout.Label("Graph Snippets", EditorStyles.boldLabel);
                    GUILayout.Space(8);

                    GUILayout.Label("Control Snippets", EditorStyles.miniBoldLabel);
                    foreach (var group in controlSnippets.GroupBy(s => s.Name).OrderBy(g => g.Key))
                        DrawSnippetGroup(group.Key, group);

                    GUILayout.Space(6);
                    GUILayout.Label("Value Snippets", EditorStyles.miniBoldLabel);
                    foreach (var group in valueSnippets.GroupBy(s => s.Name).OrderBy(g => g.Key))
                        DrawSnippetGroup(group.Key, group);

                    GUILayout.EndScrollView();
                },
                true, true
            );
        }

        private void DrawSnippetGroup(string name, IEnumerable<SnippetInfo> overloads)
        {
            var overloadList = overloads.ToList();
            string key = name;

            if (!foldoutStates.TryGetValue(key, out bool isOpen))
                isOpen = false;

            string foldoutLabel = $"{name} ({overloadList.Count})";

            foldoutStates[key] = HUMEditor.Foldout(
                isOpen,
                new GUIContent(foldoutLabel),
                CommunityStyles.foldoutHeaderColor,
                Color.black,
                1,
                () =>
                {
                    HUMEditor.Vertical().Box(
                        CommunityStyles.foldoutBackgroundColor,
                        Color.black,
                        new RectOffset(4, 4, 4, 4),
                        new RectOffset(1, 1, 0, 1),
                        () =>
                        {
                            GUILayout.Label("Optional Parameters:", EditorStyles.boldLabel);
                            foreach (var overload in overloadList)
                            {
                                string paramText;

                                if (string.IsNullOrEmpty(overload.Parameters))
                                {
                                    paramText = "(no parameters)";
                                }
                                else
                                {
                                    var formatted = overload.Parameters
                                        .Split(',')
                                        .Select(p => p.Trim())
                                        .ToArray();

                                    paramText = "(" + string.Join(", ", formatted) + ")";
                                }

                                GUILayout.Label(paramText, EditorStyles.miniLabel);
                            }
                        });
                });
        }

        public override void OnClose()
        {
            foldoutStates.Clear();
        }

        public static void Show(Rect activatorRect)
        {
            PopupWindow.Show(activatorRect, new GraphSnippetsPopup());
        }
    }
}