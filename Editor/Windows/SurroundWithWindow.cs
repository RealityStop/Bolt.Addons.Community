using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class SurroundWithWindow : EditorWindow
    {
        private static List<Type> surroundCommands;
        public static Action<SurroundCommand> onCommandSelected {get; private set; }
        private bool positionSet;
        private Vector2 mousePosition;

        public static SurroundWithWindow Window { get; private set; }

        /// <summary>
        /// Show the popup window at the mouse position.
        /// </summary>
        public static SurroundWithWindow ShowWindow(Action<SurroundCommand> onCommandSelected)
        {
            var window = CreateInstance<SurroundWithWindow>();
            SurroundWithWindow.onCommandSelected = onCommandSelected;
            surroundCommands = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ISurroundWithCommandBase).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .ToList();
            window.ShowPopup();
            Window = window;
            return window;
        }

        void OnDestroy()
        {
            EditorApplicationUtility.onAssemblyReload -= Close;
        }
        private GUIStyle _buttonStyle;

        private GUIStyle buttonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle() { normal = { background = HUMColor.CacheTexture(HUMEditorColor.DefaultEditorBackground.Darken(0.15f)), textColor = Color.white }, hover = { background = HUMColor.CacheTexture(HUMEditorColor.DefaultEditorBackground), textColor = Color.white } };
                }
                return _buttonStyle;
            }
        }
        Vector2 scrollPos = new Vector2();
        private void OnGUI()
        {
            if (surroundCommands == null) Close();
            if (!positionSet)
            {
                mousePosition = Event.current.mousePosition.Add(new Vector2(10, 0));
                minSize = new Vector2(250, (EditorGUIUtility.singleLineHeight * (surroundCommands.Count + 2)) + EditorStyles.boldLabel.CalcSize(new GUIContent("Surround With [...]:")).y);
                maxSize = new Vector2(250, (EditorGUIUtility.singleLineHeight * (surroundCommands.Count + 2)) + EditorStyles.boldLabel.CalcSize(new GUIContent("Surround With [...]:")).y);
                positionSet = true;
            }

            position = new Rect(mousePosition, new Vector2(250, surroundCommands == null ? 300f : ((EditorGUIUtility.singleLineHeight * (surroundCommands.Count + 2)) + EditorStyles.boldLabel.CalcSize(new GUIContent("Surround With [...]:")).y)));
            if (focusedWindow != this) Close();
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 2, 2), () =>
            {
                if (surroundCommands == null || surroundCommands.Count == 0)
                {
                    HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 2, 2), () =>
                    {
                        EditorGUILayout.LabelField("No Surround With Commands Found");
                    });
                    return;
                }

                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 2, 2), () =>
                {
                    EditorGUILayout.LabelField("Surround With Commands:", EditorStyles.boldLabel);
                });
                GUILayout.Space(4);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                foreach (var command in surroundCommands)
                {
                    HUMEditor.Horizontal(() =>
                    {
                        GUILayout.Label("[...]", GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("[...]")).x));
                        var instance = (SurroundCommand)Activator.CreateInstance(command);
                        if (GUILayout.Button(instance.DisplayName, buttonStyle))
                        {
                            onCommandSelected?.Invoke((SurroundCommand)Activator.CreateInstance(command));
                            Close();
                        }
                    });
                }
                GUILayout.EndScrollView();
            }, true, true);
        }
    }
}