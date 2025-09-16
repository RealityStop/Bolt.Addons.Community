using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community;
using Unity.VisualScripting;
using System.Text.RegularExpressions;

namespace Unity.VisualScripting.Community.CSharp
{
    public class CSharpPreviewWindow : EditorWindow
    {
<<<<<<< Updated upstream
        public static CSharpPreviewWindow instance;
        private float zoomFactor = 1.0f;
        public bool showCodeWindow = true;
        private List<(Label label, int)> labels = new List<(Label, int)>();
=======
        [SerializeField] private UnityEngine.Object _asset;
        private string manualCode;
        private Vector2 scrollPosition;
        private float visualZoom = 0.7f;
>>>>>>> Stashed changes

        private bool canCompile = true;

        private string[] codeLines;
        private Dictionary<int, List<ClickableRegion>> cachedRegions;

        private string searchQuery = "";
        private List<int> searchMatches = new();
        private int currentMatchIndex = 0;
        private Color highlightColor = new Color(1f, 0.8f, 0f, 0.1f);

        private const int LineHeight = 18;
        private const int BufferLines = 20;

        [MenuItem("Window/Community Addons/C# Preview")]
        public static void Open()
        {
<<<<<<< Updated upstream
            CSharpPreviewWindow window = GetWindow<CSharpPreviewWindow>();
            window.titleContent = new GUIContent("C# Preview");
            instance = window;
            window.minSize = new Vector2(400, 400);
        }

        public void CreateGUI()
        {
            instance = this;
            Selection.selectionChanged += () =>
            {
                if (Selection.activeGameObject != null)
                {
                    currentComponentIndex = 0;
                    UpdateComponentSelectionUI(Selection.activeGameObject);
                }
                else
                {
                    ClearComponentNavigationUI();
                }
                ChangeSelection();
            };

            toolbar = new Toolbar();
            toolbar.name = "Toolbar";

            var codeView = new VisualElement() { style = { flexDirection = FlexDirection.Row, flexGrow = 1 }, name = "codeView" };
            var codeContainer = new ScrollView
            {
                name = "codeContainer",
                style = {
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    flexGrow = 1,
                    minWidth = 100
                },
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                mode = ScrollViewMode.VerticalAndHorizontal
            };

            var settingsContainer = new ScrollView
            {
                name = "settingsContainer",
                style = { backgroundColor = new Color(0.18f, 0.18f, 0.18f), flexGrow = 1 }
            };
            var lineNumbersContainer = new ScrollView()
            {
                name = "lineNumbersContainer",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                style = {
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    flexDirection = FlexDirection.Column,
                    width = 50,
                    minWidth = 50
                }
            };

            codeContainer.verticalScroller.valueChanged += (newValue) =>
            {
                lineNumbersContainer.verticalScroller.value = newValue;
            };
            lineNumbersContainer.verticalScroller.valueChanged += (newValue) =>
            {
                codeContainer.verticalScroller.value = newValue;
            };
            codeView.Add(lineNumbersContainer);
            codeView.Add(codeContainer);
            CreateSettingsUI(settingsContainer);

            var zoomContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 10,
                    flexGrow = 0
                }
            };
            var zoomTextLabel = new Label("Zoom:") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 5 } };
            var zoomSlider = new Slider(0.5f, 1.0f)
            {
                value = zoomFactor,
                style = { width = Length.Percent(20), alignSelf = Align.Center }
            };
            var zoomLabel = new Label($"{GetDisplayZoom(zoomFactor):0.#}x") { style = { marginLeft = 5, alignSelf = Align.Center } };

            zoomSlider.RegisterValueChangedCallback(evt =>
            {
                zoomFactor = evt.newValue;
                zoomLabel.text = $"{GetDisplayZoom(zoomFactor):0.#}x";
                codeContainer.style.fontSize = Mathf.RoundToInt(14 * zoomFactor);
                lineNumbersContainer.style.fontSize = Mathf.RoundToInt(14 * zoomFactor);

                RefreshVirtualization(codeContainer, lineNumbersContainer);
            });

            static float GetDisplayZoom(float actualZoom) => 0.5f + (actualZoom - 0.5f) * 3f;

            zoomContainer.Add(zoomTextLabel);
            zoomContainer.Add(zoomSlider);
            zoomContainer.Add(zoomLabel);

            toolbar.Add(zoomContainer);

            var searchContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginLeft = 10,
                    flexShrink = 1,
                    flexGrow = 0,
                }
            };

            searchField = new TextField
            {
                style = {
                    flexGrow = 1,
                    flexShrink = 1
                }
            };
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue;
                PerformSearch();
            });

            var prevSearchButton = new Button(() => NavigateSearch(-1)) { text = "←" };
            var nextSearchButton = new Button(() => NavigateSearch(1)) { text = "→" };
            var searchCountLabel = new Label("0/0") { name = "searchCount" };

            searchContainer.Add(new Label("Search:") { style = { marginRight = 5 } });
            searchContainer.Add(searchField);
            searchContainer.Add(prevSearchButton);
            searchContainer.Add(nextSearchButton);
            searchContainer.Add(searchCountLabel);

            toolbar.Add(searchContainer);

            toolbar.Add(new VisualElement { style = { flexGrow = 1 } });

            var compileButton = UIBuilder.CreateToolbarButton("Compile", CompileCode, false, true);
            toolbar.Add(compileButton);

            var copyButton = UIBuilder.CreateToolbarButton("Copy to Clipboard", CopyToClipboard, false, true);
            toolbar.Add(copyButton);

            var refreshButton = UIBuilder.CreateToolbarButton("Refresh", UpdateCodeDisplay, true, true);
            toolbar.Add(refreshButton);

            var toggleButton = UIBuilder.CreateToolbarButton(showCodeWindow ? "Settings" : "Preview", ToggleWindowMode, true, false);
            toggleButton.name = "toggleButton";
            toolbar.Add(toggleButton);

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(codeView);
            rootVisualElement.Add(settingsContainer);
            ChangeSelection();

            codeView.style.display = showCodeWindow ? DisplayStyle.Flex : DisplayStyle.None;
            settingsContainer.style.display = showCodeWindow ? DisplayStyle.None : DisplayStyle.Flex;
=======
            var win = GetWindow<CSharpPreviewWindow>();
            win.titleContent = new GUIContent("C# Preview");
            win.minSize = new Vector2(400, 400);
            win.UpdateCodeDisplay();
