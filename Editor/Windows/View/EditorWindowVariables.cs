using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public sealed class EditorWindowVariables : EditorWindow
    {
        [SerializeField]
        private EditorWindowAsset asset;
        private SerializedObject serializedObject;
        private SerializedObject view;
        private Metadata variablesMetadata;
        private Metadata windowVariablesMetadata;

        private Vector2 scrollPosition;

        private bool focused;
        private bool cached;
        private bool locked;
        private bool isInstance, isDefinition;

        public static void Open(Rect position, EditorWindowAsset asset, EditorWindowView view)
        {
            EditorWindowVariables window = CreateInstance<EditorWindowVariables>();
            window.position = position;
            window.asset = asset; 
            window.serializedObject = new SerializedObject(asset);
            window.variablesMetadata = Metadata.FromProperty(window.serializedObject?.FindProperty("variables"));
            window.view = new SerializedObject(view);
            window.windowVariablesMetadata = Metadata.FromProperty(window.view.FindProperty("variables"));
            window.ShowPopup();
        } 

        private void OnFocus()
        {
            focused = true; 
        }

        private void OnLostFocus()
        {
            focused = false; 
        }

        private void OnGUI()
        {
            view?.Update();

            if (!cached)
            {
                if (!isInstance && !isDefinition) isInstance = true;
                cached = true;
            }

            if (!locked && (!focused && FuzzyWindow.instance == null || !focused && focusedWindow != FuzzyWindow.instance)) Close();

            HUMEditor.Draw(new Rect(new Vector2(0,0), position.size)).Box(HUMEditorColor.DefaultEditorBackground, Color.black, BorderDrawPlacement.Inside, 1);

            HUMEditor.Horizontal(() =>
            {
                var instance = isInstance;
                isInstance = EditorGUILayout.Toggle(isInstance, new GUIStyle(GUI.skin.button), GUILayout.Height(24));
                var lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, new GUIContent("Instance"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                if (isInstance != instance) isDefinition = false;

                var definition = isDefinition;
                isDefinition = EditorGUILayout.Toggle(isDefinition, new GUIStyle(GUI.skin.button), GUILayout.Height(24));
                lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, new GUIContent("Definition"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                if (isDefinition != definition) isInstance = false;

                locked = EditorGUILayout.Toggle(locked, new GUIStyle(GUI.skin.button), GUILayout.Width(48), GUILayout.Height(24));
                lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, new GUIContent("Locked"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
            });

            scrollPosition = HUMEditor.Draw().ScrollView(scrollPosition, ()=> 
            {
                if (isInstance) { LudiqGUI.InspectorLayout(windowVariablesMetadata); }
                if (isDefinition) { LudiqGUI.InspectorLayout(variablesMetadata); }
            });

            view?.ApplyModifiedProperties();

            if (locked) Repaint();
        }
    }
}