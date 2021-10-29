using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [IncludeInSettings(true)]
    [Serializable]
    public sealed class EditorWindowView : EditorWindow, IMacro
    {
        public EditorWindowAsset asset;
        VisualElement header;

        [SerializeField]
        public bool assetWasCopied;

        [MenuItem("Window/Community Addons/New Editor Window View")]
        public static void Open()
        {
            EditorWindowView window = CreateInstance<EditorWindowView>();
            window.position = new Rect(HUMEditor.ScreenCenter(new Vector2(400, 400)).position, new Vector2(400, 400));
            window.showReferencePicker = true;
            window.titleContent = new GUIContent("Editor Window View");
            window.Show();
        }

        public static EditorWindowView CreateWindow(EditorWindowAsset windowType, bool showReferencePicker = false)
        {
            EditorWindowView window = CreateInstance<EditorWindowView>();
            window.position = new Rect(HUMEditor.ScreenCenter(new Vector2(400, 400)).position, new Vector2(400, 400));
            window.asset = windowType;
            window.showReferencePicker = showReferencePicker;
            window.titleContent = new GUIContent("Editor Window View");
            return window;
        }

        [SerializeField]
        private bool showReferencePicker;

        public IGraph childGraph => asset == null ? graph : asset.graph;

        public bool isSerializationRoot => true;

        public UnityEngine.Object serializedObject => asset;

#if VISUAL_SCRIPTING_1_7_5_OR_GREATER
        public IEnumerable<object> GetAotStubs(HashSet<object> visited)
        {
            return visited;
        } 
#else
        public IEnumerable<object> aotStubs => graph.aotStubs;
#endif

        [SerializeReference]
        private IGraph _graph = new FlowGraph();
        public IGraph graph { get => asset == null ? _graph : asset.graph; set { if (asset == null) { _graph = value; } else { asset.graph = (FlowGraph)value; } } }

        private GraphReference reference;

        [SerializeField] 
        private bool isAssetReference;

        [Inspectable][SerializeField]
        public CustomVariables variables = new CustomVariables();

        public VisualElement container;

        public Event e { get; private set; }

        private bool firstPass = true;

        private void OnHeaderGUI() 
        {
            e = Event.current;

            if (showReferencePicker)
            {
                HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(2, 2, 2, 2), new RectOffset(2, 2, 2, 2), () =>
                {
                    HUMEditor.Disabled(asset == null, () =>
                    {
                        if (GUILayout.Button("Variables", GUILayout.Width(100)))
                        {
                            EditorWindowVariables.Open(new Rect(GUIUtility.GUIToScreenPoint(new Vector2(e.mousePosition.x, e.mousePosition.y)), new Vector2(220, 300)), asset, this);
                        }

                        if (GUILayout.Button("Edit Graph", GUILayout.Width(120)))
                        {
                            GraphWindow.OpenActive(GetReference() as GraphReference);
                        }
                    });

                    HUMEditor.Changed(() =>
                    {
                        asset = (EditorWindowAsset)EditorGUILayout.ObjectField(asset, typeof(EditorWindowAsset), false);
                    }, () =>
                    {
                        if (asset == null)
                        {
                            variables.Clear();
                        }
                        else
                        {
                            if (!firstPass) variables.Clear();
                            variables.CopyFrom(asset.variables, true);
                        }
                    });

                    firstPass = false;
                }, GUILayout.Height(24));
            }
        }

        private void OnContainerGUI()
        {
            if (showReferencePicker && header != null && !rootVisualElement.Contains(header)) rootVisualElement.Add(header);
            if (!showReferencePicker && header != null && rootVisualElement.Contains(header)) rootVisualElement.Remove(header);

            if (asset != null)
            {
                if (!assetWasCopied)
                {
                    variables.CopyFrom(asset.variables);
                    assetWasCopied = true;
                }

                GetReference()?.AsReference()?.TriggerEventHandler<EditorWindowEventArgs>((hook) => { return hook == "EditorWindow_OnGUI"; }, new EditorWindowEventArgs(this), (p) => true, true);
            }
        }

        public GraphPointer GetReference()
        {
            return GraphReference.New(asset, true);
        }

        public IGraph DefaultGraph()
        {
            return new FlowGraph();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }

        private void OnDestroy()
        {
            variables.onVariablesChanged -= OnVariablesChanged;
            if (asset != null)
            {
                if (!assetWasCopied)
                {
                    variables.CopyFrom(asset.variables);
                    assetWasCopied = true;
                }

                GetReference().AsReference().TriggerEventHandler<EditorWindowEventArgs>((hook) => { return hook == "EditorWindow_OnDestroy"; }, new EditorWindowEventArgs(this), (p) => true, true);
            }
        }
        
        private void OnDisable()
        {
            variables.onVariablesChanged -= OnVariablesChanged;

            if (asset != null)
            {
                if (!assetWasCopied)
                {
                    variables.CopyFrom(asset.variables);
                    assetWasCopied = true;
                }

                GetReference().AsReference().TriggerEventHandler<EditorWindowEventArgs>((hook) => { return hook == "EditorWindow_OnDisable"; }, new EditorWindowEventArgs(this), (p) => true, true);
            }

            var windows = Resources.FindObjectsOfTypeAll<EditorWindowVariables>();
            for (int i = 0; i < windows.Length; i++)
            {
                windows[i].Close();
                UnityEngine.Object.DestroyImmediate(windows[i]);
            } 
        }

        private void OnEnable()
        {
            header = new IMGUIContainer(OnHeaderGUI);
            header.style.height = 27;
            container = new IMGUIContainer(OnContainerGUI);
            container.style.flexGrow = 1;

            rootVisualElement.Add(header);
            rootVisualElement.Add(container);

            if (asset != null) { variables.CopyFrom(asset.variables); assetWasCopied = true; }

            variables.onVariablesChanged += OnVariablesChanged;

            if (asset != null)
            {
                if (!assetWasCopied)
                {
                    variables.CopyFrom(asset.variables);
                    assetWasCopied = true;
                }

                GetReference().AsReference().TriggerEventHandler<EditorWindowEventArgs>((hook) => { return hook == "EditorWindow_OnEnable"; }, new EditorWindowEventArgs(this), (p) => true, true);
            }
        }

        private void OnFocus()
        {
            if (asset != null)
            {
                if (!assetWasCopied)
                {
                    variables.CopyFrom(asset.variables);
                    assetWasCopied = true;
                }

                GetReference().AsReference().TriggerEventHandler<EditorWindowEventArgs>((hook) => { return hook == "EditorWindow_OnFocus"; }, new EditorWindowEventArgs(this), (p) => true, true);
            }
        }

        private void OnLostFocus()
        {
            if (asset != null)
            {
                if (!assetWasCopied)
                {
                    variables.CopyFrom(asset.variables);
                    assetWasCopied = true;
                }

                GetReference().AsReference().TriggerEventHandler<EditorWindowEventArgs>((hook) => { return hook == "EditorWindow_OnLostFocus"; }, new EditorWindowEventArgs(this), (p) => true, true);
            }
        }

        private void OnVariablesChanged() 
        {
            if (asset != null)
            {
                variables.CopyFrom(asset.variables);
            }
            else
            {
                variables.Clear();
            }
        }
    }
}