>>>>>>> Stashed changes
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
            UpdateCodeDisplay();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeObject is ScriptGraphAsset or CodeAsset or GameObject)
            {
<<<<<<< Updated upstream
                wasVisible = isVisible;
                if (isVisible)
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (this != null && rootVisualElement?.Q<VisualElement>("codeView") != null)
                        {
                            UpdateCodeDisplay();
                        }
                    };
                }
            }
        }

        private void OnAfterAssemblyReload()
        {
            EditorApplication.delayCall += () =>
            {
                if (this != null && rootVisualElement?.Q<VisualElement>("codeView") != null)
                {
                    ChangeSelection();
                }
            };
        }

        private class ExternalEventManipulator : Manipulator
        {
            private readonly List<VisualElement> externalElements;
            private EventCallback<ClickEvent> action;
            public ExternalEventManipulator(List<VisualElement> externalElements, EventCallback<ClickEvent> action)
            {
                this.externalElements = externalElements;
                this.action = action;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                externalElements.ForEach(e => e.RegisterCallback(action));
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                externalElements.ForEach(e => e.UnregisterCallback(action));
            }
        }

        private void CreateSettingsUI(ScrollView settingsContainer)
        {
            settingsManager.InitializeSettings();
            settingsManager.UpdateSettings((settings) =>
            {
                CSharpPreviewSettings.ShouldShowRecommendations = settings.showRecommendations;

                var settingsLabel = new Label("C# Preview Settings")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        fontSize = 18,
                        alignSelf = Align.Center,
                        marginTop = 10
                    }
                };
                settingsContainer.Add(settingsLabel);


                var generationSettingsSection = new VisualElement
                {
                    style = { marginLeft = 10, marginTop = 10 }
                };
                var generationSettingsLabel = new Label("Generation Settings")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14, marginTop = 10 }
                };
                generationSettingsSection.Add(generationSettingsLabel);

                float labelWidth = 200;

                var subgraphToggleContainer = new VisualElement
                {
                    style = { marginTop = 10, flexDirection = FlexDirection.Row }
                };

                var showSubgraphLabel = new Label("Show Subgraph Comment :")
                {
                    tooltip = "Generate a comment where the Subgraph and Port are being generated. Useful for navigating the code.",
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10, width = labelWidth }
                };

                var showSubgraphCommentToggle = new Toggle
                {
                    style = { marginLeft = 10 },
                    value = settings.showSubgraphComment
                };
                CSharpPreviewSettings.ShouldShowSubgraphComment = settings.showSubgraphComment;

                showSubgraphCommentToggle.RegisterValueChangedCallback(evt =>
                {
                    settings.showSubgraphComment = evt.newValue;
                    CSharpPreviewSettings.ShouldShowSubgraphComment = evt.newValue;
                    settings.SaveAndDirty();
                });

                subgraphToggleContainer.Add(showSubgraphLabel);
                subgraphToggleContainer.Add(showSubgraphCommentToggle);
                generationSettingsSection.Add(subgraphToggleContainer);

                var recommendationToggleContainer = new VisualElement
                {
                    style = { marginTop = 5, flexDirection = FlexDirection.Row }
                };

                var showRecommendationLabel = new Label("Show Recommendations :")
                {
                    tooltip = "Show recommendations if there is a better way of generating the code.",
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10, width = labelWidth }
                };

                var showRecommendationToggle = new Toggle
                {
                    style = { marginLeft = 10 },
                    value = settings.showRecommendations
                };
                CSharpPreviewSettings.ShouldShowRecommendations = settings.showRecommendations;
                showRecommendationToggle.RegisterValueChangedCallback(evt =>
                {
                    settings.showRecommendations = evt.newValue;
                    CSharpPreviewSettings.ShouldShowRecommendations = evt.newValue;
                    settings.SaveAndDirty();
                });

                recommendationToggleContainer.Add(showRecommendationLabel);
                recommendationToggleContainer.Add(showRecommendationToggle);
                generationSettingsSection.Add(recommendationToggleContainer);

                var tooltipToggleContainer = new VisualElement
                {
                    style = { marginTop = 5, flexDirection = FlexDirection.Row }
                };

                var showTooltipLabel = new Label("Show Tooltips :")
                {
                    tooltip = "Show tooltips in places where there is a problem.",
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10, width = labelWidth }
                };

                var showTooltipToggle = new Toggle
                {
                    style = { marginLeft = 10 },
                    value = settings.showTooltips
                };
                CSharpPreviewSettings.ShouldGenerateTooltips = settings.showTooltips;
                showTooltipToggle.RegisterValueChangedCallback(evt =>
                {
                    settings.showTooltips = evt.newValue;
                    CSharpPreviewSettings.ShouldGenerateTooltips = evt.newValue;
                    settings.SaveAndDirty();
                });

                tooltipToggleContainer.Add(showTooltipLabel);
                tooltipToggleContainer.Add(showTooltipToggle);
                generationSettingsSection.Add(tooltipToggleContainer);

                var recursionDepthContainer = new VisualElement
                {
                    style = { marginTop = 5, flexDirection = FlexDirection.Row }
                };

                var recursionDepthLabel = new Label("Recursion Depth :")
                {
                    tooltip = "Adjust the recursion depth to handle complex graphs. If you encounter the 'Infinite recursion' error, try increasing this value to allow for deeper recursive calls. Use caution, as setting it too high may impact performance.",
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10, width = labelWidth }
                };

                var recursionDepthField = new IntegerField
                {
                    style = { marginLeft = 10 },
                    value = settings.recursionDepth
                };
                CSharpPreviewSettings.RecursionDepth = settings.recursionDepth;
                recursionDepthField.RegisterValueChangedCallback(evt =>
                {
                    settings.recursionDepth = evt.newValue;
                    CSharpPreviewSettings.RecursionDepth = evt.newValue;
                    settings.SaveAndDirty();
                });

                recursionDepthContainer.Add(recursionDepthLabel);
                recursionDepthContainer.Add(recursionDepthField);
                generationSettingsSection.Add(recursionDepthContainer);

                settingsContainer.Add(generationSettingsSection);

                var syntaxHighlightsSection = new VisualElement
                {
                    style = { marginLeft = 10, marginTop = 10 }
                };

                var syntaxHighlightsLabel = new Label("Syntax Highlights")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14, marginTop = 10 }
                };
                syntaxHighlightsSection.Add(syntaxHighlightsLabel);

                const string defaultVariableColor = "00FFFF";
                UIBuilder.AddColorField(settings, () => CodeBuilder.VariableColor = settings.VariableColor.ToHexString(), syntaxHighlightsSection, "Variable Color :", "The variable color.", settings.VariableColor, color =>
                {
                    settings.VariableColor = color;
                    CodeBuilder.VariableColor = color.ToHexString();
                    settings.SaveAndDirty();
                }, (field) =>
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString($"#{defaultVariableColor}", out var value))
                    {
                        settings.VariableColor = value;
                        field.value = value;
                    }
                    CodeBuilder.VariableColor = default;
                });
                const string defaultStringColor = "CC8833";
                UIBuilder.AddColorField(settings, () => CodeBuilder.StringColor = settings.StringColor.ToHexString(), syntaxHighlightsSection, "String Color :", "The color for strings.", settings.StringColor, color =>
                {
                    settings.StringColor = color;
                    CodeBuilder.StringColor = color.ToHexString();
                    settings.SaveAndDirty();
                }, (field) =>
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString(defaultStringColor, out var value))
                    {
                        settings.StringColor = value;
                        field.value = value;
                    }
                    CodeBuilder.StringColor = defaultStringColor;
                });
                const string defaultNumericColor = "DDFFBB";
                UIBuilder.AddColorField(settings, () => CodeBuilder.NumericColor = settings.NumericColor.ToHexString(), syntaxHighlightsSection, "Numeric Color :", "The color for numeric values.", settings.NumericColor, color =>
                {
                    settings.NumericColor = color;
                    CodeBuilder.NumericColor = color.ToHexString();
                    settings.SaveAndDirty();
                }, (field) =>
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString($"#{defaultNumericColor}", out var value))
                    {
                        settings.NumericColor = value;
                        field.value = value;
                    }
                    CodeBuilder.NumericColor = defaultNumericColor;
                });
                const string defaultConstructColor = "4488FF";
                UIBuilder.AddColorField(settings, () => CodeBuilder.ConstructColor = settings.ConstructColor.ToHexString(), syntaxHighlightsSection, "Construct Color :", "The color for constructs (e.g., private, public, int and float types).", settings.ConstructColor, color =>
                {
                    settings.ConstructColor = color;
                    CodeBuilder.ConstructColor = color.ToHexString();
                    settings.SaveAndDirty();
                }, (field) =>
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString($"#{defaultConstructColor}", out var value))
                    {
                        settings.ConstructColor = value;
                        field.value = value;
                    }
                    CodeBuilder.ConstructColor = defaultConstructColor;
                });
                const string defaultTypeColor = "33EEAA";
                UIBuilder.AddColorField(settings, () => CodeBuilder.TypeColor = settings.TypeColor.ToHexString(), syntaxHighlightsSection, "Type Color :", "The color for data types.", settings.TypeColor, color =>
                {
                    settings.TypeColor = color;
                    CodeBuilder.TypeColor = color.ToHexString();
                    settings.SaveAndDirty();
                }, (field) =>
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString($"#{defaultTypeColor}", out var value))
                    {
                        settings.TypeColor = value;
                        field.value = value;
                    }
                    CodeBuilder.TypeColor = defaultTypeColor;
                });
                const string defaultEnumColor = "FFFFBB";
                UIBuilder.AddColorField(settings, () => CodeBuilder.EnumColor = settings.EnumColor.ToHexString(), syntaxHighlightsSection, "Enum Color :", "The color for enums.", settings.EnumColor, color =>
                {
                    settings.EnumColor = color;
                    CodeBuilder.EnumColor = color.ToHexString();
                    settings.SaveAndDirty();
                }, (field) =>
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString($"#{defaultEnumColor}", out var value))
                    {
                        settings.EnumColor = value;
                        field.value = value;
                    }
                    CodeBuilder.EnumColor = defaultEnumColor;
                });
                const string defaultInterfaceColor = "FFFFBB";
                UIBuilder.AddColorField(settings, () => CodeBuilder.InterfaceColor = settings.InterfaceColor.ToHexString(), syntaxHighlightsSection, "Interface Color :", "The color for interfaces.", settings.InterfaceColor, color =>
                {
                    settings.InterfaceColor = color;
                    CodeBuilder.InterfaceColor = color.ToHexString();
                    settings.SaveAndDirty();
                }, (field) =>
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString($"#{defaultInterfaceColor}", out var value))
                    {
                        settings.InterfaceColor = value;
                        field.value = value;
                    }
                    CodeBuilder.InterfaceColor = defaultInterfaceColor;
                });

                settingsContainer.Add(syntaxHighlightsSection);
            });
        }

        private void ChangeSelection()
        {
            if (Selection.activeObject is CodeAsset _asset)
            {
                asset = _asset;
                ClearComponentNavigationUI();
                UpdateCodeDisplay();
            }
            else if (Selection.activeObject is ScriptGraphAsset _graphAsset)
            {
                asset = _graphAsset;
                ClearComponentNavigationUI();
                UpdateCodeDisplay();
            }
            else if (Selection.activeObject is GameObject gameObject)
            {
                asset = gameObject;
                UpdateComponentSelectionUI(gameObject);
                UpdateCodeDisplay();
            }
            else if (asset != null)
            {
                if (asset is GameObject _gameObject)
                {
                    UpdateComponentSelectionUI(_gameObject);
                }
                else
                {
                    ClearComponentNavigationUI();
                }
=======
                _asset = Selection.activeObject;
                canCompile = true;
                manualCode = "";
>>>>>>> Stashed changes
                UpdateCodeDisplay();
                Repaint();
            }
        }

        /// <summary>
        /// Opens the preview window with specific code, no asset required.
        /// </summary>
        public static void OpenWithCode(string code, bool canCompile)
        {
            var win = GetWindow<CSharpPreviewWindow>();
            win.titleContent = new GUIContent("C# Preview");
            win.minSize = new Vector2(400, 400);
            win.manualCode = code;
            win.canCompile = canCompile;
            win.UpdateCodeDisplay();
            win.Show();
        }

        private void UpdateCodeDisplay()
        {
            string code = string.Empty;

            if (!string.IsNullOrEmpty(manualCode))
            {
                code = manualCode;
            }
            else if (_asset != null)
            {
                code = LoadCode();
            }

            codeLines = string.IsNullOrEmpty(code)
                ? Array.Empty<string>()
                : code.Split(new[] { '\n' }, StringSplitOptions.None);

            var clickableRegions = CodeUtility.ExtractAndPopulateClickableRegions(code ?? "");
            cachedRegions = clickableRegions
                .GroupBy(r => r.startLine)
                .ToDictionary(g => g.Key, g => g.ToList());

            lineWidths = new float[codeLines.Length];
        }

        private string LoadCode()
        {
            if (_asset == null) return string.Empty;
            var generator = CodeGenerator.GetSingleDecorator(_asset);
            if (generator == null) return string.Empty;

            var code = generator.Generate(0);
            return code.RemoveMarkdown();
        }
        private float[] lineWidths;
        float contentWidth;
        private void OnGUI()
        {
            if (CodeGeneratorValueUtility.currentAsset == null && _asset != null)
                CodeGeneratorValueUtility.currentAsset = _asset;

            DrawToolbar();

            if (showSettings)
            {
                EditorGUILayout.Space();
                settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                settingsManager.InitializeSettings();
                settingsManager.UpdateSettings(settings => UIBuilder.DrawCSharpPreviewSettings(settings, () => RefreshPreview()));
                EditorGUILayout.EndScrollView();
                return;
            }

            if (codeLines == null || codeLines.Length == 0)
            {
                GUILayout.Label("No code to display.");
                return;
            }

            float lineHeight = LineHeight * visualZoom;
            float totalHeight = codeLines.Length * lineHeight;

            contentWidth = position.width;
            if (lineWidths != null && lineWidths.Length > 0)
            {
                contentWidth = Mathf.Max(position.width, lineWidths.Max()) + 20f;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, true);

            Rect scrollRect = GUILayoutUtility.GetRect(contentWidth, totalHeight);

            float bgWidth = Mathf.Max(contentWidth, position.width);
            float bgHeight = Mathf.Max(totalHeight, position.height);

            EditorGUI.DrawRect(new Rect(0, 0, bgWidth, bgHeight), new Color(0.15f, 0.15f, 0.15f));

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / lineHeight) - BufferLines);
            int lastVisible = Mathf.Min(codeLines.Length - 1,
                Mathf.CeilToInt((scrollPosition.y + position.height) / lineHeight) + BufferLines);

            float maxLineWidth = 0f;

            for (int i = firstVisible; i <= lastVisible; i++)
            {
                Rect lineRect = new Rect(0, i * lineHeight, contentWidth, lineHeight);
                float lineWidth = DrawLine(lineRect, i);
                maxLineWidth = Mathf.Max(maxLineWidth, lineWidth);
            }

            if (lineWidths != null && lineWidths.Length > 0)
            {
                contentWidth = Mathf.Max(position.width, maxLineWidth) + 20f;
            }

            EditorGUILayout.EndScrollView();
        }

        private bool showSettings = false;
        private readonly CSharpPreviewSettingsManager settingsManager = new CSharpPreviewSettingsManager();
        private Vector2 settingsScroll;

        int currentComponentIndex;

        private float sliderValue = 0.15f;
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Zoom", GUILayout.Width(40));
            var newSliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 1f, GUILayout.Width(150));
            if (Math.Abs(newSliderValue - sliderValue) > 0.001f)
            {
                sliderValue = newSliderValue;
                visualZoom = Mathf.Lerp(0.5f, 1.5f, sliderValue);
            }
            GUILayout.Label($"{visualZoom * 100f:0}%", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:");
            if (GUILayout.Button("<", EditorStyles.toolbarButton))
            {
                if (searchMatches.Count > 0)
                {
                    currentMatchIndex = (currentMatchIndex - 1 + searchMatches.Count) % searchMatches.Count;
                    ScrollToLine(searchMatches[currentMatchIndex]);
                }
            }

            string newSearchQuery = GUILayout.TextField(searchQuery, EditorStyles.toolbarTextField, GUILayout.Width(150));
            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
                UpdateSearchMatches();
            }

            if (GUILayout.Button(">", EditorStyles.toolbarButton))
            {
                if (searchMatches.Count > 0)
                {
                    currentMatchIndex = (currentMatchIndex + 1) % searchMatches.Count;
                    ScrollToLine(searchMatches[currentMatchIndex]);
                }
            }
            if (searchMatches.Count > 0)
                GUILayout.Label($"{currentMatchIndex + 1}/{searchMatches.Count}");
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(!canCompile);
            if (GUILayout.Button("Compile", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                CompileCode();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy To Clipboard", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                if (allCodeSelected || (selectedLines.Count == 0 && selectedRegions.Count == 0))
                {
                    GUIUtility.systemCopyBuffer = string.Join("\n", codeLines.Select(l => CodeUtility.CleanCode(RemoveColorTags(l))));
                }
                else
                {
                    var lineSelections = new Dictionary<int, List<string>>();

                    foreach (var line in selectedLines)
                    {
                        if (line >= 0 && line < codeLines.Length)
                        {
                            if (!lineSelections.ContainsKey(line))
                                lineSelections[line] = new List<string>();

                            lineSelections[line].Add(CodeUtility.CleanCode(RemoveColorTags(codeLines[line])));
                        }
                    }

                    foreach (var region in selectedRegions.OrderBy(c => c.startLine).ThenBy(c => c.startIndex))
                    {
                        int lineIndex = region.startLine;
                        if (!lineSelections.ContainsKey(lineIndex))
                            lineSelections[lineIndex] = new List<string>();

                        lineSelections[lineIndex].Add(CodeUtility.CleanCode(RemoveColorTags(region.code)));
                    }

                    var finalLines = lineSelections.OrderBy(kv => kv.Key).Select(v => string.Join("", v.Value));

                    GUIUtility.systemCopyBuffer = string.Join("\n", finalLines);
                }
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                UpdateCodeDisplay();
                Repaint();
            }

            if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                showSettings = !showSettings;
            }

            if (_asset is GameObject @object)
            {
                var components = @object.GetComponents<ScriptMachine>();
                var generator = CodeGenerator.GetSingleDecorator(@object) as GameObjectGenerator;

                if (components == null || components.Length <= 1)
                {
                    currentComponentIndex = 0;
                    GUILayout.EndHorizontal();
                    return;
                }

                if (currentComponentIndex < 0 || currentComponentIndex >= components.Length)
                    currentComponentIndex = 0;

                generator.current = components[currentComponentIndex];
                CodeGeneratorValueUtility.currentAsset = components[currentComponentIndex];

                if (GUILayout.Button("<", EditorStyles.toolbarButton))
                {
                    NavigateComponents(-1, generator);
                }
                GUILayout.Label($"{currentComponentIndex + 1}/{components.Length}");
                if (GUILayout.Button(">", EditorStyles.toolbarButton))
                {
                    NavigateComponents(1, generator);
                }
            }


            GUILayout.EndHorizontal();
        }

        private void NavigateComponents(int direction, GameObjectGenerator currentGameObjectGenerator)
        {
            if (currentGameObjectGenerator?.components == null) return;

            var components = currentGameObjectGenerator.components;
            var newIndex = currentComponentIndex + direction;

            if (newIndex >= 0 && newIndex < components.Length)
            {
                currentComponentIndex = newIndex;
                currentGameObjectGenerator.current = components[currentComponentIndex];
                UpdateCodeDisplay();
            }
        }

        private void CompileCode()
        {
            if (_asset != null)
                AssetCompiler.CompileAsset(_asset);
        }

        private string RemoveColorTags(string input)
        {
            return Regex.Replace(input, @"<color=#[0-9a-fA-F]{6,8}>|</color>", string.Empty, RegexOptions.Compiled);
        }

        private const float LineNumberWidth = 35f;
        private HashSet<int> selectedLines = new();
        private HashSet<ClickableRegion> selectedRegions = new();
        private string selectedUnitID;
        private bool allCodeSelected = false;

