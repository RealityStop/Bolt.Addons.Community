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
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{

    public interface IGraphProvider
    {
        string Name { get; }
        bool IsEnabled { get; }
        IEnumerable<(GraphReference, IGraphElement)> GetElements();
        void HandleMatch(NodeFinderWindow.MatchObject match);
        Object GetAssetForElement(GraphReference reference);
    }

    public interface IMatchHandler
    {
        bool CanHandle(IGraphElement element);
        NodeFinderWindow.MatchObject HandleMatch(IGraphElement element, string pattern);
    }
    public class NodeFinderWindow : EditorWindow
    {
        public abstract class BaseGraphProvider : IGraphProvider
        {
            protected readonly NodeFinderWindow _window;
            public abstract string Name { get; }
            private bool _isEnabled = true;
            public virtual bool IsEnabled => _isEnabled;

            protected BaseGraphProvider(NodeFinderWindow window)
            {
                _window = window;
            }

            public abstract IEnumerable<(GraphReference, IGraphElement)> GetElements();
            public abstract void HandleMatch(MatchObject match);

            public virtual void SetEnabled(bool enabled)
            {
                _isEnabled = enabled;
            }

            // Add these methods to handle results
            protected Dictionary<Object, List<MatchObject>> MatchMap { get; } = new();
            protected List<Object> SortedKeys { get; private set; } = new();

            public virtual void ClearResults()
            {
                MatchMap.Clear();
                SortedKeys.Clear();
            }

            public virtual void AddMatch(MatchObject match, Object key)
            {
                if (!MatchMap.TryGetValue(key, out var list))
                {
                    list = new List<MatchObject>();
                    MatchMap[key] = list;
                    SortedKeys.Add(key);
                }

                if (!list.Any(m => m.Unit == match.Unit))
                {
                    list.Add(match);
                }
                else
                {
                    list[list.IndexOf(list.First(m => m.Unit == match.Unit))] = match;
                }
            }

            public virtual IEnumerable<(Object key, List<MatchObject> matches)> GetResults()
            {
                SortedKeys.Sort((a, b) => string.Compare(GetSortKey(a), GetSortKey(b), StringComparison.Ordinal));
                return SortedKeys.Select(key => (key, MatchMap[key]));
            }

            protected virtual string GetSortKey(Object key)
            {
                return key.name;
            }

            public virtual Object GetAssetForElement(GraphReference reference)
            {
                return reference?.rootObject;
            }
        }

        // Optimized data structures
        private readonly Dictionary<Type, IGraphProvider> _graphProviders = new();
        private readonly Dictionary<Type, IMatchHandler> _matchHandlers = new();
        private readonly Dictionary<Type, List<MatchObject>> _matchMap = new();
        private readonly List<MatchObject> _matchObjects = new();

        // Cache
        private readonly Dictionary<GraphReference, List<(GraphReference, IGraphElement)>> _graphElementCache = new();
        private float _lastSearchTime;
        private const float SearchCooldown = 0.5f; // Prevent too frequent searches

        // State
        private string _pattern = "";
        private string _previousPattern = "";
        private Vector2 _scrollViewRoot;
        private bool _matchError = true;
        private float _lastErrorCheckTime;
        private const float ErrorCheckInterval = 1.0f;

        // Add these near the top with other private fields
        private class FilterOption
        {
            public string Label { get; set; }
            public bool IsEnabled { get; set; }
            public Action<bool> OnToggled { get; set; }
            public bool RequiresSearch { get; set; }
            public Type ProviderType { get; set; }
        }

        private readonly List<FilterOption> _filters = new();

        private bool _needsSearch = false;

        private bool _showProviderFilters = true;
        private bool _showTypeFilters = true;
        private bool _showSpecialFilters = true;
        private Dictionary<MatchType, bool> _typeFilters = new();

        [MenuItem("Window/Community Addons/Node Finder")]
        public static void Open()
        {
            var window = GetWindow<NodeFinderWindow>();

            // Get the built-in search icon
            GUIContent searchIconContent = EditorGUIUtility.IconContent("d_ViewToolZoom");
            window.titleContent = new GUIContent("Node Finder", searchIconContent.image);
        }

        private void OnDisable()
        {
            _matchObjects.Clear();
            _matchMap.Clear();
        }

        private void OnEnable()
        {
            RegisterDefaultProviders();
            RegisterDefaultHandlers();
            InitializeFilters();
            InitializeTypeFilters();
            Search();
        }

        private void RegisterDefaultProviders()
        {
            RegisterProvider(new ScriptGraphProvider(this));
            RegisterProvider(new StateGraphProvider(this));
            RegisterProvider(new ClassAssetProvider(this));
            RegisterProvider(new StructAssetProvider(this));
            RegisterProvider(new ScriptMachineProvider(this));
            RegisterProvider(new StateMachineProvider(this));
        }

        private void RegisterDefaultHandlers()
        {
            RegisterHandler(new UnitMatchHandler());
            RegisterHandler(new GroupMatchHandler());
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            RegisterHandler(new StickyNoteMatchHandler());
#endif
            RegisterHandler(new CommentsMatchHandler());
            RegisterHandler(new ErrorMatchHandler());
        }

        private void RegisterProvider<T>(T provider) where T : IGraphProvider
        {
            _graphProviders[provider.GetType()] = provider;
        }

        private void RegisterHandler<T>(T handler) where T : IMatchHandler
        {
            _matchHandlers[handler.GetType()] = handler;
        }

        private void OnGUI()
        {
            Event e = Event.current;
            DrawSearchBar();
            GUILayout.Space(6);
            DrawFilters();
            DrawSeparator();
            if (e.keyCode == KeyCode.Return || _pattern != _previousPattern)
            {
                _needsSearch = true;
            }

            if (_needsSearch)
            {
                Search();
                _previousPattern = _pattern;
                _needsSearch = false;
            }
            DrawResults();
            if (_matchError && Time.realtimeSinceStartup - _lastErrorCheckTime >= ErrorCheckInterval)
            {
                _needsSearch = false;
                SearchForErrors();
                _lastErrorCheckTime = Time.realtimeSinceStartup;
                Repaint();
            }
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

        private void InitializeFilters()
        {
            _filters.Clear();

            foreach (var provider in _graphProviders.Values)
            {
                AddFilter(provider.Name,
                    () => provider.IsEnabled,
                    (enabled) =>
                    {
                        if (provider is BaseGraphProvider baseProvider)
                        {
                            baseProvider.SetEnabled(enabled);
                            if (enabled) Search();
                        }
                    },
                    true,
                    provider.GetType());
            }

            // Add special filters like errors that aren't tied to providers
            AddFilter("Errors",
                () => _matchError,
                (enabled) =>
                {
                    _matchError = enabled;
                    if (enabled) SearchForErrors();
                });
        }

        private void InitializeTypeFilters()
        {
            foreach (MatchType type in Enum.GetValues(typeof(MatchType)))
            {
                if (!_typeFilters.ContainsKey(type))
                {
                    _typeFilters[type] = true;
                }
            }
        }

        public void AddFilter(string label, Func<bool> getter, Action<bool> onToggled, bool requiresSearch = true, Type providerType = null)
        {
            _filters.Add(new FilterOption
            {
                Label = label,
                IsEnabled = getter(),
                OnToggled = onToggled,
                RequiresSearch = requiresSearch,
                ProviderType = providerType
            });
        }

        private void DrawFilters()
        {
            var filterLabelStyle = new GUIStyle(LudiqStyles.toolbarLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            var foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(2, 2, 2, 2), new RectOffset(1, 1, 1, 1), () =>
                {
                    // Provider Filters Section
                    _showProviderFilters = EditorGUILayout.Foldout(_showProviderFilters, "Provider Filters", true, foldoutStyle);
                    if (_showProviderFilters)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        var providerFilters = _filters.Where(f => f.ProviderType != null).ToList();
                        for (int i = 0; i < providerFilters.Count; i++)
                        {
                            DrawFilterToggle(providerFilters[i]);
                            GUILayout.Space(4);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel--;
                    }
                });
                DrawSeparator();
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(2, 2, 2, 2), new RectOffset(1, 1, 1, 1), () =>
                {
                    // Match Type Filters Section
                    _showTypeFilters = EditorGUILayout.Foldout(_showTypeFilters, "Type Filters", true, foldoutStyle);
                    if (_showTypeFilters)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.BeginHorizontal();
                        var types = Enum.GetValues(typeof(MatchType)).Cast<MatchType>().ToList();
                        for (int i = 0; i < types.Count; i++)
                        {
                            DrawTypeFilterToggle(types[i]);
                            GUILayout.Space(4);

                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel--;
                    }
                });
                DrawSeparator();
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(2, 2, 2, 2), new RectOffset(1, 1, 1, 1), () =>
                {
                    // Special Filters Section
                    _showSpecialFilters = EditorGUILayout.Foldout(_showSpecialFilters, "Special Filters", true, foldoutStyle);
                    if (_showSpecialFilters)
                    {
                        var specialFilters = _filters.Where(f => f.ProviderType == null).ToList();
                        if (specialFilters.Any())
                        {
                            EditorGUILayout.BeginHorizontal();
                            foreach (var filter in specialFilters)
                            {
                                DrawFilterToggle(filter);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                });
            });
        }

        private void DrawSeparator()
        {
            GUILayout.Space(4);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
            GUILayout.Space(4);
        }


        private void DrawFilterToggle(FilterOption filter)
        {
            bool previousState = filter.IsEnabled;
            filter.IsEnabled = GUILayout.Toggle(filter.IsEnabled, filter.Label, EditorStyles.toolbarButton);

            if (filter.IsEnabled != previousState)
            {
                filter.OnToggled?.Invoke(filter.IsEnabled);
                if (filter.RequiresSearch)
                {
                    Search();
                }
            }
        }

        private void DrawTypeFilterToggle(MatchType type)
        {
            bool previousState = _typeFilters[type];
            _typeFilters[type] = GUILayout.Toggle(previousState, type.ToString(), EditorStyles.toolbarButton);

            if (_typeFilters[type] != previousState)
            {
                Search();
            }
        }

        private void DrawResults()
        {
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
            {
                _scrollViewRoot = EditorGUILayout.BeginScrollView(_scrollViewRoot);

                bool empty = string.IsNullOrEmpty(_pattern) || _matchObjects.Count == 0;
                bool isShowingErrors = false;
                // Show total results count
                if (!empty)
                {
                    EditorGUILayout.LabelField($"Total Results: {_matchObjects.Count}", EditorStyles.boldLabel);

                    // Display all results
                    foreach (var provider in _graphProviders.Values.Where(p => p.IsEnabled))
                    {
                        foreach (var (key, matches) in (provider as BaseGraphProvider)?.GetResults() ?? Enumerable.Empty<(Object, List<MatchObject>)>())
                        {
                            if (!ShouldShowItem(matches)) continue;
                            DrawResultGroup(key, matches, ref isShowingErrors);
                        }
                    }
                }
                else if (_matchError)
                {
                    // Show errors
                    foreach (var provider in _graphProviders.Values.Where(p => p.IsEnabled))
                    {
                        foreach (var (key, matches) in (provider as BaseGraphProvider)?.GetResults() ?? Enumerable.Empty<(Object, List<MatchObject>)>())
                        {
                            if (!IsError(matches)) continue;
                            isShowingErrors = true;
                            DrawResultGroup(key, matches, ref isShowingErrors, true);
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

        private void DrawResultGroup(Object key, List<MatchObject> matches, ref bool isShowingErrors, bool errorsOnly = false)
        {
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            var headerStyle = new GUIStyle(LudiqStyles.toolbarLabel)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };

            var icon = GetGroupIcon(key);
            var label = GetGroupLabel(key);

            GUILayout.Label(new GUIContent(label, icon), headerStyle);

            foreach (var match in matches)
            {
                if (errorsOnly && !match.Matches.Any(m => m == MatchType.Error)) continue;
                DrawMatchItem(match, ref isShowingErrors);
            }
        }

        private Texture GetUnitIcon(IGraphElement element)
        {
            if (element is null) return typeof(Null).Icon()[1];
            if (element is MemberUnit)
            {
                if (element is InvokeMember invokeMember)
                {
                    var descriptor = invokeMember.Descriptor<InvokeMemberDescriptor>();
                    return descriptor.Icon()[1];
                }
                else if (element is GetMember getMember)
                {
                    var descriptor = getMember.Descriptor<GetMemberDescriptor>();
                    return descriptor.Icon()[1];
                }
                else
                {
                    var descriptor = element.Descriptor<SetMemberDescriptor>();
                    return descriptor.Icon()[1];
                }
            }
            else if (element is Literal literal)
            {
                var descriptor = literal.Descriptor<LiteralDescriptor>();
                return descriptor.Icon()[1];
            }
            else if (element is UnifiedVariableUnit unifiedVariableUnit)
            {
                var descriptor = unifiedVariableUnit.Descriptor<UnitDescriptor<UnifiedVariableUnit>>();
                return descriptor.Icon()[1];
            }
            else if (element is CommentNode commentNode)
            {
                return commentNode.Descriptor<CommentDescriptor>().Icon()[1];
            }
            else
            {
                var icon = element.GetType().Icon();
                return icon[1];
            }
        }

        private void SearchForErrors()
        {
            if (Time.realtimeSinceStartup - _lastSearchTime < SearchCooldown) return;

            _lastSearchTime = Time.realtimeSinceStartup;

            var handler = _matchHandlers[typeof(ErrorMatchHandler)];
            foreach (var provider in _graphProviders.Values.Where(p => p.IsEnabled))
            {
                List<IGraphElement> elements = new();
                foreach (var element in provider.GetElements())
                {
                    (handler as ErrorMatchHandler).graphPointer = element.Item1;
                    if (!handler.CanHandle(element.Item2) || elements.Contains(element.Item2)) continue;
                    elements.Add(element.Item2);
                    var match = handler.HandleMatch(element.Item2, _pattern);
                    if (match != null)
                    {
                        match.Reference = element.Item1;
                        ProcessMatch(match, provider);
                    }
                }
            }
        }

        private void Search()
        {
            if (Time.realtimeSinceStartup - _lastSearchTime < SearchCooldown) return;

            _lastSearchTime = Time.realtimeSinceStartup;

            _matchObjects.Clear();

            foreach (var provider in _graphProviders.Values.Where(p => p.IsEnabled))
            {
                if (provider is BaseGraphProvider baseProvider)
                {
                    baseProvider.ClearResults();
                }
                List<IGraphElement> elements = new();
                foreach (var element in provider.GetElements())
                {
                    foreach (var handler in _matchHandlers.Values)
                    {
                        if (!handler.CanHandle(element.Item2) || elements.Contains(element.Item2)) continue;
                        elements.Add(element.Item2);
                        var match = handler.HandleMatch(element.Item2, _pattern);
                        if (match != null)
                        {
                            match.Reference = element.Item1;
                            ProcessMatch(match, provider);
                        }
                    }
                }
            }
        }

        private IEnumerable<(GraphReference, IGraphElement)> GetCachedElements(IGraphProvider provider)
        {
            // Check if we need to refresh the cache
            bool shouldRefreshCache = false;
            foreach (var graphRef in _graphElementCache.Keys.ToList())
            {
                if (graphRef?.graph == null || !graphRef.isValid)
                {
                    _graphElementCache.Remove(graphRef);
                    shouldRefreshCache = true;
                }
            }

            // Get elements from provider
            var elements = provider.GetElements();

            // Process each element
            foreach (var (reference, element) in elements)
            {
                // Skip invalid references
                if (reference == null || !reference.isValid) continue;

                // Check cache for this reference
                if (!shouldRefreshCache && _graphElementCache.TryGetValue(reference, out var cachedElements))
                {
                    // Verify cached elements are still valid
                    bool cacheValid = true;
                    foreach (var (_, cachedElement) in cachedElements)
                    {
                        if (cachedElement is Unit unit && !unit.graph.elements.Contains(unit))
                        {
                            cacheValid = false;
                            break;
                        }
                    }

                    // Use cache if valid
                    if (cacheValid)
                    {
                        foreach (var cachedElement in cachedElements)
                        {
                            yield return cachedElement;
                        }
                        continue;
                    }
                }

                // Build new cache entry
                var newElements = new List<(GraphReference, IGraphElement)>();

                // Process the element
                if (element != null)
                {
                    newElements.Add((reference, element));
                    yield return (reference, element);
                }

                // Cache the results
                _graphElementCache[reference] = newElements;
            }
        }

        private void ProcessMatch(MatchObject match, IGraphProvider provider)
        {
            if (match == null || match.Matches.Count == 0) return;

            _matchObjects.Add(match);
            // Assign the appropriate asset type based on provider
            if (provider is ScriptGraphProvider)
                match.ScriptGraphAsset = provider.GetAssetForElement(match.Reference) as ScriptGraphAsset;
            else if (provider is StateGraphProvider)
                match.StateGraphAsset = provider.GetAssetForElement(match.Reference) as StateGraphAsset;
            else if (provider is ClassAssetProvider)
                match.ClassAsset = provider.GetAssetForElement(match.Reference) as ClassAsset;
            else if (provider is StructAssetProvider)
                match.StructAsset = provider.GetAssetForElement(match.Reference) as StructAsset;

            // Let provider handle the match
            provider.HandleMatch(match);
        }

        private Object GetAssetKeyForMatch(MatchObject match)
        {
            if (match.ScriptGraphAsset != null) return match.ScriptGraphAsset;
            if (match.StateGraphAsset != null) return match.StateGraphAsset;
            if (match.ClassAsset != null) return match.ClassAsset;
            if (match.StructAsset != null) return match.StructAsset;
            if (match.ScriptMachine != null) return match.ScriptMachine;
            if (match.StateMachine != null) return match.StateMachine;
            return null;
        }

        public IEnumerable<GraphReference> GetReferences(ClassAsset asset)
        {
            foreach (var constructor in asset.constructors)
            {
                yield return constructor.GetReference().AsReference();
            }
            foreach (var variable in asset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        yield return variable.getter.GetReference().AsReference();
                    if (variable.set)
                        yield return variable.setter.GetReference().AsReference();
                }
            }
            foreach (var method in asset.methods)
            {
                yield return method.GetReference().AsReference();
            }
        }

        public IEnumerable<GraphReference> GetReferences(StructAsset asset)
        {
            foreach (var constructor in asset.constructors)
            {
                yield return constructor.GetReference().AsReference();
            }
            foreach (var variable in asset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        yield return variable.getter.GetReference().AsReference();
                    if (variable.set)
                        yield return variable.setter.GetReference().AsReference();
                }
            }
            foreach (var method in asset.methods)
            {
                yield return method.GetReference().AsReference();
            }
        }

        private bool HandleUnitSearch(IUnit unit, out string name)
        {
            if (unit is null) { name = ""; return false; }
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
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
        private bool HandleStickyNoteSearch(StickyNote note, out string name)
        {
            var _name = StickyNoteFullName(note);
            name = _name;
            return SearchUtility.Matches(SearchUtility.Relevance(_pattern, _name));
        }
#endif
        private bool HandleGraphGroupSearch(GraphGroup group, out string name)
        {
            var _name = GroupFullName(group);
            name = _name;
            return SearchUtility.Matches(SearchUtility.Relevance(_pattern, _name));
        }

        private MatchObject MatchUnit(Regex matchWord, IGraphElement element)
        {
            MatchObject matchRecord = null;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (element is StickyNote stickyNote)
            {
                matchRecord = matchRecord = new MatchObject
                {
                    Matches = new List<MatchType>(),
                    stickyNote = stickyNote,
                    FullTypeName = StickyNoteFullName(stickyNote)
                };
                if (HandleStickyNoteSearch(stickyNote, out string name))
                {
                    matchRecord.Matches.Add(MatchType.StickyNote);
                }
            }
            else if (element is GraphGroup graphGroup)
            {
                matchRecord = matchRecord = new MatchObject
                {
                    Matches = new List<MatchType>(),
                    group = graphGroup,
                    FullTypeName = GroupFullName(graphGroup)
                };
                if (HandleGraphGroupSearch(graphGroup, out string name))
                {
                    matchRecord.Matches.Add(MatchType.Group);
                }
            }
            else
            {
                var unit = element as Unit;
                matchRecord = new MatchObject
                {
                    Matches = new List<MatchType>(),
                    Unit = unit,
                    FullTypeName = GetUnitFullName(unit)
                };
                CheckMemberUnit(matchWord, unit, matchRecord);
                CheckLiteralUnit(unit, matchRecord);
                CheckDefaultValues(matchWord, unit, matchRecord);
                if (HandleUnitSearch(unit, out string name))
                {
                    matchRecord.FullTypeName = GetFullNameWithInputs(unit, name);
                    matchRecord.Matches.Add(MatchType.Unit);
                }
            }
#else
            if (element is GraphGroup graphGroup)
            {
                matchRecord = matchRecord = new MatchObject
                {
                    Matches = new List<MatchType>(),
                    group = graphGroup,
                    FullTypeName = GroupFullName(graphGroup)
                };
                if (HandleGraphGroupSearch(graphGroup, out string name))
                {
                    matchRecord.Matches.Add(MatchType.Group);
                }
            }
            else
            {
                var unit = element as Unit;
                matchRecord = new MatchObject
                {
                    Matches = new List<MatchType>(),
                    Unit = unit,
                    FullTypeName = GetUnitFullName(unit)
                };
                CheckMemberUnit(matchWord, unit, matchRecord);
                CheckLiteralUnit(unit, matchRecord);
                CheckFields(matchWord, unit, matchRecord);
                CheckDefaultValues(matchWord, unit, matchRecord);
                if (HandleUnitSearch(unit, out string name))
                {
                    matchRecord.FullTypeName = GetFullNameWithInputs(unit, name);
                    matchRecord.Matches.Add(MatchType.Unit);
                }
            }
#endif      
            return matchRecord.Matches.Count > 0 ? matchRecord : null;
        }

        private MatchObject MatchUnit(Unit unit, GraphReference baseRef)
        {
            if (unit == null) return null;

            var matchRecord = new MatchObject
            {
                Matches = new List<MatchType>(),
                Unit = unit,
                FullTypeName = GetUnitFullName(unit)
            };
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            bool hasError = unit.GetException(baseRef) != null || unit is MissingType;
            if (!hasError) return null;
#else
            bool hasError = unit.GetException(baseRef) != null;
            if (!hasError) return null;
#endif

            if (unit.GetException(baseRef) != null)
            {
                matchRecord.FullTypeName += $" ({unit.GetException(baseRef).Message})";
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (unit is MissingType missingType)
            {
                matchRecord.FullTypeName += $" {(missingType.formerType == null ? "Missing Type" : "Missing Type : " + missingType.formerType)}";
            }
#endif

            matchRecord.Matches.Add(MatchType.Error);
            return matchRecord;
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
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
        private string StickyNoteFullName(StickyNote note)
        {
            if (!string.IsNullOrEmpty(note.title) && !string.IsNullOrEmpty(note.body))
            {
                return "StickyNote : " + note.title + "." + note.body;
            }
            else if (!string.IsNullOrEmpty(note.title))
            {
                return "StickyNote : " + note.title;
            }
            else if (!string.IsNullOrEmpty(note.body))
            {
                return "StickyNote : " + note.body;
            }
            return "Empty StickyNote";
        }
#endif
        private string GroupFullName(GraphGroup group)
        {
            if (!string.IsNullOrEmpty(group.label) && !string.IsNullOrEmpty(group.comment))
            {
                return "Graph Group : " + group.label + "." + group.comment;
            }
            else if (!string.IsNullOrEmpty(group.label))
            {
                return "Graph Group : " + group.label;
            }
            else if (!string.IsNullOrEmpty(group.comment))
            {
                return "Graph Group : " + group.comment;
            }
            return "Unnamed Graph Group";
        }

        private void CheckMemberUnit(Regex matchWord, Unit unit, MatchObject matchRecord)
        {
            if (unit is null) return;
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
            if (unit is null) return;
            if (unit is Literal literal)
            {
                matchRecord.FullTypeName = $"{matchRecord.FullTypeName} (Type : {literal.type.As().CSharpName(false, false, false)}, Value : {literal.value})";
            }
            else if (unit.valueInputs.Count > 0)
            {
                matchRecord.FullTypeName += $" : ({string.Join(", ", unit.valueInputs.Select(port => GetValue(port)))})";
            }
        }

        private void CheckDefaultValues(Regex matchWord, Unit unit, MatchObject matchRecord)
        {
            if (unit is null) return;
            foreach (var kvp in unit.defaultValues)
            {
                var value = kvp.Value;
                if (value == null) continue;
                if (matchWord.IsMatch(value.ToString()))
                {
                    matchRecord.Matches.Add(MatchType.Unit);
                    matchRecord.FullTypeName += $" ({kvp.Key.LegalMemberName().Prettify()} : {(value is Type type ? type.HumanName() : value is Object @object ? @object.name : value)})";
                    break;
                }
            }
        }

        private string GetFullNameWithInputs(Unit unit, string baseName)
        {
            if (unit is null) return "";
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

        private string GetUnitName(IGraphElement element)
        {
            if (element is Unit unit)
                return BoltFlowNameUtility.UnitTitle(unit.GetType(), true, false);
            else if (element is not null) return element.Descriptor().description.title;
            return "Null Element";
        }

        private string GetValue(ValueInput valueInput)
        {
            if (valueInput.hasDefaultValue && !valueInput.hasAnyConnection)
            {
                return $"{valueInput.key.LegalMemberName().Prettify()} : " + (!valueInput.nullMeansSelf && !(typeof(Component).IsAssignableFrom(valueInput.type) || valueInput.type == typeof(GameObject)) ? valueInput.unit.defaultValues[valueInput.key] is Type type ? type.HumanName() : (valueInput.unit.defaultValues[valueInput.key] is Object obj ? obj.name : valueInput.unit.defaultValues[valueInput.key]?.ToString()) ?? "null" : "This");
            }
            else if (valueInput.hasAnyConnection)
            {
                return valueInput.key.LegalMemberName().Prettify() + " : " + "Connected";
            }
            return valueInput.key.LegalMemberName().Prettify() + " : " + "No Value";
        }

        bool ShouldShowItem(IEnumerable<MatchObject> list)
        {
            foreach (var match in list)
            {
                foreach (var matchType in match.Matches)
                {
                    if (_typeFilters[matchType])
                    {
                        return true;
                    }
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
            var target = OpenReferencePath(match.Reference);
            GraphWindow.OpenActive(target);
            // focus
            var context = target.Context();
            if (context == null)
                return;
            context.BeginEdit();
            if (match.group != null)
            {
                context.canvas?.ViewElements(((IGraphElement)match.group).Yield());
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (match.stickyNote != null)
            {
                context.canvas?.ViewElements(((IGraphElement)match.stickyNote).Yield());
            }
#endif
            else if (match.Unit != null)
            {
                context.canvas?.ViewElements(((IGraphElement)match.Unit).Yield());
            }
            context.EndEdit();
        }

        GraphReference OpenReferencePath(GraphReference graphReference)
        {
            var path = GraphTraversal.GetReferencePath(graphReference);
            GraphReference targetReference = graphReference.root.GetReference().AsReference();
            foreach (var item in path)
            {
                if (item.Item2 != null)
                {
                    targetReference = targetReference.ChildReference(item.Item2, false);
                }
                else if (item.Item1.isRoot)
                {
                    targetReference = item.Item1;
                }
            }
            return targetReference;
        }

        public enum MatchType
        {
            Unit,
            Comment,
            Group,
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            StickyNote,
#endif
            Error
        }

        public class MatchObject
        {
            public List<MatchType> Matches { get; set; } = new List<MatchType>();
            public ScriptGraphAsset ScriptGraphAsset { get; set; }
            public ScriptMachine ScriptMachine { get; set; }
            public StateMachine StateMachine { get; set; }
            public StateGraphAsset StateGraphAsset { get; set; }
            public ClassAsset ClassAsset { get; set; }
            public StructAsset StructAsset { get; set; }
            public GraphReference Reference { get; set; }
            public string FullTypeName { get; set; }
            public IUnit Unit { get; set; }
            public GraphGroup group { get; set; }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            public StickyNote stickyNote { get; set; }
#endif
            public CommentNode comment { get; set; }
        }

        private Texture GetGroupIcon(Object key)
        {
            switch (key)
            {
                case ScriptGraphAsset _:
                    return EditorGUIUtility.ObjectContent(key, typeof(ScriptGraphAsset)).image;
                case StateGraphAsset _:
                    return EditorGUIUtility.ObjectContent(key, typeof(StateGraphAsset)).image;
                case ScriptMachine _:
                case StateMachine _:
                    return EditorGUIUtility.IconContent("GameObject Icon").image;
                case ClassAsset classAsset:
                    return classAsset.icon ?? typeof(ClassAsset).Icon()[1];
                case StructAsset structAsset:
                    return structAsset.icon ?? typeof(StructAsset).Icon()[1];
                default:
                    return null;
            }
        }

        private string GetGroupLabel(Object key)
        {
            switch (key)
            {
                case ScriptMachine scriptMachine:
                    return $"{scriptMachine.name} (ScriptMachine)";
                case StateMachine stateMachine:
                    return $"{stateMachine.name} (StateMachine)";
                default:
                    return key.name;
            }
        }

        private void DrawMatchItem(MatchObject match, ref bool isShowingErrors, bool errorsOnly = false)
        {
            var pathNames = GraphTraversal.GetElementPath(match.Reference);
            var isError = match.Matches.Contains(MatchType.Error) && _matchError;

            if (isError)
            {
                isShowingErrors = true;
            }

            var label = isError
                ? $"      {pathNames} <color=#FF6800>{SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}</color>"
                : $"      {pathNames} {SearchUtility.HighlightQuery(match.FullTypeName, _pattern)}";

            var pathStyle = new GUIStyle(LudiqStyles.paddedButton)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };

            if (match.Matches.Contains(MatchType.Error) && !_matchError) return;

            IGraphElement element = match.Unit;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (match.stickyNote != null) element = match.stickyNote;
#endif
            if (match.group != null) element = match.group;
            if (match.comment != null) element = match.comment;

            EditorGUILayout.BeginHorizontal();
            bool buttonClicked = GUILayout.Button(new GUIContent(label, GetUnitIcon(element)), pathStyle);
            EditorGUILayout.EndHorizontal();

            if (buttonClicked)
            {
                FocusMatchObject(match);
            }
        }
    }

    public class ScriptMachineProvider : NodeFinderWindow.BaseGraphProvider
    {
        public override string Name => "ScriptMachines";
        public ScriptMachineProvider(NodeFinderWindow nodeFinderWindow) : base(nodeFinderWindow)
        {
        }

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            foreach (var machine in FindObjectsOfTypeIncludingInactive<ScriptMachine>().Where(_asset => _asset.nest.source == GraphSource.Embed))
            {
                if (machine?.GetReference().graph is not FlowGraph) continue;

                var baseRef = machine.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseFlowGraph(baseRef))
                {
                    yield return element;
                }
            }
        }

        private static IEnumerable<T> FindObjectsOfTypeIncludingInactive<T>()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded)
                {
                    foreach (var rootGameObject in scene.GetRootGameObjects())
                    {
                        foreach (var result in rootGameObject.GetComponents<T>())
                        {
                            yield return result;
                        }
                        foreach (var result in rootGameObject.GetComponentsInChildren<T>(true))
                        {
                            yield return result;
                        }
                    }
                }
            }
        }

        public override void HandleMatch(NodeFinderWindow.MatchObject match)
        {
            var machine = match.ScriptMachine;
            if (machine != null)
            {
                AddMatch(match, machine);
            }
        }

        protected override string GetSortKey(Object key)
        {
            return (key as ScriptMachine).graph.title ?? base.GetSortKey(key);
        }
    }

    // Implement concrete providers and handlers
    public class ScriptGraphProvider : NodeFinderWindow.BaseGraphProvider
    {
        public override string Name => "ScriptGraphAssets";

        public ScriptGraphProvider(NodeFinderWindow window) : base(window) { }

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:ScriptGraphAsset", null);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(assetPath);
                if (asset?.GetReference().graph is not FlowGraph) continue;

                var baseRef = asset.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseFlowGraph(baseRef))
                {
                    yield return element;
                }
            }
        }

        public override void HandleMatch(NodeFinderWindow.MatchObject match)
        {
            var asset = match.ScriptGraphAsset;
            if (asset != null)
            {
                AddMatch(match, asset);
            }
        }

        protected override string GetSortKey(Object key)
        {
            return (key as ScriptGraphAsset).graph.title ?? base.GetSortKey(key);
        }
    }

    public class StateGraphProvider : NodeFinderWindow.BaseGraphProvider
    {
        public override string Name => "StateGraphAssets";

        public StateGraphProvider(NodeFinderWindow window) : base(window) { }

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:StateGraphAsset", null);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<StateGraphAsset>(assetPath);
                if (asset?.GetReference().graph is not StateGraph) continue;

                var baseRef = asset.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseStateGraph(baseRef))
                {
                    yield return element;
                }
            }
        }

        public override void HandleMatch(NodeFinderWindow.MatchObject match)
        {
            var asset = match.StateGraphAsset;
            if (asset != null)
            {
                AddMatch(match, asset);
            }
        }

        protected override string GetSortKey(Object key)
        {
            return (key as StateGraphAsset).graph.title ?? base.GetSortKey(key);
        }
    }

    public class StateMachineProvider : NodeFinderWindow.BaseGraphProvider
    {
        public override string Name => "StateMachines";

        public StateMachineProvider(NodeFinderWindow window) : base(window) { }

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            foreach (var machine in UnityObjectUtility.FindObjectsOfTypeIncludingInactive<StateMachine>().Where(_asset => _asset.nest.source == GraphSource.Embed))
            {
                if (machine?.GetReference().graph is not FlowGraph) continue;

                var baseRef = machine.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseFlowGraph(baseRef))
                {
                    yield return element;
                }
            }
        }

        public override void HandleMatch(NodeFinderWindow.MatchObject match)
        {
            var asset = match.StateGraphAsset;
            if (asset != null)
            {
                AddMatch(match, asset);
            }
        }

        protected override string GetSortKey(Object key)
        {
            return (key as StateMachine).graph.title ?? base.GetSortKey(key);
        }
    }

    public class ClassAssetProvider : NodeFinderWindow.BaseGraphProvider
    {
        public override string Name => "ClassAssets";

        public ClassAssetProvider(NodeFinderWindow window) : base(window) { }

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:ClassAsset", null);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ClassAsset>(assetPath);
                if (asset == null) continue;

                foreach (var reference in _window.GetReferences(asset))
                {
                    foreach (var element in GraphTraversal.TraverseFlowGraph(reference))
                    {
                        yield return element;
                    }
                }
            }
        }

        public override void HandleMatch(NodeFinderWindow.MatchObject match)
        {
            var asset = match.ClassAsset;
            if (asset != null)
            {
                AddMatch(match, asset);
            }
        }
    }

    public class StructAssetProvider : NodeFinderWindow.BaseGraphProvider
    {
        public override string Name => "StructAssets";

        public StructAssetProvider(NodeFinderWindow window) : base(window) { }

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:StructAsset", null);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<StructAsset>(assetPath);
                if (asset == null) continue;

                foreach (var reference in _window.GetReferences(asset))
                {
                    foreach (var element in GraphTraversal.TraverseFlowGraph(reference))
                    {
                        yield return element;
                    }
                }
            }
        }

        public override void HandleMatch(NodeFinderWindow.MatchObject match)
        {
            var asset = match.StructAsset;
            if (asset != null)
            {
                AddMatch(match, asset);
            }
        }
    }

    public class UnitMatchHandler : IMatchHandler
    {
        public bool CanHandle(IGraphElement element)
        {
            return element is Unit && element is not CommentNode;
        }

        public NodeFinderWindow.MatchObject HandleMatch(IGraphElement element, string pattern)
        {
            if (element is Unit unit)
            {
                var matchRecord = new NodeFinderWindow.MatchObject
                {
                    Matches = new List<NodeFinderWindow.MatchType>(),
                    Unit = unit,
                    FullTypeName = GetFullNameWithInputs(unit, GetUnitFullName(unit))
                };

                // Check for matches in the unit name
                if (SearchUtility.Matches(pattern, matchRecord.FullTypeName))
                {
                    matchRecord.Matches.Add(NodeFinderWindow.MatchType.Unit);
                }

                // Check member unit
                if (unit is MemberUnit && SearchUtility.Matches(pattern, GetUnitFullName(unit)))
                {
                    matchRecord.Matches.Add(NodeFinderWindow.MatchType.Unit);
                }

                if (unit.valueInputs.Count > 0)
                {
                    var inputValues = GetFullNameWithInputs(unit, matchRecord.FullTypeName);
                    if (SearchUtility.Matches(pattern, inputValues))
                    {
                        matchRecord.Matches.Add(NodeFinderWindow.MatchType.Unit);
                    }
                }

                // Check literal values
                if (unit is Literal literal)
                {
                    if (literal.value != null && SearchUtility.Matches(pattern, literal.value.ToString()))
                    {
                        matchRecord.Matches.Add(NodeFinderWindow.MatchType.Unit);
                    }
                }

                if (matchRecord.Matches.Count > 0)
                {
                    return matchRecord;
                }
            }
            return null;
        }

        private string GetValue(ValueInput valueInput)
        {
            if (valueInput.hasDefaultValue && !valueInput.hasAnyConnection)
            {
                return $"{valueInput.key.LegalMemberName().Prettify()} : " + (!valueInput.nullMeansSelf && !(typeof(Component).IsAssignableFrom(valueInput.type) || valueInput.type == typeof(GameObject)) ? valueInput.unit.defaultValues[valueInput.key] is Type type ? type.HumanName() : (valueInput.unit.defaultValues[valueInput.key] is Object obj ? obj.name : valueInput.unit.defaultValues[valueInput.key]?.ToString()) ?? "null" : "This");
            }
            else if (valueInput.hasAnyConnection)
            {
                return valueInput.key.LegalMemberName().Prettify() + " : " + "Connected";
            }
            return valueInput.key.LegalMemberName().Prettify() + " : " + "No Value";
        }

        private string GetUnitFullName(Unit unit)
        {
            var typeName = GetUnitName(unit);

            if (unit is MemberUnit member && member.member.targetType != null)
            {
                typeName = member.member.ToPseudoDeclarer().ToString();
            }

            return typeName;
        }

        private string GetUnitName(IGraphElement element)
        {
            if (element is Unit unit)
            {
                if (unit is GraphOutput or GraphInput)
                {
                    return unit.GetType().HumanName();
                }
                if (unit is SubgraphUnit subgraphUnit)
                {
                    if (subgraphUnit.nest.source == GraphSource.Embed)
                    {
                        return !string.IsNullOrEmpty(subgraphUnit.nest.graph.title) ? subgraphUnit.nest.graph.title : "Unnamed Subgraph";
                    }
                    else
                    {
                        return !string.IsNullOrEmpty(subgraphUnit.nest.graph.title) ? subgraphUnit.nest.graph.title : !string.IsNullOrEmpty(subgraphUnit.nest.macro.name) ? subgraphUnit.nest.macro.name : "Unnamed Subgraph";
                    }
                }
                return BoltFlowNameUtility.UnitTitle(unit.GetType(), false, false);
            }
            else if (element is not null) return element.Descriptor().description.title;
            return "Invalid Element";
        }

        private string GetFullNameWithInputs(Unit unit, string baseName)
        {
            if (unit is null) return "";
            if (unit is Literal literal)
            {
                return $"{baseName} (Type : {literal.type.As().CSharpName(false, false, false)}, Value : {literal.value ?? "No Value"})";
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
    }

    public class GroupMatchHandler : IMatchHandler
    {
        public bool CanHandle(IGraphElement element)
        {
            return element is GraphGroup;
        }

        public NodeFinderWindow.MatchObject HandleMatch(IGraphElement element, string pattern)
        {
            if (element is GraphGroup group)
            {
                var matchRecord = new NodeFinderWindow.MatchObject
                {
                    Matches = new List<NodeFinderWindow.MatchType>(),
                    group = group,
                    FullTypeName = GetGroupFullName(group)
                };

                if (SearchUtility.Matches(pattern, matchRecord.FullTypeName))
                {
                    matchRecord.Matches.Add(NodeFinderWindow.MatchType.Group);
                    return matchRecord;
                }
            }
            return null;
        }

        private string GetGroupFullName(GraphGroup group)
        {
            if (!string.IsNullOrEmpty(group.label) && !string.IsNullOrEmpty(group.comment))
            {
                return "Graph Group : " + group.label + "." + group.comment;
            }
            else if (!string.IsNullOrEmpty(group.label))
            {
                return "Graph Group : " + group.label;
            }
            else if (!string.IsNullOrEmpty(group.comment))
            {
                return "Graph Group : " + group.comment;
            }
            return "Unnamed Graph Group";
        }
    }

#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
    public class StickyNoteMatchHandler : IMatchHandler
    {
        public bool CanHandle(IGraphElement element)
        {
            return element is StickyNote;
        }

        public NodeFinderWindow.MatchObject HandleMatch(IGraphElement element, string pattern)
        {
            if (element is StickyNote note)
            {
                var matchRecord = new NodeFinderWindow.MatchObject
                {
                    Matches = new List<NodeFinderWindow.MatchType>(),
                    stickyNote = note,
                    FullTypeName = GetStickyNoteFullName(note)
                };

                if (SearchUtility.Matches(pattern, matchRecord.FullTypeName))
                {
                    matchRecord.Matches.Add(NodeFinderWindow.MatchType.StickyNote);
                    return matchRecord;
                }
            }
            return null;
        }

        private string GetStickyNoteFullName(StickyNote note)
        {
            if (!string.IsNullOrEmpty(note.title) && !string.IsNullOrEmpty(note.body))
            {
                return "StickyNote : " + note.title + "." + note.body;
            }
            else if (!string.IsNullOrEmpty(note.title))
            {
                return "StickyNote : " + note.title;
            }
            else if (!string.IsNullOrEmpty(note.body))
            {
                return "StickyNote : " + note.body;
            }
            return "Empty StickyNote";
        }

    }
#endif

    public class CommentsMatchHandler : IMatchHandler
    {
        public bool CanHandle(IGraphElement element)
        {
            return element is CommentNode;
        }

        public NodeFinderWindow.MatchObject HandleMatch(IGraphElement element, string pattern)
        {
            if (element is CommentNode comment)
            {
                var matchRecord = new NodeFinderWindow.MatchObject
                {
                    Matches = new List<NodeFinderWindow.MatchType>(),
                    comment = comment,
                    FullTypeName = GetCommentFullName(comment)
                };

                if (SearchUtility.Matches(pattern, matchRecord.FullTypeName))
                {
                    matchRecord.Matches.Add(NodeFinderWindow.MatchType.Comment);
                    return matchRecord;
                }
            }
            return null;
        }

        private string GetCommentFullName(CommentNode note)
        {
            if (!string.IsNullOrEmpty(note.title) && !string.IsNullOrEmpty(note.comment))
            {
                return "Comment : " + note.title + "." + note.comment;
            }
            else if (!string.IsNullOrEmpty(note.title))
            {
                return "Comment : " + note.title;
            }
            else if (!string.IsNullOrEmpty(note.comment))
            {
                return "Comment : " + note.comment;
            }
            return "Empty Comment";
        }
    }

    public class ErrorMatchHandler : IMatchHandler
    {
        public GraphPointer graphPointer;
        public bool CanHandle(IGraphElement element)
        {
            return graphPointer != null && element is Unit unit && IsErrorUnit(unit);
        }
        private bool IsErrorUnit(Unit unit)
        {
            if (unit.GetException(graphPointer) != null)
                return true;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (unit is MissingType)
                return true;
#endif
            return false;
        }
        public NodeFinderWindow.MatchObject HandleMatch(IGraphElement element, string pattern)
        {
            if (element is Unit unit)
            {
                var matchRecord = new NodeFinderWindow.MatchObject
                {
                    Matches = new List<NodeFinderWindow.MatchType>(),
                    Unit = unit,
                    FullTypeName = GetUnitFullName(unit)
                };

                if (IsErrorUnit(unit))
                {
                    matchRecord.Matches.Add(NodeFinderWindow.MatchType.Error);
                    return matchRecord;
                }
            }
            return null;
        }

        private string GetUnitFullName(Unit unit)
        {
            var typeName = GetUnitName(unit);

            if (unit is MemberUnit member && member.member.targetType != null)
            {
                typeName = member.member.ToPseudoDeclarer().ToString();
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (unit is MissingType missingType)
            {
                typeName = string.IsNullOrEmpty(missingType.formerType) ? "Missing Type" : "Missing Type : " + missingType.formerType;
            }
#endif
            if (unit.GetException(graphPointer) != null)
            {
                typeName += " Error : " + unit.GetException(graphPointer).Message;
            }
            return typeName;
        }

        private string GetUnitName(IGraphElement element)
        {
            if (element is Unit unit)
            {
                if (unit is GraphOutput or GraphInput)
                {
                    return unit.GetType().HumanName();
                }
                if (unit is SubgraphUnit subgraphUnit)
                {
                    if (subgraphUnit.nest.source == GraphSource.Embed)
                    {
                        return !string.IsNullOrEmpty(subgraphUnit.nest.graph.title) ? subgraphUnit.nest.graph.title : "Unnamed Subgraph";
                    }
                    else
                    {
                        return !string.IsNullOrEmpty(subgraphUnit.nest.graph.title) ? subgraphUnit.nest.graph.title : !string.IsNullOrEmpty(subgraphUnit.nest.macro.name) ? subgraphUnit.nest.macro.name : "Unnamed Subgraph";
                    }
                }
                return BoltFlowNameUtility.UnitTitle(unit.GetType(), false, false);
            }

            return "Invalid Element";
        }
    }
}
