using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    internal static class GraphGUIFloatingToolbar
    {
        private static readonly Color BackgroundColor = EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);
        private static readonly Color HoverColor = EditorGUIUtility.isProSkin ? new Color32(76, 76, 76, 255) : new Color32(214, 214, 214, 255);
        private static readonly Color ActiveColor = EditorGUIUtility.isProSkin ? new Color32(88, 88, 88, 255) : new Color32(150, 150, 150, 255);
        private static readonly Color ActiveHoverColor = EditorGUIUtility.isProSkin ? new Color32(108, 108, 108, 255) : new Color32(130, 130, 130, 255);
        private static readonly Color BorderColor = EditorGUIUtility.isProSkin ? new Color32(26, 26, 26, 255) : new Color32(164, 164, 164, 255);

        private static bool previousDeveloperMode = BoltCore.Configuration.developerMode;

        private enum ButtonPosition
        {
            Left,
            Middle,
            Right,
            All
        }

        public static void Build(VisualElement root, GraphWindow window)
        {
            var state = GraphGUIState.Get(window);
            if (state.FloatingToolbar != null) return;

            var toolbar = CreateToolbarContainer(window);
            state.FloatingToolbar = toolbar;

            void RebuildToolbar()
            {
                toolbar.Clear();
                var reference = window.reference;
                if (reference == null || !reference.isValid) return;

                var canvas = reference.Context().canvas;
                toolbar.style.width = StyleKeyword.Auto;

                var errorSection = CreateSection("ErrorSection");
                errorSection.Add(CreateErrorButton(window));
                toolbar.Add(errorSection);

                if (canvas is FlowCanvas flowCanvas)
                {
                    var flowSection = CreateSection("CanvasSection");
                    flowSection.Add(CreateToggle(EditorGUIUtility.IconContent("UnityEditor.Graphs.AnimatorControllerTool").image, "Port Relations", flowCanvas.showRelations, v => flowCanvas.showRelations = v, ButtonPosition.Left));
                    flowSection.Add(CreateToggle(EditorGUIUtility.IconContent("UnityEditor.InspectorWindow").image, "Flow Values", BoltFlow.Configuration.showConnectionValues, v => { BoltFlow.Configuration.showConnectionValues = v; BoltFlow.Configuration.Save(); }, ButtonPosition.Middle));
                    flowSection.Add(CreateDimNodesToggle(ButtonPosition.Middle));
                    flowSection.Add(CreateToggle(BoltCore.Icons.window?[IconSize.Small], "Carry Children", BoltCore.Configuration.carryChildren, v => { BoltCore.Configuration.carryChildren = v; BoltCore.Configuration.Save(); }, ButtonPosition.Right));
                    toolbar.Add(flowSection);
                }
                else if (canvas is StateCanvas)
                {
                    var stateSection = CreateSection("CanvasSection");
                    stateSection.Add(CreateDimNodesToggle(ButtonPosition.All));
                    toolbar.Add(stateSection);
                }

                var layoutSection = CreateSection("LayoutSection");
                layoutSection.Add(CreateActionButton(PathUtil.Load("Align", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Align", b =>
                    LudiqGUI.FuzzyDropdown(b.worldBound, EnumOptionTree.For<AlignOperation>(), null, op => canvas.Align((AlignOperation)op)),
                    () => canvas.selection.Count > 1, ButtonPosition.Left));

                layoutSection.Add(CreateActionButton(PathUtil.Load("Distribute", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Distribute", b =>
                    LudiqGUI.FuzzyDropdown(b.worldBound, EnumOptionTree.For<DistributeOperation>(), null, op => canvas.Distribute((DistributeOperation)op)),
                    () => canvas.selection.Count > 1, ButtonPosition.Right));
                toolbar.Add(layoutSection);

                var windowSection = CreateSection("WindowSection", BoltCore.Configuration.developerMode);
                windowSection.Add(CreateActionButton((Texture2D)EditorGUIUtility.IconContent("SearchOverlay").image, "Overview", _ =>
                    GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(reference.graph.elements)), null , ButtonPosition.Left));

                windowSection.Add(CreateStatefulToggle(PathUtil.Load("maximize_window", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Maximize",
                    () => window.maximized,
                    v => { window.maximized = v; GUIUtility.ExitGUI(); }, ButtonPosition.Right));
                toolbar.Add(windowSection);

                if (BoltCore.Configuration.developerMode)
                {
                    var debugSection = CreateSection("DebugSection", false);
                    string iconName = EditorGUIUtility.isProSkin ? "debug On" : "debug";
                    debugSection.Add(CreateToggle((Texture2D)EditorGUIUtility.IconContent(iconName).image, "Debug", BoltCore.Configuration.debug, v => BoltCore.Configuration.debug = v, ButtonPosition.All));
                    toolbar.Add(debugSection);
                }
            }

            toolbar.schedule.Execute(() =>
            {
                if (previousDeveloperMode != BoltCore.Configuration.developerMode)
                {
                    previousDeveloperMode = BoltCore.Configuration.developerMode;
                    toolbar.Q("DebugSection").style.display = BoltCore.Configuration.developerMode ? DisplayStyle.Flex : DisplayStyle.None;
                    toolbar.Q("WindowSection").style.marginRight = BoltCore.Configuration.developerMode ? 12 : 0;
                }
            }).Every(500);

            RebuildToolbar();
            state.contextChanged += () => toolbar.schedule.Execute(RebuildToolbar);
            root.Add(toolbar);
        }

        static VisualElement CreateToolbarContainer(GraphWindow window)
        {
            var sidebars = GraphGUIUtilities.GetSidebars(window);
            const float rightMargin = 10f;
            return new VisualElement
            {
                name = "Floating-Toolbar",
                style =
                {
                    position = Position.Absolute,
                    flexDirection = FlexDirection.Row,
                    top = 25,
                    right = sidebars.right.show ? sidebars.right.GetWidth() + rightMargin : rightMargin,
                    height = 25,
                    backgroundColor = Color.clear,
                }
            };
        }

        private static VisualElement CreateSection(string sectionName, bool addSpace = true)
        {
            var section = new VisualElement
            {
                name = sectionName,
                style = {
                    flexDirection = FlexDirection.Row,
                    marginRight = addSpace ? 12 : 0,
                    height = 26
                }
            };
            return section;
        }

        private static ToolbarButton CreateBaseButton(Texture icon, string tooltip, ButtonPosition position)
        {
            var btn = new ToolbarButton
            {
                tooltip = tooltip,
                focusable = false,
                style = {
                    width = 34, height = 26,
                    justifyContent = Justify.Center, alignItems = Align.Center,
                    backgroundColor = BackgroundColor,
                    borderLeftColor = BorderColor, borderTopColor = BorderColor,
                    borderBottomColor = BorderColor, borderRightColor = BorderColor,
                    borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1,
                    borderTopLeftRadius = position == ButtonPosition.Left || position == ButtonPosition.All ? 3 : 0,
                    borderBottomLeftRadius = position == ButtonPosition.Left || position == ButtonPosition.All ? 3 : 0,
                    borderTopRightRadius = position == ButtonPosition.Right || position == ButtonPosition.All ? 3 : 0,
                    borderBottomRightRadius = position == ButtonPosition.Right || position == ButtonPosition.All ? 3 : 0,
                    marginLeft = -1
                }
            };

            if (icon != null)
            {
                var img = new Image { image = icon, scaleMode = ScaleMode.ScaleToFit };
                img.style.width = 16; img.style.height = 16;
                btn.Add(img);
            }

            return btn;
        }

        private static ToolbarButton CreateActionButton(Texture icon, string tooltip, Action<ToolbarButton> onClick, Func<bool> enabledCheck = null,ButtonPosition position = ButtonPosition.Middle)
        {
            var btn = CreateBaseButton(icon, tooltip, position);
            btn.clicked += () => onClick?.Invoke(btn);

            btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = HoverColor);
            btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.backgroundColor = BackgroundColor);

            if (enabledCheck != null)
            {
                btn.SetEnabled(enabledCheck());
                btn.schedule.Execute(() => btn.SetEnabled(enabledCheck())).Every(100);
            }

            return btn;
        }

        private static ToolbarButton CreateToggle(Texture icon, string tooltip, bool initialState, Action<bool> onToggle, ButtonPosition position)
        {
            bool state = initialState;
            var btn = CreateBaseButton(icon, tooltip, position);

            void UpdateStyle() => btn.style.backgroundColor = state ? ActiveColor : BackgroundColor;
            UpdateStyle();

            btn.clicked += () =>
            {
                state = !state;
                onToggle?.Invoke(state);
                btn.style.backgroundColor = state ? ActiveHoverColor : HoverColor;
            };

            btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = state ? ActiveHoverColor : HoverColor);
            btn.RegisterCallback<MouseLeaveEvent>(e => UpdateStyle());

            return btn;
        }

        private static ToolbarButton CreateStatefulToggle(Texture icon, string tooltip, Func<bool> getter, Action<bool> setter, ButtonPosition position)
        {
            var btn = CreateBaseButton(icon, tooltip, position);

            void UpdateStyle() => btn.style.backgroundColor = getter() ? ActiveColor : BackgroundColor;
            UpdateStyle();

            btn.clicked += () =>
            {
                bool newState = !getter();
                btn.style.backgroundColor = newState ? ActiveColor : BackgroundColor;
                setter?.Invoke(newState);
            };

            btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = getter() ? ActiveHoverColor : HoverColor);
            btn.RegisterCallback<MouseLeaveEvent>(e => UpdateStyle());

            return btn;
        }

        private static ToolbarButton CreateErrorButton(GraphWindow window)
        {
            var btn = CreateActionButton(EditorGUIUtility.IconContent("console.erroricon.inactive.sml").image, "Clear Errors", _ =>
            {
                var reference = window.reference;
                foreach (var ed in reference.debugData.elementsData.Where(e => e.runtimeException != null))
                    ed.runtimeException = null;
            }, null, ButtonPosition.All);

            bool hasErrors = window.reference?.debugData?.elementsData.Any(e => e.runtimeException != null) ?? false;

            btn.style.opacity = hasErrors ? 1 : 0;
            btn.tooltip = hasErrors ? "Clear Errors" : "";

            btn.schedule.Execute(() =>
            {
                var hasErrors = window.reference?.debugData?.elementsData.Any(e => e.runtimeException != null) ?? false;
                btn.style.opacity = hasErrors ? 1 : 0;
                btn.tooltip = hasErrors ? "Clear Errors" : "";
                btn.SetEnabled(hasErrors);
            }).Every(200);

            return btn;
        }

        private static ToolbarButton CreateDimNodesToggle(ButtonPosition position)
        {
            Texture onIcon = EditorGUIUtility.IconContent("VisibilityOff").image;
            Texture offIcon = EditorGUIUtility.IconContent("VisibilityOn").image;

            ToolbarButton btn = null;
            btn = CreateToggle(BoltCore.Configuration.dimInactiveNodes ? onIcon : offIcon, "Dim Nodes", BoltCore.Configuration.dimInactiveNodes, v =>
            {
                BoltCore.Configuration.dimInactiveNodes = v;
                BoltCore.Configuration.Save();
                var img = btn.Q<Image>();
                if (img != null) img.image = v ? onIcon : offIcon;
            }, position);
            return btn;
        }

        public static void KeepAnchored(GraphWindow window)
        {
            var state = GraphGUIState.Get(window);
            if (state.FloatingToolbar == null) return;
            const float margin = 10f;
            var sidebars = GraphGUIUtilities.GetSidebars(window);
            state.FloatingToolbar.style.right = sidebars.right.show ? sidebars.right.GetWidth() + margin : margin;
        }
    }
}