using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class GraphKeyboardControlsWindow : EditorWindow
    {
        private static GraphKeyboardControlsWindow instance;
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
        public static void ShowWindow()
        {
            if (instance != null)
            {
                instance.Focus();
                return;
            }
            var window = CreateInstance<GraphKeyboardControlsWindow>();
            window.titleContent = new GUIContent("Graph Keyboard Controls");

            var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            window.position = new Rect(mousePosition.x, mousePosition.y, 280, 260);
            window.ShowPopup();
        }
        Vector2 scrollPosition;
        private void OnGUI()
        {
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 1, 1), () =>
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.Label("Keyboard Controls", EditorStyles.boldLabel);
                GUILayout.Space(10);

                DrawControlRow("Ctrl + Tab (Requires selection)", "Open Surround with Commands");
                DrawControlRow("Ctrl + / + /", "Create comment");
                DrawControlRow("Ctrl + / + / (Requires selection)", "Create comment with connections");
                DrawControlRow("← or → (Requires selection)\nThis only works on a single Unit \n(↑ to add Unit)", "Cycle through all ports");
                DrawControlRow("Ctrl + 1-9 (Requires selection)\nThis only works on a single Unit \n(↑ to add Unit)", "Cycle through Control ports");
                DrawControlRow("Alt + 1-9 (Requires selection)\nThis only works on a single Unit \n(↑ to add Unit)", "Cycle through Value ports");
                DrawControlRow("Tab (Requires Fuzzy Finder & Creating Connection)\nSnippet Layout: [Name],[Parameters(Separated by ',')]", "Add Graph Snippet");
                DrawControlRow("Space (While Creating Connection)", "Create Reroute");
                GUILayout.EndScrollView();
            }, true, true);
        }


        void DrawControlRow(string keys, string description)
        {
            if (!foldoutStates.ContainsKey(description))
                foldoutStates[description] = false;

            foldoutStates[description] = HUMEditor.Foldout(foldoutStates[description], new GUIContent(description), HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1,
            () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 0, 1), () =>
                {
                    GUILayout.Label(keys, EditorStyles.wordWrappedLabel);
                });
            });
        }


        private void OnLostFocus()
        {
            Close();
        }
    }
}