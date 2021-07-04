using UnityEngine;
using UnityEditor;
using Bolt.Addons.Libraries.Humility;
using System.Linq;
using Unity.VisualScripting;
using Bolt.Addons.Libraries.CSharp;
using System;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Serializable]
    public sealed class EditorWindowVariables : EditorWindow
    {
        private EditorWindowAsset asset;
        private SerializedObject serializedObject;
        private SerializedObject view;
        private Metadata variablesMetadata;
        private Metadata windowVariablesMetadata;

        [SerializeField]
        private Vector2 scrollPosition;

        private bool focused;
        private bool cached;

        [SerializeField]
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

            if (!focused && FuzzyWindow.instance == null || !focused && focusedWindow != FuzzyWindow.instance) Close();

            HUMEditor.Draw(new Rect(new Vector2(0,0), position.size)).Box(HUMEditorColor.DefaultEditorBackground, Color.black, BorderDrawPlacement.Inside, 1);

            HUMEditor.Horizontal(() =>
            {
                var definition = isDefinition;
                isDefinition = EditorGUILayout.Toggle(isDefinition, new GUIStyle(GUI.skin.button));
                var lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, new GUIContent("Definition"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                if (isDefinition != definition) isInstance = false;

                var instance = isInstance;
                isInstance = EditorGUILayout.Toggle(isInstance, new GUIStyle(GUI.skin.button));
                lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, new GUIContent("Instance"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                if (isInstance != instance) isDefinition = false;
            });
             
            scrollPosition = HUMEditor.Draw().ScrollView(scrollPosition, ()=> 
            {
                if (isInstance) { LudiqGUI.InspectorLayout(windowVariablesMetadata); }
                if (isDefinition) { LudiqGUI.InspectorLayout(variablesMetadata); }
            });

            view?.ApplyModifiedProperties();
        }
    }
}