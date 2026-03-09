using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    internal static class GraphGUIStyles
    {
        private static Type sidebarpanelStylesType =
            typeof(SidebarPanel).GetNestedType("Styles", BindingFlags.NonPublic);

        private static bool isInitialized;

        public static bool IsInitialized { get => isInitialized; }

        public static void InitializeNewGUI()
        {
            if (isInitialized)
                return;

            isInitialized = true;

#if DARKER_UI

            var bg = sidebarpanelStylesType
                .GetField("background", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null) as GUIStyle;

            var title = sidebarpanelStylesType
                .GetField("title", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null) as GUIStyle;

            bg.normal.background = CommunityStyles.backgroundColor.GetPixel();
            title.normal.background = CommunityStyles.background;
            title.border = new RectOffset(0, 0, 2, 2);

            LudiqStyles.headerBackground.normal.background =
                CommunityStyles.backgroundColor.GetPixel();

#if !NEW_TOOLBAR_STYLE
            LudiqStyles.toolbarBackground.normal.background =
                CommunityStyles.backgroundColor.GetPixel();
#endif

            var pro = EditorGUIUtility.isProSkin;

            var tabField = typeof(VariablesPanel.Styles)
                .GetField("tab", BindingFlags.Static | BindingFlags.Public);

            var tab = tabField.GetValue(null) as GUIStyle;

            tab.normal.background = CommunityStyles.toolbarButtonBackground;

            tab.hover.background = CommunityStyles.MakeBorderedTexture(
                CommunityStyles.backgroundColor.Brighten(0.07f),
                pro
                    ? CommunityStyles.backgroundColor.Darken(0.1f)
                    : CommunityStyles.backgroundColor.Brighten(0.1f),
                BorderSide.LeftRight);

            tab.onNormal.background = CommunityStyles.MakeBorderedTexture(
                CommunityStyles.backgroundColor.Brighten(0.07f),
                pro
                    ? CommunityStyles.backgroundColor.Darken(0.1f)
                    : CommunityStyles.backgroundColor.Brighten(0.1f),
                BorderSide.LeftRight);

            tab.alignment = TextAnchor.MiddleLeft;
            tab.padding = new RectOffset(4, 0, 2, 2);
            tab.border = new RectOffset(2, 10, 0, 0);
            tab.overflow = new RectOffset(0, 1, 0, 0);
            tab.fixedHeight = 22;

            var subTabField = typeof(VariablesPanel.Styles)
                .GetField("subTab", BindingFlags.Static | BindingFlags.Public);

            subTabField.SetValue(null, new GUIStyle(tab)
            {
                alignment = TextAnchor.MiddleCenter
            });

            UnitEditor.Styles.inspectorBackground.normal.background = CommunityStyles.background;
            UnitEditor.Styles.portsBackground.normal.background = CommunityStyles.background;
            StateEditor.Styles.inspectorBackground.normal.background = CommunityStyles.background;

            LudiqStyles.toolbarButton.normal = CommunityStyles.ToolbarButton.normal;
            LudiqStyles.toolbarButton.hover = CommunityStyles.ToolbarButton.hover;
            LudiqStyles.toolbarButton.active = CommunityStyles.ToolbarButton.active;
            LudiqStyles.toolbarButton.onNormal = CommunityStyles.ToolbarButton.onNormal;
            LudiqStyles.toolbarButton.onHover = CommunityStyles.ToolbarButton.onHover;
            LudiqStyles.toolbarButton.onActive = CommunityStyles.ToolbarButton.onActive;

#endif

#if NEW_UNIT_STYLE

            ApplyNodeStyle(NodeColor.Green, "GreenNode");
            ApplyNodeStyle(NodeColor.Gray, "GrayNode");
            ApplyNodeStyle(NodeColor.Yellow, "YellowNode");
            ApplyNodeStyle(NodeColor.Orange, "OrangeNode", "OrangeNodeSelected");
            ApplyNodeStyle(NodeColor.Teal, "TealNode");
            ApplyNodeStyle(NodeColor.Blue, "BlueNode");
            ApplyNodeStyle(NodeColor.Red, "RedNode", "RedNodeSelected");

            var stateBackground = VisualScripting.StateWidget<FlowState>.Styles.contentBackground;

            var baseStateColor = CommunityStyles.backgroundColor;

            var statePortBackgroundtex =
                EditorGUIUtility.isProSkin
                    ? CommunityStyles.MakeBorderedTexture(baseStateColor, baseStateColor.Darken(0.05f))
                    : CommunityStyles.MakeBorderedTexture(baseStateColor, baseStateColor.Brighten(0.05f));

            stateBackground.normal.background = statePortBackgroundtex;

#if !ENABLE_VERTICAL_FLOW || !NEW_UNIT_UI

            var unitWidgetGeneric = typeof(Unity.VisualScripting.UnitWidget<>);

            var normalType = VisualScripting.UnitWidget<IUnit>.Styles.portsBackground;
            normalType.normal.background = statePortBackgroundtex;

            foreach (var type in Codebase.editorTypes)
            {
                if (!type.InheritsFromGeneric(unitWidgetGeneric, out var result))
                    continue;

                var stylesType = unitWidgetGeneric
                    .GetNestedType("Styles", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericType(result.GetGenericArguments());

                if (stylesType == null || stylesType.ContainsGenericParameters)
                    continue;

                var field = stylesType.GetField("portsBackground",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (field == null)
                    continue;

                var portsBackground = field.GetValue(null) as GUIStyle;

                if (portsBackground == null)
                    continue;

                portsBackground.normal.background = statePortBackgroundtex;
            }

#endif
#endif
        }

        private static void ApplyNodeStyle(NodeColor color, string normal, string selected = "SelectedNode")
        {
            var style = GraphGUI.GetNodeStyle(NodeShape.Square, color);

            style.normal.background =
                PathUtil.Load(normal, CommunityEditorPath.Fundamentals)?[IconSize.Large];

            style.active.background =
                PathUtil.Load(selected, CommunityEditorPath.Fundamentals)?[IconSize.Large];

            style.focused.background =
                PathUtil.Load(selected, CommunityEditorPath.Fundamentals)?[IconSize.Large];

            style.hover.background =
                PathUtil.Load(selected, CommunityEditorPath.Fundamentals)?[IconSize.Large];

            style.padding = new RectOffset(5, 5, 5, 5);
        }
    }
}