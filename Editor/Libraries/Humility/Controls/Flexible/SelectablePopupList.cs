using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public abstract class SelectablePopupList<T> : EditorWindow
    {
        private List<T> list;
        private Vector2 scrollPosition;

        public static TSelectable Open<TSelectable>(Vector2 position, List<T> list) where TSelectable : SelectablePopupList<T>
        {
            TSelectable popup = CreateInstance<TSelectable>();
            popup.list = list;
            popup.position = new Rect(position, popup.GetSize());
            popup.ShowPopup();
            return popup;
        }

        protected abstract Vector2 GetSize();

        private void OnLostFocus()
        {
            Close();
        }

        private void OnDisable()
        {
            Close();
        }

        private void OnGUI()
        {
            scrollPosition = HUMEditor.Draw().ScrollView(scrollPosition, new GUIStyle(GUI.skin.scrollView) { stretchHeight = true, stretchWidth = true }, () =>
            {
                HUMEditor.Vertical(() =>
                {
                    for (int i = 0; i < list?.Count; i++)
                    {
                        OnDrawItem(list[i], Event.current);
                    }
                });
            });
        }

        protected abstract void OnDrawItem(T item, Event e);
    }
}
