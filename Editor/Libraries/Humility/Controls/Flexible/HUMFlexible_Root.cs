#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor
    {
        private static Texture2D _foldoutOpenIcon;
        private static Texture2D foldoutOpenIcon
        {
            get => _foldoutOpenIcon;
            set => _foldoutOpenIcon = _foldoutOpenIcon ?? AssetDatabase.LoadAssetAtPath<Texture2D>(HUMIO.PathOf("humility_root") + "/Editor/Resources/Icons/foldout_open.png");
        }

        private static Texture2D _foldoutClosedIcon;
        private static Texture2D foldoutClosedIcon
        {
            get => _foldoutClosedIcon;
            set => _foldoutClosedIcon = _foldoutClosedIcon ?? AssetDatabase.LoadAssetAtPath<Texture2D>(HUMIO.PathOf("humility_root") + "/Editor/Resources/Icons/foldout_open.png");
        }

        public static Data.Flexible Draw()
        {
            return new Data.Flexible();
        }

        public static void Horizontal(Action action, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            action();
            EditorGUILayout.EndHorizontal();
        }

        public static Data.Horizontal Horizontal()
        {
            return new Data.Horizontal();
        }

        public static void Horizontal(GUIStyle style, Action action, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
            action();
            EditorGUILayout.EndHorizontal();
        }

        public static Data.Vertical Vertical()
        {
            return new Data.Vertical();
        }

        public static void Vertical(Action action, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            action();
            EditorGUILayout.EndVertical();
        }

        public static void Vertical(GUIStyle style, Action action, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
            action();
            EditorGUILayout.EndVertical();
        }

        public static bool Foldout(bool isOpen, GUIContent label, Color backgroundColor, Color borderColor, int border = 1, Action whileOpen = null)
        {
            var foldout = false;
            Vertical().Box(backgroundColor, borderColor, new RectOffset(2, 2, 2, 2), TextAnchor.MiddleLeft, border, () =>
            {
                Horizontal(() =>
                {
                    foldout = EditorGUILayout.Toggle(isOpen, new GUIStyle(EditorStyles.foldout), GUILayout.Width(16), GUILayout.Width(16));
                    GUILayout.Box(GUIContent.none, new GUIStyle() { normal = new GUIStyleState() { background = (Texture2D)label.image }, margin = new RectOffset(0, 0, 3, 0) }, GUILayout.Width(16), GUILayout.Height(16));
                    GUILayout.Label(label.text);
                });
            });

            if (!foldout)
            {
                return false;
            }

            whileOpen();

            return true;
        }

        public static bool Foldout(bool isOpen, GUIContent label, Color backgroundColor, Color borderColor, RectOffset border, Action whileOpen = null)
        {
            var foldout = false;
            Vertical().Box(backgroundColor, borderColor, new RectOffset(2, 2, 2, 2), TextAnchor.MiddleLeft, border, () =>
            {
                Horizontal(() =>
                {
                    foldout = EditorGUILayout.Toggle(isOpen, new GUIStyle(EditorStyles.foldout), GUILayout.Width(16));
                    GUILayout.Box(GUIContent.none, new GUIStyle() { normal = new GUIStyleState() { background = (Texture2D)label.image }, margin = new RectOffset(0, 0, 3, 0) }, GUILayout.Width(16), GUILayout.Height(16));
                    GUILayout.Label(label.text);
                });
            });

            if (!foldout)
            {
                return false;
            }

            whileOpen();

            return true;
        }


        public static bool Foldout(bool isOpen, Color backgroundColor, Color borderColor, int border = 1, Action header = null, Action whileOpen = null)
        {
            return Foldout(isOpen, backgroundColor, borderColor, new RectOffset(border, border, border, border), header, whileOpen);
        }

        public static bool Foldout(bool isOpen, Color backgroundColor, Color borderColor, RectOffset border, Action header = null, Action whileOpen = null, params GUILayoutOption[] options)
        {
            var foldout = false;
            Vertical().Box(backgroundColor, borderColor, new RectOffset(2, 2, 2, 2), TextAnchor.MiddleLeft, border, () =>
            {
                Horizontal(() =>
                {
                    foldout = EditorGUILayout.Toggle(isOpen, new GUIStyle(EditorStyles.foldout), GUILayout.Width(16));
                    header?.Invoke();
                });
            }, options);

            if (!foldout)
            {
                return false;
            }

            whileOpen?.Invoke();

            return true;
        }

        public static bool Foldout(Rect positionClosed, Rect positionOpened, bool isOpen, Color backgroundColor, Color borderColor, int border = 1, Action header = null, Action whileOpen = null)
        {
            var foldout = false;
            if (isOpen) {
            }
            else {
                HUMEditor.Draw().Area(positionClosed, () =>
                {
                    Vertical().Box(backgroundColor, borderColor, new RectOffset(2, 2, 2, 2), TextAnchor.MiddleLeft, border, () =>
                    {
                        Horizontal(() =>
                        {
                            foldout = EditorGUILayout.Toggle(isOpen, new GUIStyle(EditorStyles.foldout), GUILayout.Width(16));
                            header?.Invoke();
                        });
                    });
                });
            }
           

            if (!foldout)
            {
                return false;
            }

            whileOpen?.Invoke();

            return true;
        }

        public static bool Foldout(bool isOpen, string label, Color backgroundColor, Color borderColor, int border = 1, Action whileOpen = null)
        {
            var foldout = false;
            Vertical().Box(backgroundColor, borderColor, new RectOffset(20, 0, 0, 0), TextAnchor.MiddleLeft, border, () =>
            {
                Horizontal(() =>
                {
                    foldout = EditorGUILayout.Toggle(isOpen, new GUIStyle(EditorStyles.foldout), GUILayout.Width(16));
                    GUILayout.Label(label);
                });
            });

            if (!foldout)
            {
                return false;
            }

            whileOpen?.Invoke();

            return true;
        }

        public static bool CenteredIconFoldout(bool opened, Color backgroundColor, Color borderColor, Texture2D openedIcon, Texture2D closedIcon, Action whileOpen)
        {
            var _opened = opened;

            HUMEditor.Vertical().Box(backgroundColor, borderColor, new RectOffset(), new RectOffset(1, 1, 0, 0), () =>
            {
                HUMEditor.Horizontal(() =>
                {
                    if (_opened)
                    {
                        if (GUILayout.Button(GUIContent.none, new GUIStyle(), GUILayout.Height(16)))
                        {
                            _opened = !_opened;
                        }
                    }

                    if (GUILayout.Button(GUIContent.none, new GUIStyle() { normal = new GUIStyleState() { background = opened ? openedIcon : closedIcon } }, GUILayout.Width(16), GUILayout.Height(16)))
                    {
                        _opened = !_opened;
                    }

                    if (_opened)
                    {
                        if (GUILayout.Button(GUIContent.none, new GUIStyle(), GUILayout.Height(16)))
                        {
                            _opened = !_opened;
                        }
                    }
                });
            }, true, false, GUILayout.Height(16));


            HUMEditor.Vertical().Box(backgroundColor.Darken(0.05f), borderColor, new RectOffset(), new RectOffset(1, 1, 0, 0), () =>
            {
                if (opened)
                {
                    whileOpen?.Invoke();
                }
            }, true, true);
            return _opened;
        }

        public static void Image(Texture2D texture, int width, int height)
        {
            GUILayout.Box(GUIContent.none, new GUIStyle() { normal = new GUIStyleState() { background = texture }, margin = new RectOffset(0, 0, 3, 0) }, GUILayout.Width(width), GUILayout.Height(height));
        }

        public static void Image(Texture2D texture, int width, int height, RectOffset padding, RectOffset margin)
        {
            GUILayout.Box(GUIContent.none, new GUIStyle() { normal = new GUIStyleState() { background = texture }, padding = padding, margin = margin }, GUILayout.Width(width), GUILayout.Height(height));
        }

        public static void ImageButton(Texture2D normal, Texture2D hover, Texture2D pressed, int width, int height, Action onClicked)
        {
            if (GUILayout.Button(GUIContent.none, new GUIStyle()
            {
                normal = new GUIStyleState() { background = normal },
                hover = new GUIStyleState() { background = hover },
                active = new GUIStyleState() { background = pressed },
                margin = new RectOffset(0, 0, 4, 0)
            }, GUILayout.Width(width), GUILayout.Height(height)))
            {
                onClicked?.Invoke();
            }

        }

        public static void LostFocus(ref string focusedControl, string controlName, Action onControl, Action onLostFocus)
        {
            GUI.SetNextControlName(controlName);
            onControl?.Invoke();
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                if (focusedControl != controlName)
                {
                    focusedControl = controlName;
                }
            }
            else
            {
                if (focusedControl == controlName)
                {
                    onLostFocus?.Invoke();
                    focusedControl = string.Empty;
                }
            }
        }

        public static void Focus(string controlName, Action onFocused, Action onNotFocused)
        {
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                onFocused?.Invoke();
            }
            else
            {
                onNotFocused?.Invoke();
            }
        }

        public static T SerializedValue<T>(this SerializedProperty prop) where T : class
        {
            return prop.serializedObject.targetObject as T;
        }
    }
}
#endif