<<<<<<< Updated upstream
        private const int BUFFER_LINES = 20;
        private Dictionary<int, (VisualElement lineNumber, VisualElement content)> virtualizedLines = new Dictionary<int, (VisualElement, VisualElement)>();
        private int firstVisibleLine = 0;
        private int lastVisibleLine = 0;
        private string[] cachedLines;
        private Dictionary<int, List<ClickableRegion>> cachedRegions;

        public void UpdateCodeDisplay()
        {
            if (asset != null && rootVisualElement?.Q<VisualElement>("codeView") != null)
            {
                var scrollView = rootVisualElement.Q<VisualElement>("codeView").Q<ScrollView>("codeContainer");
                var lineNumbersScrollView = rootVisualElement.Q<VisualElement>("codeView").Q<ScrollView>("lineNumbersContainer");

                lastScrollPosition = scrollView.scrollOffset;
                lastLineNumbersScrollPosition = lineNumbersScrollView.scrollOffset.y;

                var loadedCode = LoadCode();
                DisplayCode(lineNumbersScrollView, scrollView, loadedCode);

                EditorApplication.delayCall += () =>
                {
                    scrollView.scrollOffset = lastScrollPosition;
                    lineNumbersScrollView.scrollOffset = new Vector2(0, lastLineNumbersScrollPosition);
                    RefreshVirtualization(scrollView, lineNumbersScrollView);
                };
            }
        }

        Dictionary<string, List<(Label, int)>> unitIDRegions = new Dictionary<string, List<(Label, int)>>();
        private void DisplayCode(ScrollView lineNumbersScrollView, ScrollView scrollView, string code)
        {
            scrollView.Clear();
            lineNumbersScrollView.Clear();
            labels.Clear();
            unitIDRegions.Clear();
            virtualizedLines.Clear();

            var clickableRegions = CodeUtility.ExtractAndPopulateClickableRegions(code);
            cachedRegions = clickableRegions.GroupBy(region => region.startLine)
                                          .ToDictionary(g => g.Key, g => g.ToList());

            var beforeRemoveLines = CodeUtility.RemovePattern(code, "[CommunityAddonsCodeSelectable(", ")]");
            var operatedLines = CodeUtility.RemovePattern(beforeRemoveLines, "[CommunityAddonsCodeSelectableEnd(", ")]");
            cachedLines = operatedLines.Split('\n');

            var contentContainer = new VisualElement
            {
                name = "content",
                style = {
                    flexShrink = 0,
                    flexGrow = 1
                }
            };
            contentContainer.style.height = cachedLines.Length * 15;
            scrollView.Add(contentContainer);

            var lineNumbersContent = new VisualElement { name = "lineNumbers" };
            lineNumbersContent.style.height = contentContainer.style.height;
            lineNumbersContent.style.width = 35;
            lineNumbersScrollView.Add(lineNumbersContent);

            RenderVisibleLines(scrollView, lineNumbersScrollView, 0, Math.Min(50, cachedLines.Length));
            scrollView.verticalScroller.valueChanged += (value) =>
            {
                lineNumbersScrollView.scrollOffset = new Vector2(0, value);
                UpdateVisibleLines(scrollView, lineNumbersScrollView);
            };

            SetupScrollbarMarkers(scrollView);

            searchResults.Clear();
            currentSearchIndex = -1;
            if (!string.IsNullOrEmpty(searchText))
            {
                PerformSearch();
            }
        }

        private void UpdateVisibleLines(ScrollView scrollView, ScrollView lineNumbersScrollView)
        {
            if (cachedLines == null) return;

            float scrollPos = scrollView.scrollOffset.y;
            float viewportHeight = scrollView.resolvedStyle.height;
            float lineHeight = 15 * zoomFactor;

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(scrollPos / lineHeight) - BUFFER_LINES);
            int lastVisible = Mathf.Min(cachedLines.Length - 1,
                Mathf.CeilToInt((scrollPos + viewportHeight) / lineHeight) + BUFFER_LINES);

            if (firstVisible != firstVisibleLine || lastVisible != lastVisibleLine)
            {
                firstVisibleLine = firstVisible;
                lastVisibleLine = lastVisible;
                RenderVisibleLines(scrollView, lineNumbersScrollView, firstVisible, lastVisible);
=======
        private string CleanLine(string line)
        {
            var toolTipText = CodeUtility.ExtractTooltip(line, out _);
            string cleanText = CodeUtility.CleanCode(toolTipText);
            return RemoveColorTags(cleanText);
        }
        private float DrawLine(Rect rect, int index)
        {
            float x = rect.x - scrollPosition.x;

            if (!string.IsNullOrEmpty(searchQuery) && CleanLine(codeLines[index]).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), highlightColor);
            }

            Color borderColor = new Color(0.20f, 0.20f, 0.20f, 0.1f);
            const float borderHeight = 1f;
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, borderHeight), borderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - borderHeight, rect.width, borderHeight), borderColor);

            string lineNumberText = $"{index + 1,4}: ";
            Vector2 numberSize = baseStyle.CalcSize(new GUIContent(lineNumberText));
            float lineNumberWidth = Mathf.Max(LineNumberWidth, numberSize.x);
            Rect lineNumberRect = new Rect(x, rect.y, lineNumberWidth, rect.height);
            GUI.Label(lineNumberRect, $"{index + 1,4}: ", baseStyle);
            x += lineNumberWidth;

            float lineHeight = LineHeight * visualZoom;

            if (cachedRegions != null && cachedRegions.TryGetValue(index, out var regions) && regions.Count > 0)
            {
                AdjustLeadingWhitespacesForFirstRegion(codeLines[index], regions[0]);
                bool isMatchingInLine = false;
                foreach (var region in regions)
                {

                    if (!string.IsNullOrEmpty(searchQuery) && !isMatchingInLine)
                    {
                        isMatchingInLine = true;
                        var cleanedCode = CleanLine(codeLines[index]);
                        int matchIndex = cleanedCode.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
                        if (matchIndex >= 0)
                        {
                            Vector2 startPos = noPaddingStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanedCode), matchIndex);
                            Vector2 endPos = noPaddingStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanedCode), matchIndex + searchQuery.Length);
                            Rect highlightRect = new Rect(x + startPos.x, rect.y, endPos.x - startPos.x, rect.height);
                            EditorGUI.DrawRect(highlightRect, highlightColor.WithAlpha(0.3f));
                        }
                    }

                    var toolTipText = CodeUtility.ExtractTooltip(region.code, out var tooltip);
                    string cleanText = CodeUtility.CleanCode(toolTipText);

                    Vector2 size = noPaddingStyle.CalcSize(Temp(cleanText));
                    var content = string.IsNullOrEmpty(tooltip) ? Temp(cleanText) : Temp(cleanText, tooltip);
                    Rect buttonRect = new Rect(x, rect.y, size.x, rect.height);

                    if (allCodeSelected || selectedLines.Contains(index) || (region != null && selectedRegions.Contains(region)))
                    {
                        EditorGUI.DrawRect(buttonRect, new Color(0.25f, 0.5f, 0.8f, 0.3f));
                    }
                    else if (selectedUnitID == region.unitId)
                    {
                        EditorGUI.DrawRect(buttonRect, new Color(0.25f, 0.25f, 0.25f, 0.4f));
                    }

                    if (GUI.Button(buttonRect, content, noPaddingStyle))
                    {
                        selectedUnitID = region.unitId;
                        HandleClick(index, region);
                        HandleClickableRegionClick(region.unitId, index);
                    }

                    x += size.x;
                }
            }
            else
            {
                string lineText = CodeUtility.ExtractTooltip(codeLines[index], out var tooltip);
                string cleanText = CodeUtility.CleanCode(lineText);

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    int matchIndex = RemoveColorTags(cleanText).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
                    if (matchIndex >= 0)
                    {
                        Vector2 startPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), matchIndex);
                        Vector2 endPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), matchIndex + searchQuery.Length);
                        Rect highlightRect = new Rect(x + startPos.x, rect.y, endPos.x - startPos.x, rect.height);
                        EditorGUI.DrawRect(highlightRect, highlightColor.WithAlpha(0.3f));
                    }
                }

                Vector2 size = baseStyle.CalcSize(Temp(cleanText));
                Rect labelRect = new Rect(x, rect.y, size.x, rect.height);
                var content = string.IsNullOrEmpty(tooltip) ? Temp(cleanText) : Temp(cleanText, tooltip);

                if (allCodeSelected || selectedLines.Contains(index))
                {
                    EditorGUI.DrawRect(labelRect, new Color(0.25f, 0.5f, 0.8f, 0.3f));
                }

                if (GUI.Button(labelRect, content, baseStyle))
                {
                    selectedUnitID = "";
                    HandleClick(index, null);
                }

                x += size.x;
            }

            lineWidths[index] = x;
            return x;
        }

        private int clickCount = 0;
        private float lastClickTime = 0f;
        private const float doubleClickThreshold = 0.3f;
        private int lastClickedLine = -1;

        private void HandleClick(int index, ClickableRegion region)
        {
            var e = Event.current;
            bool isRegion = region != null;

            if (e != null && e.button == 0)
            {
                float time = Time.realtimeSinceStartup;
                if (index == lastClickedLine && time - lastClickTime <= doubleClickThreshold)
                {
                    clickCount++;
                }
                else
                {
                    clickCount = 1;
                }

                lastClickTime = time; lastClickedLine = index;
                bool ctrl = e.control || e.command;

                if (clickCount == 2)
                {
                    if (ctrl)
                    {
                        selectedLines.Add(index);
                        if (selectedRegions.Contains(region))
                            selectedRegions.Remove(region);
                    }
                    else
                    {
                        selectedLines.Clear();
                        selectedRegions.Clear();
                        selectedLines.Add(index);
                    }
                    allCodeSelected = false;
                }
                else if (clickCount == 1)
                {
                    if (ctrl)
                    {
                        if (isRegion)
                        {
                            if (selectedRegions.Contains(region))
                                selectedRegions.Remove(region);
                            else
                                selectedRegions.Add(region);
                        }
                        else
                        {
                            if (selectedLines.Contains(index))
                                selectedLines.Remove(index);
                            else
                                selectedLines.Add(index);
                        }
                    }
                    else
                    {
                        selectedLines.Clear();
                        selectedRegions.Clear();
                        if (isRegion)
                            selectedRegions.Add(region);
                        else
                            selectedLines.Add(index);
                        allCodeSelected = false;
                    }
                }
                else if (clickCount >= 3)
                {
                    allCodeSelected = true;
                    selectedLines.Clear();
                    selectedRegions.Clear();
                    clickCount = 0;
                }
            }
        }

        const float Height = 16f;
        private static GUIStyle _noPaddingStyle;
        private GUIStyle noPaddingStyle
        {
            get
            {
                if (_noPaddingStyle == null)
                {
                    _noPaddingStyle = new GUIStyle(baseStyle)
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                    };
                    _noPaddingStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);

                    // prevent blue-on-click
                    var c = Color.white;
                    _noPaddingStyle.normal.textColor = c;
                    _noPaddingStyle.hover.textColor = c;
                    _noPaddingStyle.active.textColor = c;
                    _noPaddingStyle.focused.textColor = c;
                }
                else
                {
                    _noPaddingStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);
                }
                return _noPaddingStyle;
