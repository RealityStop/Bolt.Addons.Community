using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(Arrow))]
    public class ArrowInspector : Inspector
    {
        private GUIStyle headerStyle;
        private GUIStyle largeButtonStyle;

        public ArrowInspector(Metadata metadata) : base(metadata)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.fontSize = 14;

            largeButtonStyle = new GUIStyle(GUI.skin.button);
            largeButtonStyle.fontSize = 18;
            largeButtonStyle.alignment = TextAnchor.MiddleCenter;
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 18f;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            position = BeginLabeledBlock(metadata, position, GUIContent.none);

            Arrow arrow = (Arrow)metadata.value;
            var height = position.y;
            void IncreaseHeight()
            {
                height += EditorGUIUtility.singleLineHeight * 1.5f;
            }
            Rect arrowColorRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ArrowColor = EditorGUI.ColorField(arrowColorRect, new GUIContent("Arrow Color"), arrow.ArrowColor);
            IncreaseHeight();
            Rect lineColorRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.Color = EditorGUI.ColorField(lineColorRect, new GUIContent("Line Color"), arrow.Color);
            IncreaseHeight();
            Rect lengthRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.Length = Mathf.Clamp(EditorGUI.FloatField(lengthRect, new GUIContent("Length"), arrow.Length), 0f, 1000f);
            IncreaseHeight();
            Rect rotationRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            float newRotationAngle = EditorGUI.FloatField(rotationRect, new GUIContent("Rotation Angle"), arrow.rotationAngle);
            if (EditorGUI.EndChangeCheck())
            {
                metadata.RecordUndo();
                arrow.rotationAngle = Mathf.Repeat(newRotationAngle, 360f);
            }
            IncreaseHeight();
            Rect textRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.Text = EditorGUI.TextField(textRect, new GUIContent("Text"), arrow.Text);
            IncreaseHeight();
            Rect showSquareRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ShowSquare = EditorGUI.Toggle(showSquareRect, new GUIContent("Show Point", "Shows the point of the arrow that is interactable"), arrow.ShowSquare);
            IncreaseHeight();
            Rect showBottomArrowRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ShowBottomArrow = EditorGUI.Toggle(showBottomArrowRect, new GUIContent("Show Bottom Arrow"), arrow.ShowBottomArrow);
            IncreaseHeight();
            Rect showTopArrowRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ShowTopArrow = EditorGUI.Toggle(showTopArrowRect, new GUIContent("Show Top Arrow"), arrow.ShowTopArrow);
            IncreaseHeight();
            Rect buttonsRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            DrawRotationButtons(buttonsRect, arrow);
            IncreaseHeight();
            Rect LineTypeRect = new Rect(position.x, height, position.width, EditorGUIUtility.singleLineHeight);
            arrow.lineType = (LineType)EditorGUI.EnumPopup(LineTypeRect, new GUIContent("Line Type"), arrow.lineType);
            IncreaseHeight();
            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
            }
        }

        private void DrawRotationButtons(Rect position, Arrow arrow)
        {
            float buttonWidth = position.width / 3f;
            Rect buttonRect = new Rect(position.x, position.y, buttonWidth, position.height);

            if (GUI.Button(buttonRect, "<--"))
            {
                metadata.RecordUndo();
                arrow.rotationAngle -= 15f;
                arrow.rotationAngle = Mathf.Repeat(arrow.rotationAngle, 360f);
            }

            buttonRect.x += buttonWidth;

            if (GUI.Button(buttonRect, "Reset"))
            {
                metadata.RecordUndo();
                arrow.rotationAngle = 0f;
            }

            buttonRect.x += buttonWidth;

            if (GUI.Button(buttonRect, "-->"))
            {
                metadata.RecordUndo();
                arrow.rotationAngle += 15f;
                arrow.rotationAngle = Mathf.Repeat(arrow.rotationAngle, 360f);
            }
        }
    }
}
