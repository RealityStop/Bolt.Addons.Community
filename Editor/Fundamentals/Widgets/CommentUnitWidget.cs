using UnityEngine;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    static class Extension
    {
        public static Rect Offset(this Rect rect, float x = 0, float y = 0, float xy = 0, float w = 0, float h = 0, float wh = 0, bool centre = false)
        {
            rect.x += x + xy;
            rect.y += y + xy;
            rect.width += centre ? -(x + xy) * 2f : w + wh;
            rect.height += centre ? -(x + xy) * 2f : h + wh;

            return rect;
        }
    }

    [Widget(typeof(CommentNode))]
    public sealed class CommentNodeWidget : UnitWidget<CommentNode>
    {
        public CommentNodeWidget(FlowCanvas canvas, CommentNode unit) : base(canvas, unit) { }

        const float
            borderOutside = 12f,
            borderInside = 1.5f,
            borderText = 3f,
            borderTotal = borderOutside * 2f + borderInside * 2f + borderText * 2f;

        Rect
            wholeRect,
            borderRect,
            textRect;

        Vector2
            textAreaSize;

        int hash;

        GUIStyle
            textGUI = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true },
            titleGUI = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, alignment = TextAnchor.MiddleLeft, fontSize = 8 };

        public override Rect position
        {
            get => wholeRect; set => wholeRect = value;
        }

        public override void DrawBackground()
        {
            if (hash == 0) hash = unit.GetHashCode();

            // If first time running, create a palette.
            if (!CommentNodeInspector.initialised) { CommentNodeInspector.UpdatePalette(); CommentNodeInspector.initialised = true; }

            // If unit locked to palette, grab the assigned color
            if (unit.lockedToPalette)
            {
                unit.color = CommentNodeInspector.colorPalette[unit.customPalette ? 1 : 0, unit.paletteSelection.row, unit.paletteSelection.col] / 3f;
                unit.fontColor = CommentNodeInspector.fontPalette[(unit.customPalette ? 2 : unit.fontColorize ? 1 : 0), unit.paletteSelection.row, unit.paletteSelection.col];
            }

            // Set text area GUI style
            textGUI.fontStyle = (unit.fontBold && unit.fontItalic ? FontStyle.BoldAndItalic : unit.fontBold ? FontStyle.Bold : unit.fontItalic ? FontStyle.Italic : FontStyle.Normal);
            textGUI.fontSize = unit.fontSize;
            textGUI.alignment = unit.alignCentre ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;

            // Get text area xy size
            textAreaSize = textGUI.CalcSizeWithConstraints(new GUIContent(unit.comment), new Vector2(Mathf.Round(unit.maxWidth - borderTotal), 1000f));
            textAreaSize.y = textGUI.CalcHeight(new GUIContent(unit.comment), unit.maxWidth - borderTotal);

            // Set whole area rect
            wholeRect = new Rect(
                unit.position.x,
                unit.position.y,
                Mathf.Clamp(
                    (textAreaSize.x + borderTotal),
                    (unit.autoWidth ? borderTotal : unit.maxWidth),
                    unit.maxWidth),
                Mathf.Clamp(
                    textAreaSize.y + borderTotal,
                    borderTotal,
                    1000));

            // Resource - https://unitylist.com/p/5c3/Unity-editor-icons

            // Draw border if mouse present
            if (wholeRect.Contains(e.mousePosition) || selection.Contains(unit))
            {
                GUI.DrawTexture(wholeRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color * new Color(0.5f, 0.5f, 0.5f, 0.5f), 0, borderOutside);
            }

            // Get inner area rect
            borderRect = wholeRect.Offset(xy: borderOutside, centre: true);

            // Draw border
            GUI.DrawTexture(borderRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color, 0, 7);
            GUI.DrawTexture(borderRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color * (2f - unit.color.grayscale), borderInside, 7);

        }

        public override void DrawForeground()
        {
            // Get text area rect
            textRect = borderRect.Offset(xy: borderText, centre: true);

            // If mouse hovering over unit
            if (textRect.Contains(e.mousePosition))
            {
                GUI.SetNextControlName("commentField" + hash.ToString());
                unit.comment = EditorGUI.TextArea(textRect, unit.comment, textGUI);
            }
            // If unit text selected
            else if (GUI.GetNameOfFocusedControl() == "commentField" + hash.ToString())
            {
                unit.comment = EditorGUI.TextArea(textRect, unit.comment, textGUI);
                return;
            }

            // Draw outline?
            if (unit.hasOutline)
            {
                GUI.contentColor = unit.fontColor.maxColorComponent > 0.5f ? unit.color * 0.9f * (unit.color.maxColorComponent / 1f) : (unit.color * 0.9f * (1f / unit.color.maxColorComponent)).WithAlpha(1f);

                float outline = Mathf.Max(unit.fontSize / 60f, 1f);
                EditorGUI.LabelField(textRect.Offset(x: -outline, y: -outline), unit.comment, textGUI);
                EditorGUI.LabelField(textRect.Offset(x: outline, y: outline), unit.comment, textGUI);
                EditorGUI.LabelField(textRect.Offset(x: -outline, y: outline), unit.comment, textGUI);
                EditorGUI.LabelField(textRect.Offset(x: outline, y: -outline), unit.comment, textGUI);
            }

            // Draw main comment
            GUI.contentColor = unit.fontColor;
            EditorGUI.LabelField(textRect, unit.comment, textGUI);

            // Draw title
            GUI.contentColor = Color.white;
            if (unit.hasTitle)
                EditorGUI.LabelField(new Rect(unit.position.x + borderOutside + 7f, unit.position.y, borderRect.width, borderOutside), unit.title, titleGUI);

            unit.position = wholeRect.position;
        }
    }
}