>>>>>>> Stashed changes
            }
        }

        private static GUIStyle _baseStyle;
        private GUIStyle baseStyle
        {
            get
            {
                if (_baseStyle == null)
                {
                    _baseStyle = new GUIStyle(EditorStyles.label)
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                        richText = true
                    };
                    _baseStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);

                    var c = Color.white;
                    _baseStyle.normal.textColor = c;
                    _baseStyle.hover.textColor = c;
                    _baseStyle.active.textColor = c;
                    _baseStyle.focused.textColor = c;
                }
                else
                {
                    _baseStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);
                }
                return _baseStyle;
            }
        }

        private static readonly GUIContent _temp = new GUIContent();
        private static readonly GUIContent _tempTooltip = new GUIContent();
        private GUIContent Temp(string text)
        {
            _temp.text = text;
            return _temp;
        }

        private GUIContent Temp(string text, string tooltip)
        {
            _tempTooltip.text = text;
            _tempTooltip.tooltip = tooltip;
            return _tempTooltip;
        }

        private void AdjustLeadingWhitespacesForFirstRegion(string line, ClickableRegion firstRegion)
        {
            int leadingWhitespaceLength = 0;
            while (leadingWhitespaceLength < line.Length && char.IsWhiteSpace(line[leadingWhitespaceLength]))
            {
                leadingWhitespaceLength++;
            }

            string whitespace = line[..leadingWhitespaceLength];
            if (!firstRegion.code.StartsWith(whitespace))
            {
                firstRegion.code = whitespace + firstRegion.code.TrimStart();
            }
        }

        private UnityEngine.Object GetRootObject(GraphReference reference)
        {
            var root = reference.rootObject;
            if (root is MethodDeclaration method) return method.parentAsset;
            if (root is ConstructorDeclaration constructor) return constructor.parentAsset;
            if (root is PropertyGetterMacro getter) return getter.parentAsset;
            if (root is PropertySetterMacro setter) return setter.parentAsset;
            return root;
        }

        private void HandleClickableRegionClick(string unitId, int line)
        {
            var code = CodeGenerator.GetSingleDecorator(_asset);

            if (GraphWindow.active != null && GraphWindow.active.reference != null && GraphWindow.active.context.graph is FlowGraph)
            {
                if (GetRootObject(GraphWindow.active.reference) != (_asset is GameObject ? (code as GameObjectGenerator).current : _asset))
                {
                    OpenInitial(unitId, line, code);
                }
                var reference = GraphWindow.active.reference.isRoot ? GraphWindow.active.reference : GraphWindow.active.reference.root.GetReference() as GraphReference;
                GraphWindow.active.Focus();

                List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                if (reference.macro != null && reference.macro is MethodDeclaration or ConstructorDeclaration or FieldDeclaration)
                {
                    if (reference.macro is MethodDeclaration methodDeclaration)
                    {
                        ProcessMethodDeclaration(methodDeclaration, units);
                    }
                    else if (reference.macro is ConstructorDeclaration constructorDeclaration)
                    {
                        ProcessConstructorDeclaration(constructorDeclaration, units);
                    }
                    else if (reference.macro is FieldDeclaration fieldDeclaration)
                    {
                        ProcessFieldDeclaration(fieldDeclaration, units);
                    }
                }
                else
                {
                    units = GraphTraversal.TraverseFlowGraph<Unit>(reference).ToList();
                }
                var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                if (ordered.Count() > 0 && ordered.Any(selectable => selectable.result.Item2 != null))
                {
                    ordered = ordered.Where(selectable => selectable.result.Item2 != null);
                }
                if (units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString()).Count() > 0)
                {
                    if (!GraphWindow.active.reference.isRoot && reference != ordered.First().result.Item1)
                    {
                        GraphWindow.active.reference = GraphWindow.active.reference.root.GetReference() as GraphReference;
                    }
                    var path = GraphTraversal.GetReferencePath(ordered.First().result.Item1);
                    if (GraphWindow.active.reference != ordered.First().result.Item1)
                    {
                        foreach (var item in path)
                        {
                            if (item.Item2 != null)
                            {
                                GraphWindow.active.reference = GraphWindow.active.reference.ChildReference(item.Item2, false);
                            }
                            else if (item.Item1.isRoot)
                            {
                                GraphWindow.active.reference = item.Item1;
                            }
                        }
                    }
                    if (ordered.First().result.Item2 != null)
                    {
                        var canvas = GraphWindow.active.context.canvas as FlowCanvas;
                        GraphWindow.active.context.BeginEdit();
                        canvas.ViewElements(new List<Unit>() { ordered.First().result.Item2 });
                        GraphWindow.active.context.canvas.UpdateViewport();
                        GraphWindow.active.context.EndEdit();
                    }
                }
            }
            else
            {
                OpenInitial(unitId, line, code);
            }
        }

        private void OpenInitial(string unitId, int line, CodeGenerator code)
        {
            if (code is ClassAssetGenerator classAssetGenerator)
            {
                if (classAssetGenerator.Data != null)
                {
                    List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                    foreach (var constructorDeclaration in classAssetGenerator.Data.constructors)
                    {
                        ProcessConstructorDeclaration(constructorDeclaration, units);
                    }

                    foreach (var fieldDeclaration in classAssetGenerator.Data.variables)
                    {
                        ProcessFieldDeclaration(fieldDeclaration, units);
                    }

                    foreach (var methodDeclaration in classAssetGenerator.Data.methods)
                    {
                        ProcessMethodDeclaration(methodDeclaration, units);
                    }
                    var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                    GraphWindow.OpenActive(ordered.First().result.Item1);
                    HandleClickableRegionClick(unitId, line);
                }
            }
            else if (code is StructAssetGenerator structAssetGenerator)
            {
                if (structAssetGenerator.Data != null)
                {
                    List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                    foreach (var constructorDeclaration in structAssetGenerator.Data.constructors)
                    {
                        ProcessConstructorDeclaration(constructorDeclaration, units);
                    }

                    foreach (var fieldDeclaration in structAssetGenerator.Data.variables)
                    {
                        ProcessFieldDeclaration(fieldDeclaration, units);
                    }

                    foreach (var methodDeclaration in structAssetGenerator.Data.methods)
                    {
                        ProcessMethodDeclaration(methodDeclaration, units);
                    }
                    var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                    GraphWindow.OpenActive(ordered.First().result.Item1.isRoot ? ordered.First().result.Item1 : ordered.First().result.Item1.root.GetReference() as GraphReference);
                    HandleClickableRegionClick(unitId, line);
                }
            }
            else if (code is ScriptGraphAssetGenerator graphAssetGenerator)
            {
                if (graphAssetGenerator.Data != null)
                {
<<<<<<< Updated upstream
                    if (gameObjectGenerator.Data != null)
                    {
                        List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                        units = GraphTraversal.TraverseFlowGraph(gameObjectGenerator.current.GetReference() as GraphReference).ToList();
                        var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                        GraphWindow.OpenActive(ordered.First().result.Item1.isRoot ? ordered.First().result.Item1 : ordered.First().result.Item1.root.GetReference() as GraphReference);
                        HandleClickableRegionClick(unitId, line);
                    }
=======
                    List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                    units = GraphTraversal.TraverseFlowGraph<Unit>(graphAssetGenerator.Data.GetReference() as GraphReference).ToList();
                    var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                    GraphWindow.OpenActive(ordered.First().result.Item1.isRoot ? ordered.First().result.Item1 : ordered.First().result.Item1.root.GetReference() as GraphReference);
                    HandleClickableRegionClick(unitId, line);
                }
            }
            else if (code is GameObjectGenerator gameObjectGenerator)
            {
                if (gameObjectGenerator.Data != null)
                {
                    List<(GraphReference, Unit)> units = new();
                    units = GraphTraversal.TraverseFlowGraph<Unit>(gameObjectGenerator.current.GetReference() as GraphReference).ToList();
                    var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                    GraphWindow.OpenActive(ordered.First().result.Item1.isRoot ? ordered.First().result.Item1 : ordered.First().result.Item1.root.GetReference() as GraphReference);
                    HandleClickableRegionClick(unitId, line);
>>>>>>> Stashed changes
                }
            }
        }

        private void ProcessMethodDeclaration(MethodDeclaration methodDeclaration, List<(GraphReference, Unit)> units)
        {
            if (methodDeclaration.parentAsset is ClassAsset classAsset)
            {
                AddUnitsFromClassAsset(classAsset, units);
            }
            else if (methodDeclaration.parentAsset is StructAsset structAsset)
            {
                AddUnitsFromStructAsset(structAsset, units);
            }
        }

        private void ProcessConstructorDeclaration(ConstructorDeclaration constructorDeclaration, List<(GraphReference, Unit)> units)
        {
            if (constructorDeclaration.parentAsset is ClassAsset classAsset)
            {
                AddUnitsFromClassAsset(classAsset, units);
            }
            else if (constructorDeclaration.parentAsset is StructAsset structAsset)
            {
                AddUnitsFromStructAsset(structAsset, units);
            }
        }

        private void ProcessFieldDeclaration(FieldDeclaration fieldDeclaration, List<(GraphReference, Unit)> units)
        {
            if (fieldDeclaration.parentAsset is ClassAsset classAsset)
            {
                AddUnitsFromClassAsset(classAsset, units);
            }
            else if (fieldDeclaration.parentAsset is StructAsset structAsset)
            {
                AddUnitsFromStructAsset(structAsset, units);
            }
        }

        private void AddUnitsFromClassAsset(ClassAsset classAsset, List<(GraphReference, Unit)> units)
        {
            foreach (var method in classAsset.methods)
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(method.GetReference() as GraphReference));

            foreach (var constructor in classAsset.constructors)
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(constructor.GetReference() as GraphReference));

            foreach (var variable in classAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        private void AddUnitsFromStructAsset(StructAsset structAsset, List<(GraphReference, Unit)> units)
        {
            foreach (var method in structAsset.methods)
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(method.GetReference() as GraphReference));

            foreach (var constructor in structAsset.constructors)
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(constructor.GetReference() as GraphReference));

            foreach (var variable in structAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        private void UpdateSearchMatches()
        {
<<<<<<< Updated upstream
            string tooltip;
            var codeWithoutTooltip = CodeUtility.ExtractTooltip(text, out tooltip);
            var label = new Label(CodeUtility.RemoveAllSelectableTags(codeWithoutTooltip))
            {
                tooltip = tooltip
            };
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
            label.enableRichText = true;
            label.style.color = Color.white;
            label.style.backgroundColor = new Color(1, 1, 1, 0);
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.paddingLeft = 0;
            label.style.paddingRight = 0;
            label.style.paddingTop = 0;
            label.style.paddingBottom = 0;
            label.style.marginLeft = 0;
            label.style.marginRight = 0;
            label.style.marginTop = 0;
            label.style.marginBottom = 0;
=======
            searchMatches.Clear();
            currentMatchIndex = 0;
>>>>>>> Stashed changes

            if (string.IsNullOrEmpty(searchQuery) || codeLines == null) return;

            for (int i = 0; i < codeLines.Length; i++)
            {
                if (CleanLine(codeLines[i]).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    searchMatches.Add(i);
            }

<<<<<<< Updated upstream
            label.RegisterCallback<ClickEvent>(evt => SelectLabel(label, evt, "", currentLine));
            return label;
        }

        private Label CreateCodeLabel(ClickableRegion region, int currentLine)
        {
            var codeWithoutTooltip = CodeUtility.ExtractTooltip(region.code, out string tooltip);
            var label = new Label(CodeUtility.RemoveAllSelectableTags(codeWithoutTooltip))
            {
                tooltip = tooltip
            };
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
            label.style.color = Color.white;
            label.enableRichText = true;
            label.style.backgroundColor = new Color(1, 1, 1, 0);
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.paddingLeft = 0;
            label.style.paddingRight = 0;
            label.style.paddingTop = 0;
            label.style.paddingBottom = 0;
            label.style.marginLeft = 0;
            label.style.marginRight = 0;
            label.style.marginTop = 0;
            label.style.marginBottom = 0;

            if (selectedLabels.Any(sl => sl.Item2 == currentLine && sl.Item1.text == label.text))
            {
                label.style.backgroundColor = new Color(0.25f, 0.5f, 0.8f, 0.3f);
            }
            else if (relatedLabels.Any(rl => rl.text == label.text))
            {
                label.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);
            }

            label.RegisterCallback<ClickEvent>(evt =>
            {
                SelectLabel(label, evt, region.unitId, currentLine);
                OnCodeRegionClicked(region, currentLine);
            });
            return label;
=======
            if (searchMatches.Count > 0)
                ScrollToLine(searchMatches[currentMatchIndex]);
>>>>>>> Stashed changes
        }

        private void ScrollToLine(int line)
        {
            float lineHeight = LineHeight * visualZoom;
            scrollPosition.y = line * lineHeight;
        }

        [SerializeField]
        private static CSharpPreviewWindow window;
        public static void RefreshPreview(bool open = false)
        {
            if (window == null && open) window = GetWindow<CSharpPreviewWindow>();
            if (window != null)
            {
<<<<<<< Updated upstream
                SelectWholeLine(currentLine, unitId);
            }
            // else if (doubleClickCount == 2)
            // {
            //     SelectAllCode();
            //     doubleClickCount = 0;
            // }
        }

        private void SelectWholeLine(int currentLine, string unitId)
        {
            foreach (var (label, line) in labels.FindAll(l => l.Item2 == currentLine))
            {
                AddLabelToSelection(label, unitId, line);
            }
        }

        // private void SelectAllCode()
        // {
        //     foreach (var (label, line) in labels)
        //     {
        //         AddLabelToSelection(label, "unitId", line);
        //     }
        // }

        private void AddLabelToSelection(Label label, string unitId, int currentLine)
        {
            if (unitIDRegions.ContainsKey(unitId))
            {
                selectedLabels.Add((label, currentLine));
                label.style.backgroundColor = new Color(0.25f, 0.5f, 0.8f, 0.3f);
                foreach (var (targetLabel, line) in unitIDRegions[unitId])
                {
                    if (targetLabel != label && !selectedLabels.Contains((targetLabel, line)))
                    {
                        targetLabel.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);
                        relatedLabels.Add(targetLabel);
                    }
                }
                UpdateScrollBarMarkers(rootVisualElement.Q<ScrollView>("codeContainer"), selectedLabels);
            }
            else
            {
                label.style.backgroundColor = new Color(0.25f, 0.5f, 0.8f, 0.3f);
                selectedLabels.Add((label, currentLine));
                UpdateScrollBarMarkers(rootVisualElement.Q<ScrollView>("codeContainer"), selectedLabels);
            }
        }

        private void DeselectLabel(Label label, string unitId, int currentLine)
        {
            if (unitIDRegions.ContainsKey(unitId))
            {
                selectedLabels.Remove((label, currentLine));
                if (unitIDRegions[unitId].Contains((label, currentLine)))
                    label.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);
                else
                    label.style.backgroundColor = new Color(1, 1, 1, 0);
                foreach (var (targetLabel, line) in unitIDRegions[unitId])
                {
                    if (targetLabel != label && !selectedLabels.Contains((targetLabel, line)) && !selectedLabels.Any(_label => unitIDRegions[unitId].Contains(_label)))
                    {
                        targetLabel.style.backgroundColor = new Color(1, 1, 1, 0);
                        relatedLabels.Remove(targetLabel);
                    }
                }
                UpdateScrollBarMarkers(rootVisualElement.Q<ScrollView>("codeContainer"), selectedLabels);
            }
            else
            {
                label.style.backgroundColor = new Color(1, 1, 1, 0);
                selectedLabels.Remove((label, currentLine));
                UpdateScrollBarMarkers(rootVisualElement.Q<ScrollView>("codeContainer"), selectedLabels);
            }
        }

        private void ClearSelection()
        {
            foreach (var selectedLabel in selectedLabels)
            {
                selectedLabel.Item1.style.backgroundColor = new Color(1, 1, 1, 0);
            }
            foreach (var selectedLabel in relatedLabels)
            {
                selectedLabel.style.backgroundColor = new Color(1, 1, 1, 0);
            }
            selectedLabels.Clear();
            relatedLabels.Clear();
            UpdateScrollBarMarkers(rootVisualElement.Q<ScrollView>("codeContainer"), selectedLabels);
        }

        private VisualElement markerContainer;
        ExternalEventManipulator scrollBarMarkerManipulator;
        private void SetupScrollbarMarkers(ScrollView scrollView)
        {
            if (scrollBarMarkerManipulator != null)
            {
                scrollView.RemoveManipulator(scrollBarMarkerManipulator);
                if (markerContainer != null && markerContainer.parent != null)
                    markerContainer.parent.Remove(markerContainer);
            }

            var scroller = scrollView.verticalScroller;
            if (scroller == null) return;

            scrollBarMarkerManipulator = new ExternalEventManipulator(labels.Select(val => val.label as VisualElement).ToList(), (evt) => UpdateScrollBarMarkers(scrollView, selectedLabels));

            // Add markers to the scroller instead of a separate container
            foreach (var (label, line) in selectedLabels)
            {
                var marker = CreateScrollbarMarker(scroller, line, true);
                scroller.Add(marker);
            }

            scrollView.AddManipulator(scrollBarMarkerManipulator);
        }

        private VisualElement CreateScrollbarMarker(Scroller scroller, int line, bool isSelected)
        {
            float lineHeight = 15 * zoomFactor;
            float documentHeight = cachedLines.Length * lineHeight;
            float normalizedPosition = line * lineHeight / documentHeight;
            float scrollbarHeight = scroller.resolvedStyle.height;
            float buttonHeight = scroller.lowButton.resolvedStyle.height;
            float usableScrollbarHeight = scrollbarHeight - (buttonHeight * 2);
            float position = buttonHeight + (normalizedPosition * usableScrollbarHeight);

            return new VisualElement
            {
                name = "marker" + line,
                style =
                {
                    position = Position.Absolute,
                    top = position - 2,
                    height = 4,
                    width = scroller.resolvedStyle.width,
                    backgroundColor = isSelected
                        ? new Color(0.4f, 0.4f, 0.4f, 0.4f)
                        : new Color(0.4f, 0.4f, 0.4f, 0.4f),
                    borderTopLeftRadius = 2,
                    borderTopRightRadius = 2,
                    borderBottomLeftRadius = 2,
                    borderBottomRightRadius = 2
                },
                pickingMode = PickingMode.Ignore
            };
        }

        private void UpdateScrollBarMarkers(ScrollView scrollView, List<(Label, int)> selectedLines)
        {
            var scroller = scrollView.verticalScroller;
            if (scroller == null) return;

            // Remove existing markers
            foreach (var child in scroller.Children().ToList())
            {
                if (child.name != null && child.name.StartsWith("marker"))
                    scroller.Remove(child);
            }

            // Add new markers
            foreach (var (label, line) in selectedLines)
            {
                var marker = CreateScrollbarMarker(scroller, line, true);
                scroller.Add(marker);
            }

            // Add markers for related lines
            foreach (var label in relatedLabels)
            {
                var lineInfo = labels.FirstOrDefault(l => l.Item1 == label);
                if (lineInfo != default)
                {
                    var marker = CreateScrollbarMarker(scroller, lineInfo.Item2, false);
                    scroller.Add(marker);
                }
            }
        }

        private string LoadCode()
        {
            return CodeGenerator.GetSingleDecorator(asset).Generate(0).RemoveMarkdown();
        }

        private VisualElement CreateLineContent(int lineIndex)
        {
            var container = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    position = Position.Absolute,
                    left = 10 * zoomFactor,
                    top = lineIndex * (15 * zoomFactor),
                    height = 15 * zoomFactor,
                    flexShrink = 0,
                    whiteSpace = WhiteSpace.NoWrap
                }
            };

            if (cachedRegions.TryGetValue(lineIndex, out var regions))
            {
                AdjustLeadingWhitespacesForFirstRegion(cachedLines[lineIndex], regions[0]);
                foreach (var region in regions)
                {
                    var label = CreateCodeLabel(region, lineIndex);
                    labels.Add((label, lineIndex));
                    container.Add(label);

                    if (!unitIDRegions.ContainsKey(region.unitId))
                    {
                        unitIDRegions[region.unitId] = new List<(Label, int)> { (label, lineIndex) };
                    }
                    else
                    {
                        unitIDRegions[region.unitId].Add((label, lineIndex));
                    }
                }
            }
            else
            {
                var label = CreateNonClickableLabel(cachedLines[lineIndex], lineIndex);
                labels.Add((label, lineIndex));
                container.Add(label);
            }

            return container;
        }

        private Label CreateLineNumber(int lineIndex)
        {
            var label = new Label($"{lineIndex + 1}")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = Color.gray,
                    position = Position.Absolute,
                    top = lineIndex * (15 * zoomFactor),
                    right = 0,
                    left = 0,
                    height = 15 * zoomFactor,
                }
            };
            return label;
        }

        private void RefreshVirtualization(ScrollView scrollView, ScrollView lineNumbersScrollView)
        {
            if (cachedLines == null) return;

            virtualizedLines.Clear();
            var content = scrollView.Q("content");
            var lineNumbers = lineNumbersScrollView.Q("lineNumbers");

            content?.Clear();
            lineNumbers?.Clear();

            float lineHeight = 15 * zoomFactor;
            if (content != null)
                content.style.height = cachedLines.Length * lineHeight;
            if (lineNumbers != null)
                lineNumbers.style.height = content.style.height;

            float scrollPos = scrollView.scrollOffset.y;
            float viewportHeight = scrollView.resolvedStyle.height;

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(scrollPos / lineHeight) - BUFFER_LINES);
            int lastVisible = Mathf.Min(cachedLines.Length - 1,
                Mathf.CeilToInt((scrollPos + viewportHeight) / lineHeight) + BUFFER_LINES);

            RenderVisibleLines(scrollView, lineNumbersScrollView, firstVisible, lastVisible);

            SetupScrollbarMarkers(scrollView);
        }

        private void PerformSearch()
        {
            searchResults.Clear();
            currentSearchIndex = -1;

            ClearSearchHighlights();

            if (string.IsNullOrEmpty(searchText))
            {
                UpdateSearchCount();
                return;
            }

            if (cachedLines == null)
            {
                UpdateSearchCount();
                return;
            }

            for (int i = 0; i < cachedLines.Length; i++)
            {
                string line = CodeUtility.RemoveAllSelectableTags(cachedLines[i]);
                line = RemoveColorTags(line);
                int index = line.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);

                while (index != -1)
                {
                    searchResults.Add((i, index, searchText.Length));
                    index = line.IndexOf(searchText, index + 1, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (searchResults.Count > 0)
            {
                currentSearchIndex = 0;
                HighlightCurrentSearch();
            }

            UpdateSearchCount();
        }

        private void ClearSearchHighlights()
        {
            foreach (var (label, _) in labels)
            {
                var text = label.text;
                label.style.backgroundColor = GetOriginalBackgroundColor(label);
                if (text.Contains("<mark class=\"highlight\">"))
                {
                    label.text = RemoveHighlightTags(text);
                }
            }
        }

        private Color GetOriginalBackgroundColor(Label label)
        {
            if (selectedLabels.Any(sl => sl.Item1 == label))
                return new Color(0.25f, 0.5f, 0.8f, 0.3f);
            if (relatedLabels.Contains(label))
                return new Color(0.25f, 0.25f, 0.25f, 0.4f);
            return new Color(1, 1, 1, 0);
        }

        private void NavigateSearch(int direction)
        {
            if (searchResults.Count == 0) return;

            ClearActiveSearchHighlight();
            currentSearchIndex = (currentSearchIndex + direction + searchResults.Count) % searchResults.Count;
            HighlightCurrentSearch();
            UpdateSearchCount();
        }

        private void ClearActiveSearchHighlight()
        {
            if (currentSearchIndex >= 0 && currentSearchIndex < searchResults.Count)
            {
                var (line, index, length) = searchResults[currentSearchIndex];
                foreach (var current in labels)
                {
                    if (current.Item2 == line)
                    {
                        current.label.style.backgroundColor = GetOriginalBackgroundColor(current.label);
                    }
                }
            }
        }

        private void HighlightCurrentSearch()
        {
            if (currentSearchIndex >= 0 && currentSearchIndex < searchResults.Count)
            {
                var (line, startIndex, length) = searchResults[currentSearchIndex];
                GoToLine(line + 1);
                foreach (var current in labels)
                {
                    if (current.Item2 == line)
                    {
                        string text = current.label.text;
                        string cleanText = RemoveHighlightTags(text);

                        var matches = new List<(int index, int length)>();
                        int index = cleanText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                        while (index != -1)
                        {
                            matches.Add((index, searchText.Length));
                            index = cleanText.IndexOf(searchText, index + 1, StringComparison.OrdinalIgnoreCase);
                        }

                        string highlighted = cleanText;
                        int offset = 0;
                        foreach (var (matchIndex, matchLength) in matches)
                        {
                            string before = highlighted.Substring(0, matchIndex + offset);
                            string match = highlighted.Substring(matchIndex + offset, matchLength);
                            string after = highlighted.Substring(matchIndex + offset + matchLength);

                            highlighted = before + $"<mark class=\"highlight\">{match}</mark>" + after;
                            offset += "<mark class=\"highlight\">".Length + "</mark>".Length;
                        }

                        current.label.text = highlighted;
                        current.label.style.backgroundColor = matches.Any(m => m.index == startIndex) ?
                            new Color(1, 1, 0, 0.3f) :
                            new Color(1, 1, 0, 0.1f);
                    }
                }
            }
        }

        private string RemoveHighlightTags(string text)
        {
            return text.Replace("<mark class=\"highlight\">", "").Replace("</mark>", "");
        }

        private void UpdateSearchCount()
        {
            var searchCountLabel = rootVisualElement.Q<Label>("searchCount");
            if (searchCountLabel != null)
            {
                searchCountLabel.text = searchResults.Count > 0
                    ? $"{currentSearchIndex + 1}/{searchResults.Count}"
                    : "0/0";
            }
        }

        private void GoToLine(int lineNumber)
        {
            if (cachedLines == null || lineNumber < 1 || lineNumber > cachedLines.Length)
                return;

            var scrollView = rootVisualElement.Q<ScrollView>("codeContainer");
            if (scrollView == null)
                return;

            float lineHeight = 15 * zoomFactor;
            float targetPosition = (lineNumber - 1) * lineHeight;
            scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, targetPosition);
        }
=======
                window.UpdateCodeDisplay();
                window.Repaint();
            }
        }
>>>>>>> Stashed changes
    }
}