using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class CSharpPreviewWindow : EditorWindow
    {
        public static CSharpPreviewWindow instance;
        private float zoomFactor = 1.0f;
        private bool showCodeWindow = true;
        private List<Label> labels = new List<Label>();

        public static Object asset;

        [MenuItem("Window/Community Addons/C# Preview")]
        public static void Open()
        {
            CSharpPreviewWindow window = GetWindow<CSharpPreviewWindow>();
            window.titleContent = new GUIContent("C# Preview");
            instance = window;
        }

        public void CreateGUI()
        {
            Selection.selectionChanged += ChangeSelection;
            var toolbar = new Toolbar();
            toolbar.name = "Toolbar";

            var codeContainer = new ScrollView
            {
                name = "codeContainer",
                style = { backgroundColor = new Color(0.15f, 0.15f, 0.15f), flexGrow = 1 }
            };

            var settingsContainer = new ScrollView
            {
                name = "settingsContainer",
                style = { backgroundColor = new Color(0.18f, 0.18f, 0.18f), flexGrow = 1 }
            };
            CreateSettingsUI(settingsContainer);

            // Create the Zoom section
            var zoomContainer = new VisualElement
            {
                style =
        {
            flexDirection = FlexDirection.Row,
            alignItems = Align.Center,
            paddingLeft = 10
        }
            };
            var zoomTextLabel = new Label("Zoom:") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 5 } };
            var zoomSlider = new Slider(0.5f, 2.0f)
            {
                value = zoomFactor,
                style = { width = Length.Percent(20), alignSelf = Align.Center }
            };
            var zoomLabel = new Label($"{zoomFactor:0.#}x") { style = { marginLeft = 5, alignSelf = Align.Center } };

            // Register a callback for when the slider value changes
            zoomSlider.RegisterValueChangedCallback(evt =>
            {
                zoomFactor = evt.newValue;
                zoomLabel.text = $"{zoomFactor:0.#}x";
                codeContainer.style.fontSize = Mathf.RoundToInt(14 * zoomFactor);
            });

            zoomContainer.Add(zoomTextLabel);
            zoomContainer.Add(zoomSlider);
            zoomContainer.Add(zoomLabel);

            toolbar.Add(zoomContainer);

            // Toolbar Buttons with toolbar-like styling
            var compileButton = CreateToolbarButton("Compile", () => CompileCode());
            toolbar.Add(compileButton);

            var copyButton = CreateToolbarButton("Copy to Clipboard", CopyToClipboard);
            toolbar.Add(copyButton);

            var utilityButton = CreateToolbarButton("Utility Window", OpenUtilityWindow);
            toolbar.Add(utilityButton);

            var refreshButton = CreateToolbarButton("Refresh", () => UpdateCodeDisplay());
            toolbar.Add(refreshButton);

            var toggleButton = CreateToolbarButton(showCodeWindow ? "Settings" : "Preview", ToggleWindowMode);
            toggleButton.name = "toggleButton";
            toolbar.Add(toggleButton);

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(codeContainer);
            rootVisualElement.Add(settingsContainer);
            ChangeSelection();

            codeContainer.style.display = showCodeWindow ? DisplayStyle.Flex : DisplayStyle.None;
            settingsContainer.style.display = showCodeWindow ? DisplayStyle.None : DisplayStyle.Flex;
        }
        private void CreateSettingsUI(ScrollView settingsContainer)
        {
            var path = "Assets/Unity.VisualScripting.Community.Generated/";
            HUMIO.Ensure(path).Path();
            CSharpPreviewSettings settings = AssetDatabase.LoadAssetAtPath<CSharpPreviewSettings>(path + "CSharpPreviewSettings.asset");

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<CSharpPreviewSettings>();
                settings.name = "CSharpPreviewSettings";
                AssetDatabase.CreateAsset(settings, path + "CSharpPreviewSettings.asset");
                settings.Initalize();
            }
            else if (!settings.isInitalized)
            {
                settings.Initalize();
            }
            CodeBuilder.ShowRecommendations = settings.ShowRecommendations;

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


            #region Generation Settings
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
                tooltip = "Generate a comment where the Subgraph and Port are being generated.",
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10, width = labelWidth }
            };

            var showSubgraphCommentToggle = new Toggle
            {
                style = { marginLeft = 10 }
            };
            showSubgraphCommentToggle.value = settings.ShowSubgraphComment;
            CSharpPreview.ShowSubgraphComment = settings.ShowSubgraphComment;

            showSubgraphCommentToggle.RegisterValueChangedCallback(evt =>
            {
                settings.ShowSubgraphComment = evt.newValue;
                CSharpPreview.ShowSubgraphComment = evt.newValue;
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
                style = { marginLeft = 10 }
            };

            showRecommendationToggle.value = settings.ShowRecommendations;
            CodeBuilder.ShowRecommendations = settings.ShowRecommendations;
            showRecommendationToggle.RegisterValueChangedCallback(evt =>
            {
                settings.ShowRecommendations = evt.newValue;
                CodeBuilder.ShowRecommendations = evt.newValue;
                settings.SaveAndDirty();
            });

            recommendationToggleContainer.Add(showRecommendationLabel);
            recommendationToggleContainer.Add(showRecommendationToggle);
            generationSettingsSection.Add(recommendationToggleContainer);

            settingsContainer.Add(generationSettingsSection);
            #endregion

            #region Syntax Highlights
            var syntaxHighlightsSection = new VisualElement
            {
                style = { marginLeft = 10, marginTop = 10 }
            };

            var syntaxHighlightsLabel = new Label("Syntax Highlights")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14, marginTop = 10 }
            };
            syntaxHighlightsSection.Add(syntaxHighlightsLabel);

            float labelsWidth = 200; // Fixed width for label alignment

            void AddColorField(Action initialize, VisualElement container, string labelText, string tooltip, Color initialColor, Action<Color> onColorChanged, Action<ColorField> resetToDefault)
            {
                initialize();

                var colorContainer = new VisualElement
                {
                    style = { marginTop = 10, flexDirection = FlexDirection.Row }
                };

                var colorLabel = new Label(labelText)
                {
                    tooltip = tooltip,
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10, width = labelsWidth }
                };

                var colorField = new ColorField
                {
                    style = { marginLeft = 10, width = 400 },
                    value = initialColor
                };

                colorField.RegisterValueChangedCallback(evt =>
                {
                    onColorChanged(evt.newValue);
                    settings.SaveAndDirty();
                });

                var defaultButton = new Button(() =>
                {
                    resetToDefault(colorField);
                    settings.SaveAndDirty();
                })
                {
                    text = "Default",
                    style = { marginLeft = 10 }
                };

                colorContainer.Add(colorLabel);
                colorContainer.Add(colorField);
                colorContainer.Add(defaultButton);

                container.Add(colorContainer);
            }

            AddColorField(() => CodeBuilder.VariableColor = settings.VariableColor.ToHexString(), syntaxHighlightsSection, "Variable Color :", "The variable color.", settings.VariableColor, color =>
            {
                settings.VariableColor = color;
                CodeBuilder.VariableColor = color.ToHexString();
                settings.SaveAndDirty();
            }, (field) =>
            {
                if (UnityEngine.ColorUtility.TryParseHtmlString("#00FFFF", out var value))
                {
                    settings.VariableColor = value;
                    field.value = value;
                }
                CodeBuilder.VariableColor = "00FFFF";
            });

            AddColorField(() => CodeBuilder.StringColor = settings.StringColor.ToHexString(), syntaxHighlightsSection, "String Color :", "The color for strings.", settings.StringColor, color =>
            {
                settings.StringColor = color;
                CodeBuilder.StringColor = color.ToHexString();
                settings.SaveAndDirty();
            }, (field) =>
            {
                if (UnityEngine.ColorUtility.TryParseHtmlString("#CC8833", out var value))
                {
                    settings.StringColor = value;
                    field.value = value;
                }
                CodeBuilder.StringColor = "CC8833";
            });

            AddColorField(() => CodeBuilder.NumericColor = settings.NumericColor.ToHexString(), syntaxHighlightsSection, "Numeric Color :", "The color for numeric values.", settings.NumericColor, color =>
            {
                settings.NumericColor = color;
                CodeBuilder.NumericColor = color.ToHexString();
                settings.SaveAndDirty();
            }, (field) =>
            {
                if (UnityEngine.ColorUtility.TryParseHtmlString("#DDFFBB", out var value))
                {
                    settings.NumericColor = value;
                    field.value = value;
                }
                CodeBuilder.NumericColor = "DDFFBB";
            });

            AddColorField(() => CodeBuilder.ConstructColor = settings.ConstructColor.ToHexString(), syntaxHighlightsSection, "Construct Color :", "The color for constructs (e.g., loops, conditionals).", settings.ConstructColor, color =>
            {
                settings.ConstructColor = color;
                CodeBuilder.ConstructColor = color.ToHexString();
                settings.SaveAndDirty();
            }, (field) =>
            {
                if (UnityEngine.ColorUtility.TryParseHtmlString("#4488FF", out var value))
                {
                    settings.ConstructColor = value;
                    field.value = value;
                }
                CodeBuilder.ConstructColor = "4488FF";
            });

            AddColorField(() => CodeBuilder.TypeColor = settings.TypeColor.ToHexString(), syntaxHighlightsSection, "Type Color :", "The color for data types.", settings.TypeColor, color =>
            {
                settings.TypeColor = color;
                CodeBuilder.TypeColor = color.ToHexString();
                settings.SaveAndDirty();
            }, (field) =>
            {
                if (UnityEngine.ColorUtility.TryParseHtmlString("#33EEAA", out var value))
                {
                    settings.TypeColor = value;
                    field.value = value;
                }
                CodeBuilder.TypeColor = "33EEAA";
            });

            AddColorField(() => CodeBuilder.EnumColor = settings.EnumColor.ToHexString(), syntaxHighlightsSection, "Enum Color :", "The color for enums.", settings.EnumColor, color =>
            {
                settings.EnumColor = color;
                CodeBuilder.EnumColor = color.ToHexString();
                settings.SaveAndDirty();
            }, (field) =>
            {
                if (UnityEngine.ColorUtility.TryParseHtmlString("#FFFFBB", out var value))
                {
                    settings.EnumColor = value;
                    field.value = value;
                }
                CodeBuilder.EnumColor = "FFFFBB";
            });

            AddColorField(() => CodeBuilder.InterfaceColor = settings.InterfaceColor.ToHexString(), syntaxHighlightsSection, "Interface Color :", "The color for interfaces.", settings.InterfaceColor, color =>
            {
                settings.InterfaceColor = color;
                CodeBuilder.InterfaceColor = color.ToHexString();
                settings.SaveAndDirty();
            }, (field) =>
            {
                if (UnityEngine.ColorUtility.TryParseHtmlString("#DDFFBB", out var value))
                {
                    settings.InterfaceColor = value;
                    field.value = value;
                }
                CodeBuilder.InterfaceColor = "DDFFBB";
            });

            settingsContainer.Add(syntaxHighlightsSection);
            #endregion
        }

        private Button CreateToolbarButton(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.style.paddingLeft = 6;
            button.style.paddingRight = 6;
            button.style.marginLeft = 0;
            button.style.marginRight = 0;
            button.style.paddingTop = 4;
            button.style.paddingBottom = 4;
            button.style.backgroundColor = new Color(0, 0, 0, 0); // Default background color
            button.style.borderTopColor = new Color(0, 0, 0, 0);
            button.style.borderBottomColor = new Color(0, 0, 0, 0);
            button.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f);
            button.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 0;
            button.style.color = Color.white;
            button.style.borderTopLeftRadius = 0;
            button.style.borderBottomLeftRadius = 0;
            button.style.borderTopRightRadius = 0;
            button.style.borderBottomRightRadius = 0;

            // Hover effects
            var defaultBackgroundColor = button.style.backgroundColor.value;
            var hoverBackgroundColor = new Color(0.15f, 0.15f, 0.15f); // Darker color on hover

            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                button.style.backgroundColor = hoverBackgroundColor;
            });

            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.style.backgroundColor = defaultBackgroundColor;
            });

            return button;
        }

        private void CompileCode()
        {
            if (asset != null)
                AssetCompiler.CompileAsset(asset);
        }

        // Copy selected text to clipboard
        private void CopyToClipboard()
        {
            string outputToCopy;
            if (selectedLabels.Count > 0)
            {
                // Join text from selected labels
                outputToCopy = string.Join("\n", selectedLabels.Select(label => CodeUtility.RemoveAllSelectableTags(CodeUtility.RemoveCustomHighlights(RemoveColorTags(label.text)))));
            }
            else
            {
                // No selection: copy the entire code
                outputToCopy = CodeUtility.RemoveAllSelectableTags(CodeUtility.RemoveCustomHighlights(RemoveColorTags(LoadYourCode())));
            }
            EditorGUIUtility.systemCopyBuffer = outputToCopy;
        }


        private string RemoveColorTags(string input)
        {
            return Regex.Replace(input, @"<color=#[0-9a-fA-F]{6,8}>|</color>", string.Empty, RegexOptions.Compiled);
        }

        private void OpenUtilityWindow()
        {
            UtilityWindow.Open();
        }

        private void ToggleWindowMode()
        {
            showCodeWindow = !showCodeWindow;

            // Update button label
            var toggleButton = rootVisualElement.Q<Toolbar>("Toolbar").Q<Button>("toggleButton");
            toggleButton.text = showCodeWindow ? "Settings" : "Code View";

            // Toggle visibility of settings and code containers
            var codeContainer = rootVisualElement.Q<ScrollView>("codeContainer");
            var settingsContainer = rootVisualElement.Q<ScrollView>("settingsContainer");

            codeContainer.style.display = showCodeWindow ? DisplayStyle.Flex : DisplayStyle.None;
            settingsContainer.style.display = showCodeWindow ? DisplayStyle.None : DisplayStyle.Flex;

            // Optional: Refresh the code display when toggling back to code view
            if (showCodeWindow) UpdateCodeDisplay();
        }

        private void ChangeSelection()
        {
            if (Selection.activeObject is CodeAsset _asset)
            {
                asset = _asset;
                UpdateCodeDisplay();
            }
            else if (Selection.activeObject is ScriptGraphAsset _graphAsset)
            {
                asset = _graphAsset;
                UpdateCodeDisplay();
            }
        }

        public void UpdateCodeDisplay()
        {
            if (asset != null)
            {
                var code = LoadYourCode(); // Load your code based on the selected asset
                var scrollView = rootVisualElement.Q<ScrollView>(); // Get the existing scroll view
                DisplayCode(scrollView, code); // Update the displayed code
            }
        }

        private void DisplayCode(ScrollView scrollView, string code)
        {
            scrollView.Clear();
            labels.Clear();

            var clickableRegions = CodeUtility.ExtractClickableRegions(code);
            var regionsByLine = clickableRegions
                .GroupBy(region => region.startLine)
                .ToDictionary(g => g.Key, g => g.ToList());

            var lines = CodeUtility.RemoveAllSelectableTags(code).Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var lineContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                var lineNumberContainer = new Label($"{i + 1}");
                lineNumberContainer.style.width = 25;
                lineNumberContainer.style.unityTextAlign = TextAnchor.MiddleLeft;
                lineNumberContainer.style.color = Color.gray;
                lineNumberContainer.style.marginLeft = 0;
                lineNumberContainer.style.paddingLeft = 0;
                lineContainer.Add(lineNumberContainer);
                labels.Add(lineNumberContainer);


                // Code container for the actual code
                var codeContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1, marginLeft = 10 } };

                // Check for clickable regions on the line
                if (regionsByLine.TryGetValue(i, out var regions))
                {
                    AdjustLeadingWhitespacesForFirstRegion(lines[i], regions[0]);
                    foreach (var region in regions)
                    {
                        var label = CreateCodeLabel(region, i);
                        labels.Add(label);
                        codeContainer.Add(label);
                    }
                }
                else
                {
                    var label = CreateNonClickableLabel(lines[i]);
                    labels.Add(label);
                    codeContainer.Add(label);
                }

                // Add the code container to the line container and then to ScrollView
                lineContainer.Add(codeContainer);
                scrollView.Add(lineContainer);
            }
        }

        private void AdjustLeadingWhitespacesForFirstRegion(string line, ClickableRegion firstRegion)
        {
            int leadingWhitespaceLength = 0;

            while (leadingWhitespaceLength < line.Length && char.IsWhiteSpace(line[leadingWhitespaceLength]))
            {
                leadingWhitespaceLength++;
            }

            if (!firstRegion.code.StartsWith(line.AsSpan(0, leadingWhitespaceLength).ToString()))
            {
                firstRegion.code = line.Substring(0, leadingWhitespaceLength) + firstRegion.code;
            }
        }

        private void OnCodeRegionClicked(ClickableRegion region, int currentLine)
        {
            HandleClickableRegionClick(region.unitId, currentLine);
        }

        private List<Label> selectedLabels = new List<Label>();
        private Label lastSelectedLabel = null;

        // Create a non-clickable label
        private Label CreateNonClickableLabel(string text)
        {
            var label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
            label.enableRichText = true;
            label.style.color = Color.white;
            label.style.backgroundColor = new Color(1, 1, 1, 0);
            RemovePaddingAndMargin(label);
            label.RegisterCallback<ClickEvent>(evt => SelectLabel(label, evt));
            return label;
        }

        // Create a clickable code label
        private Label CreateCodeLabel(ClickableRegion region, int currentLine)
        {
            string tooltip;
            var codeWithoutTooltip = CodeUtility.ExtractTooltip(region.code, out tooltip);
            var label = new Label(CodeUtility.RemoveAllSelectableTags(codeWithoutTooltip))
            {
                tooltip = tooltip
            };
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
            label.style.color = Color.white;
            label.enableRichText = true;
            label.style.backgroundColor = new Color(1, 1, 1, 0);
            RemovePaddingAndMargin(label);

            label.RegisterCallback<ClickEvent>(evt =>
            {
                SelectLabel(label, evt);
                OnCodeRegionClicked(region, currentLine);
            });

            return label;
        }
        private void RemovePaddingAndMargin(Label label)
        {
            label.style.paddingLeft = 0;
            label.style.paddingRight = 0;
            label.style.paddingTop = 0;
            label.style.paddingBottom = 0;
            label.style.marginLeft = 0;
            label.style.marginRight = 0;
            label.style.marginTop = 0;
            label.style.marginBottom = 0;
        }

        // Handle label selection with Ctrl and Shift keys
        private void SelectLabel(Label label, ClickEvent evt)
        {
            if (evt.ctrlKey)
            {
                // Ctrl + Click: Add/remove label from selection
                if (selectedLabels.Contains(label))
                {
                    DeselectLabel(label);
                }
                else
                {
                    AddLabelToSelection(label);
                }
            }
            else if (evt.shiftKey && lastSelectedLabel != null)
            {
                // Shift + Click: Select a range
                SelectRange(label);
            }
            else
            {
                // No modifier: clear selection and select only the clicked label
                ClearSelection();
                AddLabelToSelection(label);
            }

            lastSelectedLabel = label;
        }

        // Add label to the selection
        private void AddLabelToSelection(Label label)
        {
            label.style.backgroundColor = new Color(0.25f, 0.5f, 0.8f, 0.3f); // Highlight color

            selectedLabels.Add(label);
        }

        // Deselect a label
        private void DeselectLabel(Label label)
        {
            label.style.backgroundColor = new Color(1, 1, 1, 0); // Default background
            selectedLabels.Remove(label);
        }

        // Clear all selected labels
        private void ClearSelection()
        {
            foreach (var selectedLabel in selectedLabels)
            {
                selectedLabel.style.backgroundColor = new Color(1, 1, 1, 0);
            }
            selectedLabels.Clear();
        }

        // Select a range between lastSelectedLabel and current label
        private void SelectRange(Label label)
        {
            int startIndex = labels.IndexOf(lastSelectedLabel);
            int endIndex = labels.IndexOf(label);

            if (startIndex > endIndex)
            {
                int temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                var labelInRange = labels[i];
                if (!selectedLabels.Contains(labelInRange))
                {
                    AddLabelToSelection(labelInRange);
                }
            }
        }

        private string LoadYourCode()
        {
            return CodeGenerator.GetSingleDecorator(asset).Generate(0).RemoveMarkdown();
        }

        private void HandleClickableRegionClick(string unitId, int line)
        {
            var code = CodeGenerator.GetSingleDecorator(asset);
            if (GraphWindow.active?.reference != null && GraphWindow.active.context.graph is FlowGraph)
            {
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
                    units = TraverseFlowGraph(reference).ToList();
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
                    var path = GetUnitPath(ordered.First().result.Item1);
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
                        GraphWindow.active.context.EndEdit();
                    }
                }
            }
            else
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
                        GraphWindow.OpenActive(ordered.First().result.Item1);
                        HandleClickableRegionClick(unitId, line);
                    }
                }
                else if (code is ScriptGraphAssetGenerator graphAssetGenerator)
                {
                    if (graphAssetGenerator.Data != null)
                    {
                        List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                        units = TraverseFlowGraph(graphAssetGenerator.Data.GetReference() as GraphReference).ToList();
                        var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                        GraphWindow.OpenActive(ordered.First().result.Item1);
                    }
                }
            }
        }

        private void ProcessMethodDeclaration(MethodDeclaration methodDeclaration, List<(GraphReference, Unit)> units)
        {
            if (methodDeclaration.classAsset != null)
            {
                AddUnitsFromClassAsset(methodDeclaration.classAsset, units);
            }
            else if (methodDeclaration.structAsset != null)
            {
                AddUnitsFromStructAsset(methodDeclaration.structAsset, units);
            }
        }

        private void ProcessConstructorDeclaration(ConstructorDeclaration constructorDeclaration, List<(GraphReference, Unit)> units)
        {
            if (constructorDeclaration.classAsset != null)
            {
                AddUnitsFromClassAsset(constructorDeclaration.classAsset, units);
            }
            else if (constructorDeclaration.structAsset != null)
            {
                AddUnitsFromStructAsset(constructorDeclaration.structAsset, units);
            }
        }

        private void ProcessFieldDeclaration(FieldDeclaration fieldDeclaration, List<(GraphReference, Unit)> units)
        {
            if (fieldDeclaration.classAsset != null)
            {
                AddUnitsFromClassAsset(fieldDeclaration.classAsset, units);
            }
            else if (fieldDeclaration.structAsset != null)
            {
                AddUnitsFromStructAsset(fieldDeclaration.structAsset, units);
            }
        }

        private void AddUnitsFromClassAsset(ClassAsset classAsset, List<(GraphReference, Unit)> units)
        {
            foreach (var method in classAsset.methods)
                units.AddRange(TraverseFlowGraph(method.GetReference() as GraphReference));

            foreach (var constructor in classAsset.constructors)
                units.AddRange(TraverseFlowGraph(constructor.GetReference() as GraphReference));

            foreach (var variable in classAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(TraverseFlowGraph(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(TraverseFlowGraph(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        private void AddUnitsFromStructAsset(StructAsset structAsset, List<(GraphReference, Unit)> units)
        {
            foreach (var method in structAsset.methods)
                units.AddRange(TraverseFlowGraph(method.GetReference() as GraphReference));

            foreach (var constructor in structAsset.constructors)
                units.AddRange(TraverseFlowGraph(constructor.GetReference() as GraphReference));

            foreach (var variable in structAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(TraverseFlowGraph(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(TraverseFlowGraph(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        List<(GraphReference, SubgraphUnit)> GetUnitPath(GraphReference reference)
        {
            List<(GraphReference, SubgraphUnit)> nodePath = new List<(GraphReference, SubgraphUnit)>() { (reference, !reference.isRoot ? reference.GetParent<SubgraphUnit>() : null) };
            while (reference.ParentReference(false) != null)
            {
                reference = reference.ParentReference(false);
                nodePath.Add((reference, !reference.isRoot ? reference.GetParent<SubgraphUnit>() : null));
            }
            nodePath.Reverse();
            return nodePath;
        }

        IEnumerable<(GraphReference, Unit)> TraverseFlowGraph(GraphReference graphReference)
        {
            var flowGraph = graphReference.graph as FlowGraph;
            if (flowGraph == null) yield break;
            var units = flowGraph.units;
            foreach (var element in units)
            {
                var unit = element as Unit;
                switch (unit)
                {
                    case SubgraphUnit subgraphUnit:
                        {
                            var subGraph = subgraphUnit.nest.embed ?? subgraphUnit.nest.graph;
                            if (subGraph == null) continue;
                            yield return (graphReference, subgraphUnit);
                            var childReference = graphReference.ChildReference(subgraphUnit, false);
                            foreach (var item in TraverseFlowGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    case StateUnit stateUnit:
                        {
                            var stateGraph = stateUnit.nest.embed ?? stateUnit.nest.graph;
                            if (stateGraph == null) continue;
                            // find state graph.
                            var childReference = graphReference.ChildReference(stateUnit, false);
                            foreach (var item in TraverseStateGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    default:
                        yield return (graphReference, unit);
                        break;
                }
            }
        }

        IEnumerable<(GraphReference, Unit)> TraverseStateGraph(GraphReference graphReference)
        {
            var stateGraph = graphReference.graph as StateGraph;
            if (stateGraph == null) yield break;

            foreach (var state in stateGraph.states)
            {
                switch (state)
                {
                    case FlowState flowState:
                        {
                            var graph = flowState.nest.embed ?? flowState.nest.graph;

                            if (graph == null) continue;
                            var childReference = graphReference.ChildReference(flowState, false);
                            foreach (var item in TraverseFlowGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    case SuperState superState:
                        {
                            var subStateGraph = superState.nest.embed ?? superState.nest.graph;
                            if (subStateGraph == null) continue;
                            var childReference = graphReference.ChildReference(superState, false);
                            foreach (var item in TraverseStateGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    case AnyState:
                        continue;
                }
            }

            foreach (var transition in stateGraph.transitions)
            {
                if (transition is not FlowStateTransition flowStateTransition) continue;
                var graph = flowStateTransition.nest.embed ?? flowStateTransition.nest.graph;
                if (graph == null) continue;
                var childReference = graphReference.ChildReference(flowStateTransition, false);
                foreach (var item in TraverseFlowGraph(childReference))
                {
                    yield return item;
                }
            }
        }

        private GraphReference GetFirstReference()
        {
            if (asset is ClassAsset classAsset)
            {
                var variables = classAsset.variables.Where(variable => variable.isProperty);
                if (classAsset.constructors.Count > 0)
                {
                    return classAsset.constructors[0].GetReference() as GraphReference;
                }
                else if (variables.Count() > 0)
                {
                    var first = variables.First();
                    if (first.get)
                    {
                        return first.getter.GetReference() as GraphReference;
                    }
                    else if (first.set)
                    {
                        return first.setter.GetReference() as GraphReference;
                    }
                }
                else if (classAsset.methods.Count > 0)
                {
                    return classAsset.methods[0].GetReference() as GraphReference;
                }
            }
            else if (asset is StructAsset structAsset)
            {
                var variables = structAsset.variables.Where(variable => variable.isProperty);
                if (structAsset.constructors.Count > 0)
                {
                    return structAsset.constructors[0].GetReference() as GraphReference;
                }
                else if (variables.Count() > 0)
                {
                    var first = variables.First();
                    if (first.get)
                    {
                        return first.getter.GetReference() as GraphReference;
                    }
                    else if (first.set)
                    {
                        return first.setter.GetReference() as GraphReference;
                    }
                }
                else if (structAsset.methods.Count > 0)
                {
                    return structAsset.methods[0].GetReference() as GraphReference;
                }
            }
            return null;
        }

    }
}