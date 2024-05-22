using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    public class NodeFinderWindow : EditorWindow
    {
        enum MatchType
        {
            Unit,
            Error
        }

        class MatchObject
        {
            public List<MatchType> Matches;
            public ScriptGraphAsset ScriptGraphAsset;
            public ScriptMachine ScriptMachine;
            public StateGraphAsset StateGraphAsset;
            public GraphReference Reference;
            public string FullTypeName;
            public IUnit Unit;
        }

        private string _pattern = "";
        private string _previousPattern = ""; // Store the previous text input

        private bool _matchError = true;
        private bool _checkScriptGraphAssets = true;
        private bool _checkStateGraphAssets = true;
        private bool _checkScriptMachines = true;
        private List<MatchObject> _matchObjects = new();
        private Dictionary<ScriptGraphAsset, List<MatchObject>> _matchScriptGraphMap = new();
        private Dictionary<ScriptMachine, List<MatchObject>> _matchScriptMachineMap = new();
        private List<ScriptGraphAsset> _sortedScriptGraphKey = new();
        private List<ScriptMachine> _sortedScriptMachineKey = new();
        private Dictionary<StateGraphAsset, List<MatchObject>> _matchStateGraphMap = new();
        private List<StateGraphAsset> _sortedStateGraphKey = new();
        private float errorCheckInterval = 1.0f;
        private float lastErrorCheckTime;

        // scroll view position
        private Vector2 _scrollViewRoot;


        [MenuItem("Window/Community Addons/Node Finder")]
        public static void Open()
        {
            var window = GetWindow<NodeFinderWindow>();
            window.titleContent = new GUIContent("Node finder");
        }

        private void OnDisable()
        {
            _matchObjects.Clear();
            _matchScriptGraphMap.Clear();
            _sortedScriptGraphKey.Clear();
            _matchScriptMachineMap.Clear();
            _sortedScriptMachineKey.Clear();
            _matchStateGraphMap.Clear();
            _sortedStateGraphKey.Clear();
        }

        private void OnEnable()
        {
            _previousPattern = _pattern;
            Search();
        }

        private void OnGUI()
        {
            Event e = Event.current;
            DrawSearchBar();
            GUILayout.Space(6);
            DrawFilters();
            GUILayout.Space(6);
            if (e.keyCode == KeyCode.Return)
            {
                Search();
            }

            if (_pattern != _previousPattern)
            {
                Search();
                _previousPattern = _pattern; // Update the previous pattern
            }

            if (Time.realtimeSinceStartup - lastErrorCheckTime >= errorCheckInterval)
            {
                if (_matchError)
                    SearchForErrors();
                lastErrorCheckTime = Time.realtimeSinceStartup;
            }

            DrawResults();
        }

        private void DrawSearchBar()
        {
            var findLabelStyle = new GUIStyle(LudiqStyles.toolbarLabel)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                {
                    HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, 7, () =>
                                          {
                                              EditorGUILayout.LabelField("Find:", findLabelStyle, GUILayout.Width(40));
                                              _pattern = EditorGUILayout.TextField(_pattern, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                                          }, false, false);
                });
        }

        private void DrawFilters()
        {
            var filterLabelStyle = new GUIStyle(LudiqStyles.toolbarLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
               {
                   GUILayout.Label("Filters:", filterLabelStyle);
                   HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, 7, () =>
                                         {
                                             bool prevCheckScriptGraphAssets = _checkScriptGraphAssets;
                                             bool prevCheckStateGraphAssets = _checkStateGraphAssets;
                                             bool prevCheckScriptMachines = _checkScriptMachines;
                                             bool prevMatchError = _matchError;

                                             _checkScriptGraphAssets = GUILayout.Toggle(_checkScriptGraphAssets, "ScriptGraphAssets", EditorStyles.toolbarButton);
                                             _checkStateGraphAssets = GUILayout.Toggle(_checkStateGraphAssets, "StateGraphAssets", EditorStyles.toolbarButton);
                                             _checkScriptMachines = GUILayout.Toggle(_checkScriptMachines, "ScriptMachines", EditorStyles.toolbarButton);
                                             _matchError = GUILayout.Toggle(_matchError, "Errors", EditorStyles.toolbarButton);


                                             if (_checkScriptGraphAssets != prevCheckScriptGraphAssets)
                                             {
                                                 Search();
                                             }

                                             if (_checkStateGraphAssets != prevCheckStateGraphAssets)
                                             {
                                                 Search();
                                             }

                                             if (_checkScriptMachines != prevCheckScriptMachines)
                                             {
                                                 Search();
                                             }

                                             if (_matchError != prevMatchError)
                                             {
                                                 if (_matchError) SearchForErrors();
                                             }

                                         }, false, false);
               });
        }

        private void DrawResults()
        {
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
            {
                _scrollViewRoot = EditorGUILayout.BeginScrollView(_scrollViewRoot);

                bool empty = string.IsNullOrEmpty(_pattern) || _matchObjects.Count == 0;
                bool isShowingErrors = false;

                // Display Script Graph results
                if (!empty)
                {
                    foreach (var key in _sortedScriptGraphKey)
                    {
                        var list = _matchScriptGraphMap[key];
                        if (!ShouldShowItem(list)) continue;

                        EditorGUIUtility.SetIconSize(new Vector2(16, 16));
                        var icon = EditorGUIUtility.ObjectContent(key, typeof(ScriptGraphAsset));
                        var headerStyle = new GUIStyle(LudiqStyles.toolbarLabel)
                        {
                            fontStyle = FontStyle.Bold,
                            fontSize = 14,
                            alignment = TextAnchor.MiddleLeft,
                            richText = true
                        };
                        GUILayout.Label(new GUIContent(key.name, icon.image), headerStyle);

                        foreach (var match in list)
                        {
                            var pathNames = GetUnitPath(match.Reference);
                            if (match.Matches.Contains(MatchType.Error) && _matchError)
                            {
                                isShowingErrors = true;
                                var label = $"      {pathNames} <color=#FF6800>{SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}</color>";

                                // Create the GUIStyle and enable rich text
                                var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                {
                                    alignment = TextAnchor.MiddleLeft,
                                    richText = true // Enable rich text
                                };

                                // Display the button with the formatted label
                                if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                {
                                    FocusMatchObject(match);
                                }
                            }
                            else
                            {
                                var label = $"      {pathNames} {SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}";
                                var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                {
                                    alignment = TextAnchor.MiddleLeft,
                                    richText = true
                                };

                                if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                {
                                    FocusMatchObject(match);
                                }
                            }
                        }
                    }

                    // Display ScriptMachine Graph Results
                    foreach (var key in _sortedScriptMachineKey)
                    {
                        var list = _matchScriptMachineMap[key];
                        if (!ShouldShowItem(list)) continue;
                        isShowingErrors = true;
                        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

                        // Using GameObject's default icon
                        var icon = EditorGUIUtility.IconContent("GameObject Icon");
                        var headerStyle = new GUIStyle(LudiqStyles.toolbarLabel)
                        {
                            fontStyle = FontStyle.Bold,
                            fontSize = 14,
                            alignment = TextAnchor.MiddleLeft,
                            richText = true
                        };

                        GUILayout.Label(new GUIContent(key.name, icon.image), headerStyle);

                        foreach (var match in list)
                        {
                            var pathNames = GetUnitPath(match.Reference);
                            if (match.Matches.Contains(MatchType.Error) && _matchError)
                            {
                                isShowingErrors = true;
                                var label = $"      {pathNames} <color=#FF6800>{SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}</color>";

                                // Create the GUIStyle and enable rich text
                                var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                {
                                    alignment = TextAnchor.MiddleLeft,
                                    richText = true // Enable rich text
                                };

                                // Display the button with the formatted label
                                if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                {
                                    FocusMatchObject(match);
                                }
                            }
                            else
                            {
                                var label = $"      {pathNames} {SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}";
                                var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                {
                                    alignment = TextAnchor.MiddleLeft,
                                    richText = true
                                };

                                if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                {
                                    FocusMatchObject(match);
                                }
                            }
                        }
                    }

                    // Display State Graph results
                    foreach (var key in _sortedStateGraphKey)
                    {
                        var list = _matchStateGraphMap[key];
                        if (!ShouldShowItem(list)) continue;

                        EditorGUIUtility.SetIconSize(new Vector2(16, 16));
                        var icon = EditorGUIUtility.ObjectContent(key, typeof(StateGraphAsset));
                        var headerStyle = new GUIStyle(LudiqStyles.toolbarLabel)
                        {
                            fontStyle = FontStyle.Bold,
                            fontSize = 14,
                            alignment = TextAnchor.MiddleLeft,
                            richText = true
                        };
                        GUILayout.Label(new GUIContent(key.name, icon.image), headerStyle);

                        foreach (var match in list)
                        {
                            var pathNames = GetUnitPath(match.Reference);
                            if (match.Matches.Contains(MatchType.Error) && _matchError)
                            {
                                isShowingErrors = true;
                                var label = $"      {pathNames} <color=#FF6800>{SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}</color>";

                                var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                {
                                    alignment = TextAnchor.MiddleLeft,
                                    richText = true
                                };

                                if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                {
                                    FocusMatchObject(match);
                                }
                            }
                            else
                            {
                                var label = $"      {pathNames} {SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}";
                                var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                {
                                    alignment = TextAnchor.MiddleLeft,
                                    richText = true
                                };

                                if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                {
                                    FocusMatchObject(match);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_checkScriptGraphAssets)
                    {
                        foreach (var key in _sortedScriptGraphKey)
                        {
                            var list = _matchScriptGraphMap[key];
                            if (!IsError(list)) continue;
                            isShowingErrors = true;
                            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
                            var icon = EditorGUIUtility.ObjectContent(key, typeof(ScriptGraphAsset));
                            var headerStyle = new GUIStyle(LudiqStyles.toolbarLabel)
                            {
                                fontStyle = FontStyle.Bold,
                                fontSize = 14,
                                alignment = TextAnchor.MiddleLeft,
                                richText = true
                            };
                            GUILayout.Label(new GUIContent(key.name, icon.image), headerStyle);

                            foreach (var match in list)
                            {
                                if (match.Matches.Contains(MatchType.Error))
                                {
                                    var pathNames = GetUnitPath(match.Reference);

                                    var label = $"      {pathNames} <color=#FF6800>{SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}</color>";

                                    // Create the GUIStyle and enable rich text
                                    var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                    {
                                        alignment = TextAnchor.MiddleLeft,
                                        richText = true // Enable rich text
                                    };

                                    // Display the button with the formatted label
                                    if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                    {
                                        FocusMatchObject(match);
                                    }
                                }
                            }
                        }
                    }

                    if (_checkScriptMachines)
                    {
                        // Display ScriptMachine Graph Results
                        foreach (var key in _sortedScriptMachineKey)
                        {
                            var list = _matchScriptMachineMap[key];
                            if (!IsError(list)) continue;
                            isShowingErrors = true;
                            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

                            // Using GameObject's default icon
                            var icon = EditorGUIUtility.IconContent("GameObject Icon");
                            var headerStyle = new GUIStyle(LudiqStyles.toolbarLabel)
                            {
                                fontStyle = FontStyle.Bold,
                                fontSize = 14,
                                alignment = TextAnchor.MiddleLeft,
                                richText = true
                            };
                            GUILayout.Label(new GUIContent(key.name, icon.image), headerStyle);

                            foreach (var match in list)
                            {
                                if (match.Matches.Contains(MatchType.Error))
                                {
                                    var pathNames = GetUnitPath(match.Reference);

                                    var label = $"      {pathNames} <color=#FF6800>{SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}</color>";

                                    var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                    {
                                        alignment = TextAnchor.MiddleLeft,
                                        richText = true
                                    };

                                    if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                    {
                                        FocusMatchObject(match);
                                    }
                                }
                            }
                        }
                    }

                    if (_checkStateGraphAssets)
                    {
                        // Display State Graph results
                        foreach (var key in _sortedStateGraphKey)
                        {
                            var list = _matchStateGraphMap[key];
                            if (!IsError(list)) continue;
                            isShowingErrors = true;
                            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
                            var icon = EditorGUIUtility.ObjectContent(key, typeof(StateGraphAsset));
                            var headerStyle = new GUIStyle(LudiqStyles.toolbarLabel)
                            {
                                fontStyle = FontStyle.Bold,
                                fontSize = 14,
                                alignment = TextAnchor.MiddleLeft,
                                richText = true
                            };
                            GUILayout.Label(new GUIContent(key.name, icon.image), headerStyle);

                            foreach (var match in list)
                            {
                                if (match.Matches.Contains(MatchType.Error))
                                {
                                    var pathNames = GetUnitPath(match.Reference);

                                    var label = $"      {pathNames} <color=#FF6800>{SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}</color>";

                                    var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
                                    {
                                        alignment = TextAnchor.MiddleLeft,
                                        richText = true
                                    };

                                    if (GUILayout.Button(new GUIContent(label, GetUnitIcon((Unit)match.Unit)), pathStyle))
                                    {
                                        FocusMatchObject(match);
                                    }
                                }
                            }
                        }
                    }
                }

                if (empty && !isShowingErrors)
                {
                    EditorGUILayout.HelpBox("No results found.", MessageType.Info);
                }

                EditorGUILayout.EndScrollView();
            });
        }

        private Texture GetUnitIcon(Unit unit)
        {
            if (unit is MemberUnit)
            {
                if (unit is InvokeMember invokeMember)
                {
                    var descriptor = invokeMember.Descriptor<InvokeMemberDescriptor>();
                    return descriptor.Icon()[1];
                }
                else if (unit is GetMember getMember)
                {
                    var descriptor = getMember.Descriptor<GetMemberDescriptor>();
                    return descriptor.Icon()[1];
                }
                else
                {
                    var descriptor = unit.Descriptor<SetMemberDescriptor>();
                    return descriptor.Icon()[1];
                }
            }
            else if (unit is Literal literal)
            {
                var descriptor = literal.Descriptor<LiteralDescriptor>();
                return descriptor.Icon()[1];
            }
            else if (unit is UnifiedVariableUnit unifiedVariableUnit)
            {
                var descriptor = unifiedVariableUnit.Descriptor<UnitDescriptor<UnifiedVariableUnit>>();
                return descriptor.Icon()[1];
            }
            else
            {
                var iconDescriptor = unit.GetType().Icon();
                return iconDescriptor[1];
            }
        }

        string GetUnitPath(GraphReference reference)
        {
            var nodePath = reference;
            var pathNames = "";
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            while (nodePath != null)
            {
                var prefix = "::";
                if (nodePath.graph != null)
                {
                    if (string.IsNullOrEmpty(nodePath.graph.title))
                    {
                        prefix = nodePath.graph.GetType().ToString().Split(".").Last();
                    }
                    else
                    {
                        prefix = nodePath.graph.title;
                    }

                    prefix += " -> ";
                }

                pathNames = prefix + pathNames;
                nodePath = nodePath.ParentReference(false);
            }

            return pathNames;
        }

        private void SearchForErrors()
        {
            if (_checkScriptGraphAssets)
            {
                var guids = AssetDatabase.FindAssets("t:ScriptGraphAsset", null);
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(assetPath);
                    if (asset.GetReference().graph is not FlowGraph flowGraph) continue;
                    var baseRef = asset.GetReference().AsReference();
                    foreach (var element in TraverseFlowGraph(baseRef))
                    {
                        var reference = element.Item1;
                        var unit = element.Item2;
                        var newMatch = MatchUnit(unit, baseRef);
                        if (newMatch == null) continue;
                        newMatch.ScriptGraphAsset = asset;
                        newMatch.Reference = reference;
                        _matchObjects.Add(newMatch);
                        if (_matchScriptGraphMap.TryGetValue(newMatch.ScriptGraphAsset, out var list))
                        {
                            if (!list.Any(match => match.Unit == newMatch.Unit))
                            {
                                list.Add(newMatch);
                            }
                            else
                            {
                                list[list.IndexOf(list.First(match => match.Unit == newMatch.Unit))] = newMatch;
                            }
                        }
                        else
                        {
                            _matchScriptGraphMap[newMatch.ScriptGraphAsset] = new List<MatchObject>() { newMatch };
                        }
                    }
                }

                _sortedScriptGraphKey = _matchScriptGraphMap.Keys.ToList();
                _sortedScriptGraphKey.Sort((a, b) => String.Compare(a.name, b.name, StringComparison.Ordinal));
            }

            if (_checkScriptMachines)
            {
                foreach (var machine in UnityObjectUtility.FindObjectsOfTypeIncludingInactive<ScriptMachine>().Where(_asset => _asset.nest.source == GraphSource.Embed))
                {
                    if (machine.GetReference().graph is not FlowGraph flowGraph) continue;
                    var baseRef = machine.GetReference().AsReference();
                    foreach (var element in TraverseFlowGraph(baseRef))
                    {
                        var reference = element.Item1;
                        var unit = element.Item2;
                        var newMatch = MatchUnit(unit, baseRef);
                        if (newMatch == null) continue;
                        newMatch.ScriptMachine = machine;
                        newMatch.Reference = reference;
                        _matchObjects.Add(newMatch);
                        if (_matchScriptMachineMap.TryGetValue(newMatch.ScriptMachine, out var list))
                        {
                            if (!list.Any(match => match.Unit == newMatch.Unit))
                            {
                                list.Add(newMatch);
                            }
                            else
                            {
                                list[list.IndexOf(list.First(match => match.Unit == newMatch.Unit))] = newMatch;
                            }
                        }
                        else
                        {
                            _matchScriptMachineMap[newMatch.ScriptMachine] = new List<MatchObject>() { newMatch };
                        }
                    }
                }

                _sortedScriptMachineKey = _matchScriptMachineMap.Keys.ToList();
                _sortedScriptMachineKey.Sort((a, b) => string.Compare(a.nest.graph.title, b.nest.graph.title, StringComparison.Ordinal));
            }

            if (_checkStateGraphAssets)
            {
                var guids = AssetDatabase.FindAssets("t:StateGraphAsset", null);
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<StateGraphAsset>(assetPath);

                    var baseRef = asset.GetReference().AsReference();
                    foreach (var element in TraverseStateGraph(baseRef))
                    {
                        var reference = element.Item1;
                        var unit = element.Item2;
                        var newMatch = MatchUnit(unit, baseRef);
                        if (newMatch == null) continue;
                        newMatch.StateGraphAsset = asset;
                        newMatch.Reference = reference;
                        _matchObjects.Add(newMatch);
                        if (_matchScriptGraphMap.TryGetValue(newMatch.ScriptGraphAsset, out var list))
                        {
                            if (!list.Any(match => match.Unit == newMatch.Unit))
                            {
                                list.Add(newMatch);
                            }
                            else
                            {
                                list[list.IndexOf(list.First(match => match.Unit == newMatch.Unit))] = newMatch;
                            }
                        }
                        else
                        {
                            _matchScriptGraphMap[newMatch.ScriptGraphAsset] = new List<MatchObject>() { newMatch };
                        }
                    }
                }

                _sortedStateGraphKey = _matchStateGraphMap.Keys.ToList();
                _sortedStateGraphKey.Sort((a, b) => String.Compare(a.name, b.name, StringComparison.Ordinal));
            }
        }

        private void Search()
        {
            _matchObjects.Clear();
            _matchScriptGraphMap.Clear();
            _sortedScriptGraphKey.Clear();
            _matchStateGraphMap.Clear();
            _sortedStateGraphKey.Clear();
            _matchScriptMachineMap.Clear();
            _sortedScriptMachineKey.Clear();

            var matchWord = new Regex(_pattern, RegexOptions.IgnoreCase);
            // for script graphs.
            // begin of script graph

            if (_checkScriptGraphAssets)
            {
                var guids = AssetDatabase.FindAssets("t:ScriptGraphAsset", null);
                foreach (var guid in guids)
                {
                    // continue;
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(assetPath);
                    if (asset.GetReference().graph is not FlowGraph flowGraph) continue;
                    var baseRef = asset.GetReference().AsReference();
                    foreach (var element in TraverseFlowGraph(baseRef))
                    {
                        var reference = element.Item1;
                        var unit = element.Item2;
                        var newMatch = MatchUnit(matchWord, unit);
                        if (newMatch == null) continue;
                        newMatch.ScriptGraphAsset = asset;
                        newMatch.Reference = reference;
                        _matchObjects.Add(newMatch);
                        if (_matchScriptGraphMap.TryGetValue(newMatch.ScriptGraphAsset, out var list))
                        {
                            list.Add(newMatch);
                        }
                        else
                        {
                            _matchScriptGraphMap[newMatch.ScriptGraphAsset] = new List<MatchObject>() { newMatch };
                        }
                    }
                }

                _sortedScriptGraphKey = _matchScriptGraphMap.Keys.ToList();
                _sortedScriptGraphKey.Sort((a, b) => String.Compare(a.name, b.name, StringComparison.Ordinal));
            }

            if (_checkScriptMachines)
            {
                foreach (var machine in UnityObjectUtility.FindObjectsOfTypeIncludingInactive<ScriptMachine>().Where(_asset => _asset.nest.source == GraphSource.Embed))
                {
                    if (machine.GetReference().graph is not FlowGraph flowGraph) continue;
                    var baseRef = machine.GetReference().AsReference();
                    foreach (var element in TraverseFlowGraph(baseRef))
                    {
                        var reference = element.Item1;
                        var unit = element.Item2;
                        var newMatch = MatchUnit(matchWord, unit);
                        if (newMatch == null) continue;
                        newMatch.ScriptMachine = machine;
                        newMatch.Reference = reference;
                        _matchObjects.Add(newMatch);
                        if (_matchScriptMachineMap.TryGetValue(newMatch.ScriptMachine, out var list))
                        {
                            list.Add(newMatch);
                        }
                        else
                        {
                            _matchScriptMachineMap[newMatch.ScriptMachine] = new List<MatchObject>() { newMatch };
                        }
                    }
                }

                _sortedScriptMachineKey = _matchScriptMachineMap.Keys.ToList();
                _sortedScriptMachineKey.Sort((a, b) => string.Compare(a.nest.graph.title, b.nest.graph.title, StringComparison.Ordinal));
            }

            if (_checkStateGraphAssets)
            {
                var guids = AssetDatabase.FindAssets("t:StateGraphAsset", null);
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<StateGraphAsset>(assetPath);

                    var baseRef = asset.GetReference().AsReference();
                    foreach (var element in TraverseStateGraph(baseRef))
                    {
                        var reference = element.Item1;
                        var unit = element.Item2;
                        var newMatch = MatchUnit(matchWord, unit);
                        if (newMatch == null) continue;
                        newMatch.StateGraphAsset = asset;
                        newMatch.Reference = reference;
                        _matchObjects.Add(newMatch);
                        if (_matchStateGraphMap.TryGetValue(newMatch.StateGraphAsset, out var list))
                        {
                            if (!list.Any(match => match.Unit == newMatch.Unit))
                            {
                                list.Add(newMatch);
                            }
                        }
                        else
                        {
                            _matchStateGraphMap[newMatch.StateGraphAsset] = new List<MatchObject>() { newMatch };
                        }
                    }
                }

                _sortedStateGraphKey = _matchStateGraphMap.Keys.ToList();
                _sortedStateGraphKey.Sort((a, b) => String.Compare(a.name, b.name, StringComparison.Ordinal));
            }
        }

        IEnumerable<(List<SuperState>, FlowStateTransition, FlowGraph)> GetSubStates(
            GraphElementCollection<IState> states,
            GraphConnectionCollection<IStateTransition, IState, IState> transitions,
            SuperState parent,
            List<SuperState> nestParent)
        {
            nestParent = new List<SuperState>(nestParent)
            {
                parent
            };
            // var stateGraph = states.nest.graph;
            // yield direct graphs first.
            foreach (var state in states)
            {
                if (state is not FlowState flowState) continue;
                // check flow graphs
                FlowGraph graph = null;
                graph = flowState.nest.embed ?? flowState.nest.graph;

                if (graph == null) continue;
                yield return (nestParent, null, graph);
            }

            // yield transitions.
            foreach (var transition in transitions)
            {
                if (transition is not FlowStateTransition flowStateTransition) continue;
                FlowGraph graph = null;
                graph = flowStateTransition.nest.embed ?? flowStateTransition.nest.graph;

                if (graph == null) continue;
                yield return (nestParent, flowStateTransition, graph);
            }

            // traverse sub states.
            foreach (var subState in states)
            {
                if (subState is not SuperState subSuperState) continue;
                var subStateGraph = subSuperState.nest.graph;
                var subTransitions = subStateGraph.transitions;
                foreach (var item in GetSubStates(subStateGraph.states, subTransitions, subSuperState, nestParent))
                {
                    yield return item;
                }
            }
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
                    // going deep
                    case SubgraphUnit subgraphUnit:
                        {
                            var subGraph = subgraphUnit.nest.embed ?? subgraphUnit.nest.graph;
                            if (subGraph == null) continue;
                            yield return (graphReference, subgraphUnit);
                            // find sub graph.
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
        private bool HandleSearch(IUnit unit, out string name)
        {
            if (unit is MemberUnit memberUnit)
            {
                var _name = memberUnit.member.ToPseudoDeclarer().ToString();
                name = _name;
                return SearchUtility.Matches(SearchUtility.Relevance(_pattern, _name));
            }
            else if (unit is SubgraphUnit subgraphUnit)
            {
                var _name = subgraphUnit.nest != null ? GetGraphName(subgraphUnit.nest.graph).Replace("Graph", "Subgraph") : "Subgraph";
                name = _name;
                return SearchUtility.Matches(SearchUtility.Relevance(_pattern, _name));
            }
            else
            {
                var _name = BoltFlowNameUtility.UnitTitle(unit.GetType(), false, false);
                name = _name;
                return SearchUtility.Matches(SearchUtility.Relevance(_pattern, _name));
            }
        }

        IEnumerable<(GraphReference, Unit)> TraverseStateGraph(GraphReference graphReference)
        {
            var stateGraph = graphReference.graph as StateGraph;
            if (stateGraph == null) yield break;

            // var stateGraph = states.nest.graph;
            // yield direct graphs first.
            foreach (var state in stateGraph.states)
            {
                switch (state)
                {
                    case FlowState flowState:
                        {
                            // check flow graphs, which is the base of a state.
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
                            // check state graphs
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

            // don't forget transition nodes.
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


        private MatchObject MatchUnit(Regex matchWord, Unit unit)
        {
            var matchRecord = new MatchObject
            {
                Matches = new List<MatchType>(),
                Unit = unit,
                FullTypeName = GetUnitFullName(unit)
            };

            CheckMemberUnit(matchWord, unit, matchRecord);
            CheckLiteralUnit(unit, matchRecord);
            CheckFields(matchWord, unit, matchRecord);
            CheckDefaultValues(matchWord, unit, matchRecord);
            CheckAssetReferences(matchWord, unit, matchRecord);

            if (HandleSearch(unit, out string name))
            {
                matchRecord.FullTypeName = GetFullNameWithInputs(unit, name);
                matchRecord.Matches.Add(MatchType.Unit);
            }

            return matchRecord.Matches.Count > 0 ? matchRecord : null;
        }

        private MatchObject MatchUnit(Unit unit, GraphReference baseRef)
        {
            var matchRecord = new MatchObject
            {
                Matches = new List<MatchType>(),
                Unit = unit,
                FullTypeName = GetFullNameWithInputs(unit, GetUnitFullName(unit))
            };

            if (unit.GetException(baseRef) != null || unit is MissingType)
            {
                if (unit.GetException(baseRef) != null)
                {
                    matchRecord.FullTypeName += $" ({unit.GetException(baseRef).Message})";
                }
                else if (unit is MissingType missingType)
                {
                    matchRecord.FullTypeName += $" {(missingType.formerType == null ? "Missing Type" : "Missing Type : " + missingType.formerType)}";
                }
                matchRecord.Matches.Add(MatchType.Error);
            }

            return matchRecord.Matches.Count > 0 ? matchRecord : null;
        }

        private string GetUnitFullName(Unit unit)
        {
            var typeName = GetUnitName(unit);

            if (unit is MemberUnit invoker && invoker.member.targetType != null)
            {
                typeName = invoker.member.ToPseudoDeclarer().ToString();
            }

            return typeName;
        }

        private void CheckMemberUnit(Regex matchWord, Unit unit, MatchObject matchRecord)
        {
            if (unit is MemberUnit)
            {
                if (matchWord.IsMatch(matchRecord.FullTypeName))
                {
                    matchRecord.Matches.Add(MatchType.Unit);
                }
            }
        }

        private void CheckLiteralUnit(Unit unit, MatchObject matchRecord)
        {
            if (unit is Literal literal)
            {
                matchRecord.FullTypeName = $"{matchRecord.FullTypeName} (Type : {literal.type.As().CSharpName(false, false, false)}, Value : {literal.value})";
            }
            else if (unit.valueInputs.Count > 0)
            {
                matchRecord.FullTypeName += $" : ({string.Join(", ", unit.valueInputs.Select(port => GetValue(port)))})";
            }
        }

        private void CheckFields(Regex matchWord, Unit unit, MatchObject matchRecord)
        {
            var fields = unit.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var value = field.GetValue(unit);
                if (value == null) continue;
                if (matchWord.IsMatch(value.ToString()))
                {
                    matchRecord.Matches.Add(MatchType.Unit);
                    break;
                }
            }
        }

        private void CheckDefaultValues(Regex matchWord, Unit unit, MatchObject matchRecord)
        {
            foreach (var kvp in unit.defaultValues)
            {
                var value = kvp.Value;
                if (value == null) continue;
                if (matchWord.IsMatch(value.ToString()))
                {
                    matchRecord.Matches.Add(MatchType.Unit);
                    matchRecord.FullTypeName += $" ({kvp.Key.LegalMemberName().Prettify()} : {(value is Type type ? type.HumanName() : value)})";
                    break;
                }
            }
        }

        private void CheckAssetReferences(Regex matchWord, Unit unit, MatchObject matchRecord)
        {
            foreach (var kvp in unit.defaultValues)
            {
                if (kvp.Value is not Object obj) continue;
                if (!AssetDatabase.Contains(obj)) continue;
                if (matchWord.IsMatch(AssetDatabase.GetAssetPath(obj)))
                {
                    matchRecord.Matches.Add(MatchType.Unit);
                    matchRecord.FullTypeName += $" ({kvp.Key.LegalMemberName().Prettify()} : {kvp.Value})";
                    break;
                }
            }
        }

        private string GetFullNameWithInputs(Unit unit, string baseName)
        {
            if (unit is Literal literal)
            {
                return $"{baseName} (Type : {literal.type.As().CSharpName(false, false, false)}, Value : {literal.value})";
            }
            else if (unit is MemberUnit memberUnit && memberUnit.member.targetType != null)
            {
                return $"{memberUnit.member.ToPseudoDeclarer()} : ({string.Join(", ", unit.valueInputs.Select(port => GetValue(port)))})";
            }
            else if (unit.valueInputs.Count > 0)
            {
                return $"{baseName} : ({string.Join(", ", unit.valueInputs.Select(port => GetValue(port)))})";
            }
            return baseName;
        }

        private string GetUnitName(Unit unit)
        {
            return BoltFlowNameUtility.UnitTitle(unit.GetType(), false, false);
        }

        private string GetValue(ValueInput valueInput)
        {
            if (valueInput.hasDefaultValue)
            {

                return $"{valueInput.key.LegalMemberName().Prettify()} : " + (!valueInput.nullMeansSelf ? valueInput.unit.defaultValues[valueInput.key] is Type type ? type.HumanName() : valueInput.unit.defaultValues[valueInput.key]?.ToString() ?? "null" : "This");
            }
            else if (valueInput.hasAnyConnection)
            {
                if (valueInput.hasValidConnection)
                {
                    return $"{valueInput.key.LegalMemberName().Prettify()} : Connected To : " + GetUnitName(valueInput.connection.source.unit as Unit);
                }
                else if (valueInput.hasInvalidConnection)
                {
                    return $"{valueInput.key.LegalMemberName().Prettify()} : Invalid Connection";
                }
            }

            return $"{valueInput.key.LegalMemberName().Prettify()} : No Value";
        }

        bool ShouldShowItem(IEnumerable<MatchObject> list)
        {
            foreach (var match in list)
            {
                if (match.Matches.Contains(MatchType.Unit))
                {
                    return true;
                }
            }

            return false;
        }

        bool IsError(IEnumerable<MatchObject> list)
        {
            foreach (var match in list)
            {
                if (_matchError && match.Matches.Contains(MatchType.Error))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetGraphName(Graph graph)
        {
            return !string.IsNullOrEmpty(graph.title) ? graph.title : "Unnamed Graph";
        }

        void FocusMatchObject(MatchObject match)
        {
            if (match.ScriptGraphAsset != null)
            {
                var asset = match.ScriptGraphAsset;
                // Locate
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
            else if (match.StateGraphAsset != null)
            {
                var asset = match.StateGraphAsset;
                // Locate
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
            else if (match.ScriptMachine != null)
            {
                var machine = match.ScriptMachine.gameObject;
                // Locate
                EditorGUIUtility.PingObject(machine);
                Selection.activeObject = machine;
            }

            // open
            GraphReference reference = match.Reference;
            GraphWindow.OpenActive(reference);

            // focus
            var context = reference.Context();
            if (context == null)
                return;
            context.BeginEdit();
            context.canvas?.ViewElements(((IGraphElement)match.Unit).Yield());
        }
    }
}
