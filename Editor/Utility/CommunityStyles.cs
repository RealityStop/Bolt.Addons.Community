#pragma warning disable
using System;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [System.Flags]
    public enum BorderSide
    {
        None = 0,
        Top = 1 << 0,
        Bottom = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        All = Top | Bottom | Left | Right,
        LeftRight = Left | Right,
        TopBottom = Top | Bottom
    }

    public static class CommunityStyles
    {
        public static readonly Color backgroundColor;
        public static readonly Color foldoutBackgroundColor;
        public static readonly Color foldoutHeaderColor;
        public static readonly Color highlightColor = new Color(0.3f, 0.45f, 0.6f, 1f);

        public static readonly Texture2D OutlineTexture;
        public static readonly Texture2D background;
        public static readonly Texture2D headerBackground;
        public static readonly Texture2D toolbarButtonBackground;
        public static readonly Texture2D sidebarAnchorButtonBackground;
        public static readonly Texture2D ArrowDownTexture;
        public static readonly Texture2D ArrowRightTexture;
        public static readonly Texture2D DragHandleTexture;
        public static readonly Texture2D RemoveItemTexture;

        private static GUIStyle toolbarButton;
        internal static Texture2D toolbarButtonNormalTex;
        private static Texture2D toolbarButtonHoverTex;

        private static GUIStyle sidebarAnchorButton;
        private static Texture2D sidebarAnchorButtonNormalTex;
        private static Texture2D sidebarAnchorButtonHoverTex;
        private static bool lastProSkin;

        public static GUIStyle ToolbarButton
        {
            get
            {
                bool pro = EditorGUIUtility.isProSkin;

                if (toolbarButton == null || pro != lastProSkin)
                {
                    lastProSkin = pro;

                    toolbarButtonNormalTex = toolbarButtonBackground;
                    toolbarButtonHoverTex = MakeBorderedTexture(CommunityStyles.backgroundColor.Brighten(0.07f),
                    pro ? CommunityStyles.backgroundColor.Darken(0.1f) : CommunityStyles.backgroundColor.Brighten(0.1f), BorderSide.LeftRight);

                    toolbarButton = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        padding = new RectOffset(6, 6, 2, 2),
                        border = new RectOffset(2, 2, 0, 0),
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 11,
                        fontStyle = FontStyle.Normal,

                        normal = new GUIStyleState()
                        {
                            background = toolbarButtonNormalTex,
                            textColor = pro ? new Color(0.8f, 0.8f, 0.8f) : Color.black
                        },
                        hover = new GUIStyleState()
                        {
                            background = toolbarButtonHoverTex,
                            textColor = pro ? Color.white : Color.black
                        },
                        active = new GUIStyleState()
                        {
                            background = toolbarButtonHoverTex,
                            textColor = Color.white
                        },
                        onNormal = new GUIStyleState()
                        {
                            background = toolbarButtonHoverTex,
                            textColor = Color.white
                        },
                        onHover = new GUIStyleState()
                        {
                            background = toolbarButtonHoverTex,
                            textColor = Color.white
                        },
                        onActive = new GUIStyleState()
                        {
                            background = toolbarButtonHoverTex,
                            textColor = Color.white
                        }
                    };
                }

                return toolbarButton;
            }
        }

        public static GUIStyle SidebarAnchorButton
        {
            get
            {
                bool pro = EditorGUIUtility.isProSkin;

                if (sidebarAnchorButton == null || pro != lastProSkin)
                {
                    lastProSkin = pro;

                    sidebarAnchorButtonNormalTex = sidebarAnchorButtonBackground;
                    sidebarAnchorButtonHoverTex = MakeBorderedTexture(CommunityStyles.backgroundColor.Brighten(0.07f),
                    pro ? CommunityStyles.backgroundColor.Darken(0.1f) : CommunityStyles.backgroundColor.Brighten(0.1f), BorderSide.LeftRight | BorderSide.Top);

                    sidebarAnchorButton = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        padding = new RectOffset(6, 6, 2, 2),
                        border = new RectOffset(2, 2, 0, 0),
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 11,
                        fontStyle = FontStyle.Normal,

                        normal = new GUIStyleState()
                        {
                            background = sidebarAnchorButtonNormalTex,
                            textColor = pro ? new Color(0.8f, 0.8f, 0.8f) : Color.black
                        },
                        hover = new GUIStyleState()
                        {
                            background = sidebarAnchorButtonHoverTex,
                            textColor = pro ? Color.white : Color.black
                        },
                        active = new GUIStyleState()
                        {
                            background = sidebarAnchorButtonHoverTex,
                            textColor = Color.white
                        },
                        onNormal = new GUIStyleState()
                        {
                            background = sidebarAnchorButtonHoverTex,
                            textColor = Color.white
                        },
                        onHover = new GUIStyleState()
                        {
                            background = sidebarAnchorButtonHoverTex,
                            textColor = Color.white
                        },
                        onActive = new GUIStyleState()
                        {
                            background = sidebarAnchorButtonHoverTex,
                            textColor = Color.white
                        }
                    };
                }

                return sidebarAnchorButton;
            }
        }

        static CommunityStyles()
        {
            backgroundColor = Unity.VisualScripting.ColorPalette.unityBackgroundMid.color.Darken(0.05f);
            foldoutBackgroundColor = Unity.VisualScripting.ColorPalette.unityBackgroundDark.color.Darken(0.02f);
            foldoutHeaderColor = Unity.VisualScripting.ColorPalette.unityBackgroundDark.color.Darken(0.05f);

            OutlineTexture = PathUtil.Load("Outline", CommunityEditorPath.Fundamentals)?[IconSize.Small];

            ArrowDownTexture = PathUtil.Load("ArrowDown", CommunityEditorPath.Fundamentals)?[IconSize.Small];
            ArrowRightTexture = PathUtil.Load("ArrowRight", CommunityEditorPath.Fundamentals)?[IconSize.Small];
            DragHandleTexture = PathUtil.Load("DragHandle", CommunityEditorPath.Fundamentals)?[IconSize.Small];
            RemoveItemTexture = PathUtil.Load("RemoveItem", CommunityEditorPath.Fundamentals)?[IconSize.Small];

            var bg = CommunityStyles.backgroundColor;
            var border = EditorGUIUtility.isProSkin ? CommunityStyles.backgroundColor.Darken(0.15f) : CommunityStyles.backgroundColor.Brighten(0.15f);
            background = MakeBorderedTexture(bg, border);

            var headerBg = CommunityStyles.backgroundColor;
            var headerBorder = EditorGUIUtility.isProSkin ? CommunityStyles.backgroundColor.Darken(0.05f) : CommunityStyles.backgroundColor.Brighten(0.05f);
            headerBackground = MakeBorderedTexture(headerBg, border, BorderSide.Bottom);

            var toolbarButtonBg = CommunityStyles.backgroundColor;
            var buttonBorder = EditorGUIUtility.isProSkin ? CommunityStyles.backgroundColor.Darken(0.05f) : CommunityStyles.backgroundColor.Brighten(0.05f);
            toolbarButtonBackground = MakeBorderedTexture(headerBg, border, BorderSide.LeftRight);

            var sidebarAnchorButtonBg = CommunityStyles.backgroundColor;
            var sidebarAnchorButtonBorder = EditorGUIUtility.isProSkin ? CommunityStyles.backgroundColor.Darken(0.05f) : CommunityStyles.backgroundColor.Brighten(0.05f);
            sidebarAnchorButtonBackground = MakeBorderedTexture(headerBg, border, BorderSide.LeftRight | BorderSide.Top);
        }

        public static Texture2D MakeBorderedTexture(Color background, Color border, BorderSide borderSides = BorderSide.TopBottom, int width = 32, int height = 32, int borderThickness = 1)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            borderThickness = Mathf.Clamp(borderThickness, 1, Mathf.Min(width, height));

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = false;

                    if ((borderSides & BorderSide.Top) != 0 && y >= height - borderThickness)
                        isBorder = true;
                    else if ((borderSides & BorderSide.Bottom) != 0 && y < borderThickness)
                        isBorder = true;
                    else if ((borderSides & BorderSide.Left) != 0 && x < borderThickness)
                        isBorder = true;
                    else if ((borderSides & BorderSide.Right) != 0 && x >= width - borderThickness)
                        isBorder = true;

                    pixels[y * width + x] = isBorder ? border : background;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }

        private static string[] preferredNames = new string[] { "key", "title", "id", "label", "name" };

        /// <summary>
        /// Get the display name for metadata when in a collection.
        /// </summary>
        /// <param name="element">The metadata</param>
        /// <param name="index">The metadata index in the collection</param>
        /// <param name="valueAsFallback">If the value is primitive use the value as the display name if none other can be resolved.</param>
        /// <returns>The GUIContent with the Name and Icon</returns>
        public static GUIContent GetCollectionDisplayName(Metadata element, int index, bool valueAsFallback = false)
        {
            object value = element?.value;

            if (value == null)
                return new GUIContent($"Value {index + 1}", typeof(Null).Icon()?[IconSize.Small]);

            if (value is string s && !string.IsNullOrEmpty(s))
                return new GUIContent(s, typeof(string).Icon()?[IconSize.Small]);

            if (value is Type type)
            {
                return new GUIContent(type.DisplayName(), type.Icon()?[IconSize.Small]);
            }

            if (value is UnityEngine.Object uobj)
            {
                if (uobj is UnityEngine.Object uv)
                {
                    if (uv is IEventMachine machine)
                        return new GUIContent(GetMachineName(machine), machine.GetType().Icon()?[IconSize.Small]);
                    else if (uv is IMacro macro)
                        return new GUIContent(GetMacroName(macro), macro.GetType().Icon()?[IconSize.Small]);
                    else if (!string.IsNullOrEmpty(uv.name))
                        return new GUIContent(uv.name, uv.GetType().Icon()?[IconSize.Small]);
                }
            }

            if (value is ICollection collection)
            {
                return new GUIContent($"{collection.GetType().DisplayName()} [{collection.Count}]", collection.GetType().Icon()?[IconSize.Small]);
            }

            object TryGetMemberValue(string memberName)
            {
                try
                {
                    var member = element[memberName];
                    return element[memberName].value;
                }
                catch
                {
                    return null; // The member likely does not exist
                }
            }

            foreach (var pref in preferredNames)
            {
                var mv = TryGetMemberValue(pref);
                if (mv != null)
                {
                    var icon = value.GetType().Icon()?[IconSize.Small];
                    if (mv is UnityEngine.Object uv)
                    {
                        if (uv is IEventMachine machine)
                            return new GUIContent(GetMachineName(machine), icon);
                        else if (uv is IMacro macro)
                            return new GUIContent(GetMacroName(macro), icon);
                        else if (!string.IsNullOrEmpty(uv.name))
                            return new GUIContent(uv.name, icon);
                    }

                    if (mv is string ms && !string.IsNullOrEmpty(ms))
                        return new GUIContent(ms, icon);
                }
            }

            if (valueAsFallback && value.GetType().IsBasic())
            {
                return new GUIContent(value.ToString(), value.GetType().Icon()?[IconSize.Small]);
            }

            return new GUIContent($"Value {index + 1}", value.GetType().Icon()?[IconSize.Small]);
        }

        public static string GetMachineName(IEventMachine machine)
        {
            if (!string.IsNullOrEmpty(machine?.GetReference()?.graph?.title))
            {
                return machine.GetReference().graph.title;
            }

            if (machine.nest.source == GraphSource.Macro && machine.nest.macro is UnityEngine.Object @object)
            {
                if (!string.IsNullOrEmpty(@object.name)) return $"{@object.name}";
            }

            return "Unnamed Machine";
        }

        public static string GetMacroName(IMacro macro)
        {
            if (!string.IsNullOrEmpty(macro?.GetReference()?.graph?.title))
            {
                return macro.GetReference().graph.title;
            }

            if (macro is UnityEngine.Object @object)
            {
                return @object.name;
            }

            return "Unnamed Macro";
        }
    }
}