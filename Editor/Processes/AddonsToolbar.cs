using System;
using UnityEditor;
using UnityEngine.UIElements;
using Bolt.Addons.Community.Utility.Editor;
using UnityEngine;
using Bolt.Addons.Libraries.Humility;

namespace Bolt.Addons.Community.Processing
{
    [Serializable]
    public sealed class AddonsToolbar : EditorWindow
    {
        public static AddonsToolbar active;

        public static void Open()
        {
            AddonsToolbar toolbar = null;

            if (toolbar == null)
            {
                if (active == null)
                {
                    var toolbars = Resources.FindObjectsOfTypeAll<AddonsToolbar>();

                    if (toolbars.Length > 0)
                    {
                        toolbar = toolbars[0];
                    }
                    else
                    {
                        toolbar = CreateInstance<AddonsToolbar>();
                        toolbar.ShowPopup();
                    }
                }
            }

            active = toolbar;
        }

        private void OnEnable()
        {
            var root = rootVisualElement;

            var compileButton = new Button(() =>
            {
                AssetCompiler.Compile();
            });

            compileButton.style.width = 32;
            compileButton.style.height = 32;
            compileButton.style.alignItems = Align.Center;

            var compileIcon = new Image() { image = PathUtil.Load("compiler", CommunityEditorPath.Fundamentals).Single() };
            compileIcon.style.width = 16;
            compileIcon.style.height = 16;

            compileButton.Add(compileIcon);
        }

        private void OnGUI()
        {
            position = new Rect(HUMEditorTypes.Get().GUIView().Position().position, new Vector2(96, 32));
            Debug.Log(position);
            Repaint();
        }
    }
}