using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public sealed class CommentPopup : EditorWindow
    {
        public static CommentPopup active;
        private bool positionSet;
        private string text;
        Vector2 mousePosition;
        Vector2 placement;
        private bool focused;
        System.Action<CommentNode> onCreate = null;
        public static void Open(System.Action<CommentNode> onCreate = null)
        {
            CommentPopup popup = CommentPopup.CreateInstance<CommentPopup>();
            popup.onCreate = onCreate;
            var canvas = GraphWindow.activeContext?.canvas as FlowCanvas;
            popup.placement = canvas.mousePosition;
            active = popup;
            popup.ShowPopup();
        }

        private void OnGUI()
        {
            if (!positionSet)
            {
                mousePosition = Event.current.mousePosition.Add(new Vector2(10, 0));
                minSize = new Vector2(140, 24);
                maxSize = new Vector2(140, 24);
                positionSet = true;
            }

            position = new Rect(mousePosition, new Vector2(100, 24));

            HUMEditor.Horizontal(() =>
            {
                GUILayout.Label("//", GUILayout.Width(12));
                GUI.SetNextControlName("commentPopupField");
                text = GUILayout.TextField(text);
            });

            if (!focused) { EditorGUI.FocusTextInControl("commentPopupField"); focused = true; }

            if (Event.current.keyCode == KeyCode.Return && Event.current.rawType == EventType.KeyUp)
            {
                var canvas = GraphWindow.activeContext?.canvas as FlowCanvas;
                var node = new CommentNode() { comment = text, color = new Color(Random.Range(0.1f, 0.6f), Random.Range(0.1f, 0.6f), Random.Range(0.1f, 0.6f)) };
                onCreate?.Invoke(node);
                canvas?.AddUnit(node, placement);
                Close();
            }
            else
            {
                if (Event.current.keyCode == KeyCode.Escape && Event.current.rawType == EventType.KeyUp)
                {
                    Close();
                }
            }

            if (focusedWindow != this) Close();
        }

        private void OnDestroy()
        {
            active = null;
        }
    }
}