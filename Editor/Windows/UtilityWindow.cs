using System.Collections;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public class UtilityWindow : EditorWindow
    {
        private BorderedRectangle container;

        public static Event e;
        public FlowGraphContext graphContext;

        public static UtilityWindow Open()
        {
            var window = CreateInstance<UtilityWindow>();
            window.titleContent = new GUIContent("UVS Community Utilities");

            var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            window.position = new Rect(mousePosition.x, mousePosition.y, 250, 300);

            window.ShowPopup();
            return window;
        }

        private void OnLostFocus()
        {
            Close();
        }
        
        private void OnGUI()
        {
            e = Event.current;
        }

        private void OnEnable()
        {
            var root = rootVisualElement;
            root.Clear();
            container = new BorderedRectangle(HUMEditorColor.DefaultEditorBackground, Color.black, 2);
            container.style.flexDirection = FlexDirection.Column;
            container.style.flexGrow = 1;

            container.Add(SelectionToSuperUnit());
            container.Add(GenerateCode());
            container.Add(GenerateSelectedCode());
            container.Add(QuickAccess());
            container.Add(RegenerateNodesButton());
            container.Add(NodeFinderButton());

            minSize = new Vector2(250, minSize.y);

            root.Add(container);
        }

        private VisualElement SelectionToSuperUnit()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            var header = new BorderedRectangle(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2);
            header.style.height = 24;
            header.style.marginBottom = 4;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;

            var label = new Label
            {
                text = "Selection To Subgraph"
            };
            label.style.flexGrow = 1;

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.height = 24;

            var macroButton = new Button(() => { NodeSelection.Convert(GraphSource.Macro); }) { text = "To Macro" };
            macroButton.style.flexGrow = 1;

            var embedButton = new Button(() => { NodeSelection.Convert(GraphSource.Embed); }) { text = "To Embed" };
            embedButton.style.flexGrow = 1;

            buttonContainer.Add(macroButton);
            buttonContainer.Add(embedButton);

            header.Add(label);
            container.Add(header);
            container.Add(buttonContainer);
            return container;
        }

        private VisualElement GenerateCode()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            var header = new BorderedRectangle(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2);
            header.style.height = 24;
            header.style.marginBottom = 4;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginTop = 6;

            var label = new Label
            {
                text = "Asset Compilation"
            };
            label.style.flexGrow = 1;

            var hint = new HelpBox("Generate C# scripts for Defined Events, Funcs, and Actions for complete AOT Safety on all platforms.", HelpBoxMessageType.Info);
            hint.Set().Padding(6);

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.height = 24;

            var compileButton = new Button(() => { AssetCompiler.Compile(); }) { text = "Compile All" };
            compileButton.style.maxWidth = buttonContainer.contentRect.width;
            compileButton.style.flexGrow = 1;

            buttonContainer.Add(compileButton);

            header.Add(label);
            container.Add(header);
            container.Add(hint);
            container.Add(buttonContainer);
            return container;
        }

        private VisualElement GenerateSelectedCode()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.height = 24;

            var compileSelectedButton = new Button(() =>
            {
                var root = graphContext.reference.macro as Object;
                if (root != null)
                {
                    if (root is MethodDeclaration methodDeclaration)
                    {
                        root = methodDeclaration.parentAsset;
                    }
                    else if (root is ConstructorDeclaration constructorDeclaration)
                    {
                        root = constructorDeclaration.parentAsset;
                    }
                    else if (root is FieldDeclaration fieldDeclaration)
                    {
                        root = fieldDeclaration.parentAsset;
                    }
                    if (root != null)
                        AssetCompiler.CompileAsset(root);
                }
            })
            { text = "Compile Asset" };
            compileSelectedButton.style.maxWidth = buttonContainer.contentRect.width;
            compileSelectedButton.style.flexGrow = 1;

            buttonContainer.Add(compileSelectedButton);
            container.Add(buttonContainer);
            return container;
        }

        private VisualElement QuickAccess()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            var header = new BorderedRectangle(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2);
            header.style.height = 24;
            header.style.marginBottom = 4;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginTop = 6;

            var label = new Label();
            label.text = "Quick Access";
            label.style.flexGrow = 1;

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.height = 24;

            var csPreviewButton = new Button(() => { CSharp.CSharpPreviewWindow.Open(); }) { text = "Open C# Preview Window" };
            csPreviewButton.style.flexGrow = 1;

            buttonContainer.Add(csPreviewButton);

            header.Add(label);
            container.Add(header);
            container.Add(buttonContainer);
            return container;
        }

        private VisualElement RegenerateNodesButton()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.height = 24;

            var regenerateButton = new Button(() => { UnitBase.Rebuild(); }) { text = "Regenerate Nodes" };
            regenerateButton.style.flexGrow = 1;

            buttonContainer.Add(regenerateButton);

            container.Add(buttonContainer);
            return container;
        }

        private VisualElement NodeFinderButton()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.height = 24;

            var nodeFinderWindowButton = new Button(() => { NodeFinderWindow.Open(); }) { text = "Node Finder" };
            nodeFinderWindowButton.style.flexGrow = 1;

            buttonContainer.Add(nodeFinderWindowButton);

            container.Add(buttonContainer);
            return container;
        }
    }
}