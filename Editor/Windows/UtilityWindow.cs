using System.Collections;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Bolt.Addons.Integrations.Continuum.Humility;

namespace Bolt.Addons.Community.Utility.Editor
{
    public class UtilityWindow : EditorWindow
    {
        [MenuItem("Window/Community Addons/Utilities")]
        public static void Open()
        {
            var window = GetWindow<UtilityWindow>();
            window.titleContent = new GUIContent("UVS Community Utilities");
        }

        private void OnEnable()
        {
            SelectionToSuperUnit();
            minSize = new Vector2(250, minSize.y);
        }

        private void SelectionToSuperUnit()
        {
            var root = rootVisualElement;

            var container = new BorderedRectangle(HUMEditorColor.DefaultEditorBackground, Color.black, 2);
            container.style.flexDirection = FlexDirection.Column;
            container.style.flexGrow = 1;

            var header = new BorderedRectangle(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2);
            header.style.height = 24;
            header.style.marginBottom = 4;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;

            var label = new Label();
            label.text = "Convert Unit Selection";
            label.style.flexGrow = 1;

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.height = 24;

            var macroButton = new Button(() => { UnitSelection.Convert(GraphSource.Macro); }) { text = "To Macro" };
            macroButton.style.flexGrow = 1;

            var embedButton = new Button(() => { UnitSelection.Convert(GraphSource.Embed); }) { text = "To Embed" };
            embedButton.style.flexGrow = 1;

            buttonContainer.Add(macroButton);
            buttonContainer.Add(embedButton);

            header.Add(label);
            container.Add(header);
            container.Add(buttonContainer);
            root.Add(container);
        }
    }
}