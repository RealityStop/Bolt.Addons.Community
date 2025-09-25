using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public class GraphKeyboardControlsPopup : PopupWindowContent
    {
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
        private Vector2 scrollPosition;

        private struct KeyboardControl
        {
            public string Keys { get; private set; }
            public string Description { get; private set; }
            public KeyboardControl(string keys, string description)
            {
                Keys = keys;
                Description = description;
            }
        }

        private readonly List<KeyboardControl> normalControls = new()
        {
            new("[Ctrl] + [/] + [/]", "Create comment"),
            new("[Ctrl] + [Tab]", "Cycle through graph elements"),
        };

        private readonly List<KeyboardControl> selectionControls = new()
        {
            new("[Ctrl] + [Shift] + [T]", "Open Surround with Commands"),
            new("[Ctrl] + [/] + [/]", "Create comment with connections"),
            new("[←] [→] [↑] [↓]", "Move Selected Units"),
            new("[Ctrl/Cmd] + [←] or [→]\nSingle Unit Only\nUse ↑ to add Unit", "Cycle through all ports"),
            new("[Shift] + [←] or [→]\nSingle Unit Only\nUse ↑ to add Unit", "Cycle through Control ports"),
            new("[Alt] + [←] or [→]\nSingle Unit Only\nUse ↑ to add Unit", "Cycle through Value ports"),
        };

        private readonly List<KeyboardControl> creatingConnectionControls = new()
        {
            new("[Tab]\nIn Fuzzy Finder\nSnippet Layout:\n[Name],[Parameters(Separated by ',')]", "Add Graph Snippet"),
            new("[Space]", "Create Reroute")
        };

        public override Vector2 GetWindowSize()
        {
            return new Vector2(350, (normalControls.Count + selectionControls.Count + creatingConnectionControls.Count) * 40);
        }

        public override void OnGUI(Rect rect)
        {
            HUMEditor.Vertical().Box(
                HUMEditorColor.DefaultEditorBackground.Darken(0.15f),
                Color.black,
                new RectOffset(4, 4, 4, 4),
                new RectOffset(1, 1, 1, 1),
                () =>
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    GUILayout.Label("Keyboard Controls", EditorStyles.boldLabel);
                    GUILayout.Space(8);

                    GUILayout.Label("Normal", EditorStyles.boldLabel);
                    foreach (var control in normalControls)
                        DrawControlRow(control.Keys, control.Description);

                    GUILayout.Label("Requires Selection", EditorStyles.boldLabel);
                    foreach (var control in selectionControls)
                        DrawControlRow(control.Keys, control.Description);

                    GUILayout.Label("Requires Creating Connection", EditorStyles.boldLabel);
                    foreach (var control in creatingConnectionControls)
                        DrawControlRow(control.Keys, control.Description);

                    GUILayout.EndScrollView();
                },
                true, true
            );
        }

        private void DrawControlRow(string keys, string description)
        {
            if (!foldoutStates.TryGetValue(description, out bool isOpen))
                isOpen = false;

            foldoutStates[description] = HUMEditor.Foldout(
                isOpen,
                new GUIContent(description),
                HUMEditorColor.DefaultEditorBackground.Darken(0.15f),
                Color.black,
                1,
                () =>
                {
                    HUMEditor.Vertical().Box(
                        HUMEditorColor.DefaultEditorBackground.Darken(0.15f),
                        Color.black,
                        new RectOffset(4, 4, 4, 4),
                        new RectOffset(1, 1, 0, 1),
                        () =>
                        {
                            GUILayout.Label(keys, EditorStyles.wordWrappedLabel);
                        });
                });
        }

        public override void OnClose()
        {
            foldoutStates.Clear();
        }

        public static void Show(Rect activatorRect)
        {
            PopupWindow.Show(activatorRect, new GraphKeyboardControlsPopup());
        }
    }
}