using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    [InitializeAfterPlugins]
    public static class GraphGUIPatch
    {
        private static readonly HashSet<VisualElement> patchedRoots = new HashSet<VisualElement>();

        static GraphGUIPatch()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        static void OnEditorUpdate()
        {
#if NEW_TOOLBAR_STYLE
            var tabs = GraphWindow.tabs;
            if (tabs == null || tabs.Count() == 0)
                return;

            foreach (var window in tabs)
            {
                if (window == null || window.rootVisualElement == null || window.reference == null || window.context == null)
                {
                    if (patchedRoots.Contains(window.rootVisualElement))
                    {
                        patchedRoots.Remove(window.rootVisualElement);
                        GraphGUIState.Remove(window);
                    }
                    continue;
                }

                var disableUI = GraphGUIUtilities.DisableUI;

                if (!disableUI && !patchedRoots.Contains(window.rootVisualElement))
                {
                    PatchGraphGUI(window);
                    patchedRoots.Add(window.rootVisualElement);
                }
                else if (disableUI)
                {
                    patchedRoots.Remove(window.rootVisualElement);
                    GraphGUIState.Remove(window);
                }

                GraphGUIFloatingToolbar.KeepAnchored(window);
            }
#else
            var tabs = GraphWindow.tabs;
            if (tabs == null || tabs.Count() == 0)
                return;
            var window = tabs.FirstOrDefault();
            if (window != null)
            {
                var IMGUI = new IMGUIContainer();
                IMGUI.onGUIHandler += () =>
                {
                    InitializeNewGUI();
                };
                window.rootVisualElement.Add(IMGUI);

                if (GraphGUIStyles.IsInitialized)
                {
                    EditorApplication.update -= OnEditorUpdate;
                    IMGUI.RemoveFromHierarchy();
                }
            }
#endif
        }

        static void PatchGraphGUI(GraphWindow window)
        {
            var root = window.rootVisualElement;

            GraphGUIToolbar.Build(root, window);
            GraphGUIFloatingToolbar.Build(root, window);
        }

        public static void InitializeNewGUI()
        {
            GraphGUIStyles.InitializeNewGUI();
        }
    }
}