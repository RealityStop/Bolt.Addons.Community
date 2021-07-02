using System;
using System.Collections;
using System.Collections.Generic;

using Bolt;
using UnityEngine;
using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Unity.VisualScripting;
using Bolt.Addons.Libraries.Humility;
using UnityEditor;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Widget(typeof(CommentUnit))]
    public sealed class CommentUnitWidget : UnitWidget<CommentUnit>
    {
        public CommentUnitWidget(FlowCanvas canvas, CommentUnit unit) : base(canvas, unit)
        {
        }

        private Rect fullPosition;
        private Rect borderPosition;
        private Rect containerPosition;
        private int hash;

        public override Rect position
        {
            get => fullPosition; set => fullPosition = value;
        }
        
        public override void DrawBackground()
        {
            if (hash == 0) hash = unit.GetHashCode();
            
            fullPosition = new Rect(unit.position.x, unit.position.y, Mathf.Clamp(unit.width+36, 120, 500), Mathf.Clamp(GUI.skin.textArea.CalcHeight(new GUIContent(unit.comment + "\n"), unit.width) + 40, 30, 1000));

            if (fullPosition.Contains(e.mousePosition) || selection.Contains(unit))
            {
                HUMEditor.Draw(fullPosition).Box(unit.color.Darken(0.15f), unit.color.Darken(0.15f), BorderDrawPlacement.Outside, 0);
            }

            borderPosition = fullPosition.Subtract().Width(30).Subtract().Height(30).Add().X(15).Add().Y(15);
            HUMEditor.Draw(borderPosition).Box(unit.color.Darken(0.05f), unit.color, BorderDrawPlacement.Inside, 3);
        }

        public override void DrawForeground()
        {
            containerPosition = borderPosition.Subtract().Width(6).Subtract().Height(6).Add().X(3).Add().Y(3);

            if (containerPosition.Contains(e.mousePosition))
            {
                GUI.SetNextControlName("commentField"+ hash.ToString());
                unit.comment = EditorGUI.TextArea(containerPosition.Subtract().Width(2).Subtract().Height(2).Add().X(1).Add().Y(1), unit.comment, new GUIStyle(GUI.skin.textArea) { wordWrap = true, padding = new RectOffset(6, 6, 6, 6) });
            }
            else
            {
                if (GUI.GetNameOfFocusedControl() == "commentField" + hash.ToString())
                {
                    unit.comment = EditorGUI.TextArea(containerPosition.Subtract().Width(2).Subtract().Height(2).Add().X(1).Add().Y(1), unit.comment, new GUIStyle(GUI.skin.textArea) { wordWrap = true, padding = new RectOffset(6,6,6,6) });
                    return;
                }

                EditorGUI.LabelField(containerPosition.Subtract().Width(2).Subtract().Height(2).Add().X(1).Add().Y(1), unit.comment, new GUIStyle(GUI.skin.label) { wordWrap = true, alignment = TextAnchor.UpperLeft, padding = new RectOffset(6, 6, 6, 6) });
            }

            unit.position = new Vector2(fullPosition.x, fullPosition.y);
        }
    }
}