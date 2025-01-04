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
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * 2);
            GUI.Label(headerRect, "Arrow Inspector", headerStyle);

            position.y += headerRect.height;
            position.height -= headerRect.height;

            position = BeginLabeledBlock(metadata, position, GUIContent.none);

            Arrow arrow = (Arrow)metadata.value;

            Rect arrowColorRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ArrowColor = EditorGUI.ColorField(arrowColorRect, new GUIContent("Arrow Color"), arrow.ArrowColor);

            Rect lineColorRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2.5f, position.width, EditorGUIUtility.singleLineHeight);
            arrow.Color = EditorGUI.ColorField(lineColorRect, new GUIContent("Line Color"), arrow.Color);

            Rect lengthRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 4f, position.width, EditorGUIUtility.singleLineHeight);
            arrow.Length = EditorGUI.FloatField(lengthRect, new GUIContent("Length"), arrow.Length);

            Rect rotationRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 5.5f, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            float newRotationAngle = EditorGUI.FloatField(rotationRect, new GUIContent("Rotation Angle"), arrow.rotationAngle);
            if (EditorGUI.EndChangeCheck())
            {
                metadata.RecordUndo();
                arrow.rotationAngle = Mathf.Repeat(newRotationAngle, 360f);
            }

            Rect textRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 7f, position.width, EditorGUIUtility.singleLineHeight);
            arrow.Text = EditorGUI.TextField(textRect, new GUIContent("Text"), arrow.Text);

            Rect showSquareRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 8.5f, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ShowSquare = EditorGUI.Toggle(showSquareRect, new GUIContent("Show Point", "Shows the point of the arrow that is interactable"), arrow.ShowSquare);

            Rect showBottomArrowRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 10f, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ShowBottomArrow = EditorGUI.Toggle(showBottomArrowRect, new GUIContent("Show Bottom Arrow"), arrow.ShowBottomArrow);

            Rect showTopArrowRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 11.5f, position.width, EditorGUIUtility.singleLineHeight);
            arrow.ShowTopArrow = EditorGUI.Toggle(showTopArrowRect, new GUIContent("Show Top Arrow"), arrow.ShowTopArrow);

            Rect buttonsRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 13f, position.width, EditorGUIUtility.singleLineHeight);
            DrawRotationButtons(buttonsRect, arrow);

            Rect LineTypeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 14.5f, position.width, EditorGUIUtility.singleLineHeight);
            arrow.lineType = (LineType)EditorGUI.EnumPopup(LineTypeRect, new GUIContent("Line Type"), arrow.lineType);

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
