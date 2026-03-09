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
        public const float FloatingToolbarButtonSize = 28;

        private static bool previousDeveloperMode = BoltCore.Configuration.developerMode;

        public static void Build(VisualElement root, GraphWindow window)
        {
            var state = GraphGUIState.Get(window);

            if (state.FloatingToolbar != null)
                return;

            var toolbar = CreateFloatingToolbar(window);
            state.FloatingToolbar = toolbar;

            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.FlexEnd;
            toolbar.style.justifyContent = Justify.SpaceBetween;
            toolbar.style.flexShrink = 0;

            void RebuildToolbar()
            {
                toolbar.Clear();
                var reference = window.reference;
                if (reference == null || !reference.isValid) return;
                var canvas = reference.Context().canvas;
                toolbar.style.width = GraphGUIUtilities.GetCanvasToolbarWidth(canvas);
                ToolbarButton errorButton = null;
                errorButton = CreateFloatingButton(EditorGUIUtility.IconContent("console.erroricon").image, "Clear Errors", () =>
                {
                    var reference = window.reference;
                    foreach (var ed in reference.debugData.elementsData.Where(e => e.runtimeException != null))
                    {
                        ed.runtimeException = null;
                    }
                }, () =>
                {
                    // A bit hacky but using this so I do not need to add another IMGUI container just to detect the developer mode change
                    if (previousDeveloperMode != BoltCore.Configuration.developerMode)
                    {
                        previousDeveloperMode = BoltCore.Configuration.developerMode;
                        errorButton.schedule.Execute(() => RebuildToolbar());
                        return;
                    }

                    var reference = window.reference;
                    var erroredElementsDebugData = ListPool<IGraphElementDebugData>.New();

                    foreach (var elementDebugData in reference?.debugData?.elementsData ?? Enumerable.Empty<IGraphElementDebugData>())
                    {
                        if (elementDebugData.runtimeException != null)
                        {
                            erroredElementsDebugData.Add(elementDebugData);
                        }
                    }

                    if (erroredElementsDebugData.Count > 0)
                    {
                        errorButton.style.opacity = 1;
                        errorButton.SetEnabled(true);
                    }
                    else
                    {
                        errorButton.style.opacity = 0;
                        errorButton.SetEnabled(false);
                    }

                    erroredElementsDebugData.Free();
                });
                toolbar.Add(errorButton);

                if (canvas is FlowCanvas flowCanvas)
                {
                    var relationsButton = CreateToggleButton(EditorGUIUtility.IconContent("UnityEditor.Graphs.AnimatorControllerTool").image, "Port Relations", flowCanvas.showRelations, v => flowCanvas.showRelations = v);
                    var valuesButton = CreateToggleButton(EditorGUIUtility.IconContent("UnityEditor.InspectorWindow").image, "Flow Values", BoltFlow.Configuration.showConnectionValues, v => { BoltFlow.Configuration.showConnectionValues = v; BoltFlow.Configuration.Save(); });
                    var dimButton = CreateToggleButton(EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image, "Dim Nodes", BoltCore.Configuration.dimInactiveNodes, v => { BoltCore.Configuration.dimInactiveNodes = v; BoltCore.Configuration.Save(); });
                    var carryButton = CreateToggleButton(BoltCore.Icons.window?[IconSize.Small], "Carry Children", BoltCore.Configuration.carryChildren, v => { BoltCore.Configuration.carryChildren = v; BoltCore.Configuration.Save(); });

                    toolbar.Add(relationsButton);
                    toolbar.Add(valuesButton);
                    toolbar.Add(dimButton);
                    toolbar.Add(carryButton);
                }
                else if (canvas is StateCanvas stateCanvas)
                {
                    var dimButton = CreateToggleButton(EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image, "Dim States", BoltCore.Configuration.dimInactiveNodes, v => { BoltCore.Configuration.dimInactiveNodes = v; BoltFlow.Configuration.Save(); });
                    toolbar.Add(dimButton);
                }

                toolbar.Add(CreateEnumButton(PathUtil.Load("Align", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Align", (r) =>
                {
                    LudiqGUI.FuzzyDropdown(r, EnumOptionTree.For<AlignOperation>(), null, op => canvas.Align((AlignOperation)op));
                }, canvas));

                toolbar.Add(CreateEnumButton(PathUtil.Load("Distribute", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Distribute", (r) =>
                {
                    LudiqGUI.FuzzyDropdown(r, EnumOptionTree.For<DistributeOperation>(), null, op => canvas.Distribute((DistributeOperation)op));
                }, canvas));

                toolbar.Add(CreateOverviewButton(PathUtil.Load("Overview", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Overview", () =>
                {
                    GraphUtility.OverrideContextIfNeeded(() =>
                    canvas.ViewElements(reference.graph.elements));
                }));

                toolbar.Add(CreateWindowMaximizeButton(PathUtil.Load("Maximize", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Maximize", window));

                if (BoltCore.Configuration.developerMode)
                {
                    toolbar.Add(CreateToggleButton(typeof(Debug).Icon()?[IconSize.Small], "Debug", BoltCore.Configuration.debug, v =>
                    {
                        BoltCore.Configuration.debug = !BoltCore.Configuration.debug;
                    }));
                }
            }

            RebuildToolbar();
            state.contextChanged += () =>
            {
                toolbar.schedule.Execute(() => RebuildToolbar());
            };

            root.Add(toolbar);
        }

        static VisualElement CreateFloatingToolbar(GraphWindow window)
        {
            var context = window.context;
            var sidebars = GraphGUIUtilities.GetSidebars(window);
            const float rightMargin = 10f;
            return new VisualElement
            {
                name = "Floating-Toolbar",
                style =
                {
                    position = Position.Absolute,
                    top = 30,
                    right = sidebars.right.show ? sidebars.right.GetWidth() + rightMargin : rightMargin,
                    width = GraphGUIUtilities.GetCanvasToolbarWidth(context.canvas),
                    height = 25,
                    backgroundColor = Color.clear,
                }
            };
        }

        public static void KeepAnchored(GraphWindow window)
        {
            var state = GraphGUIState.Get(window);
            if (state.FloatingToolbar == null) return;

            const float rightMargin = 10f;

            try
            {
                var sidebars = GraphGUIUtilities.GetSidebars(window);
                state.FloatingToolbar.style.right =
                    sidebars.right.show ? sidebars.right.GetWidth() + rightMargin : rightMargin;
            }
            catch { }
        }


        private static ToolbarButton CreateFloatingButton(Texture icon, string tooltip, Action callback, Action imgui)
        {
            var btn = new ToolbarButton(() => callback())
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon as Texture2D;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif
            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = LudiqStyles.spinnerButton.normal.background;
                imgui();
            }));

            return btn;
        }

        private static ToolbarButton CreateToggleButton(Texture icon, string tooltip, bool isOn, Action<bool> callback)
        {
            ToolbarButton btn = null;
            btn = new ToolbarButton(() =>
            {
                isOn = !isOn;
                callback(isOn);
            })
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon as Texture2D;
#else
            if (icon != null)
            {
                var imgContainer = new VisualElement();
                imgContainer.style.flexGrow = 1;
                imgContainer.style.flexShrink = 1;
                imgContainer.style.justifyContent = Justify.Center;
                imgContainer.style.alignItems = Align.Center;

                var img = new Image();
                img.image = icon;
                img.scaleMode = ScaleMode.ScaleToFit;
                img.style.width = StyleKeyword.Auto;
                img.style.height = StyleKeyword.Auto;

                imgContainer.Add(img);
                btn.Add(imgContainer);
            }
#endif
            var imgui = new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = !isOn ? LudiqStyles.spinnerButton.normal.background : LudiqStyles.spinnerButton.active.background;
            });

            btn.Add(imgui);

            return btn;
        }

        private static ToolbarButton CreateWindowMaximizeButton(Texture icon, string tooltip, GraphWindow window)
        {
            ToolbarButton btn = null;
            btn = new ToolbarButton(() =>
            {
                window.maximized = !window.maximized;
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
                GUIUtility.ExitGUI();
            })
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon as Texture2D;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif

            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = !window.maximized ? LudiqStyles.spinnerButton.normal.background : LudiqStyles.spinnerButton.active.background;
            }));

            return btn;
        }

        private static ToolbarButton CreateOverviewButton(Texture2D icon, string tooltip, Action callback)
        {
            var btn = new ToolbarButton(callback)
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif

            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = LudiqStyles.spinnerButton.normal.background;
            }));

            return btn;
        }

        private static ToolbarButton CreateEnumButton(Texture2D icon, string tooltip, Action<Rect> callback, ICanvas canvas)
        {
            bool show = false;
            var btn = new ToolbarButton(() =>
            {
                show = true;
            })
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = LudiqStyles.spinnerButton.normal.background;
                btn.SetEnabled(canvas.selection.Count > 1);

                if (show)
                {
                    show = false;
                    callback?.Invoke(btn.layout);
                }
            }));

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif

            return btn;
        }
    }
}