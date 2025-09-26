using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    public class NodeFinderWindow : EditorWindow
    {
        public enum SearchMode
        {
            Contains,
            StartsWith,
            // Exact
        }

        private Dictionary<Type, IGraphProvider> _graphProviders = new Dictionary<Type, IGraphProvider>();
        private readonly Dictionary<Type, IMatchHandler> _matchHandlers = new Dictionary<Type, IMatchHandler>();

        [NonSerialized]
        bool _loadedStates = false;

        public ICollection<KeyValuePair<Type, IGraphProvider>> GetProviders()
        {
            return _graphProviders.AsReadOnlyCollection();
        }

        private void OnDisable()
        {
            _cachedGUI.Clear();
        }

        private void OnEnable()
        {
            RegisterProvidersFromAttributes();
            _graphProviders = _graphProviders.OrderBy(kvp => kvp.Value.Order).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            RegisterHandlersFromAttributes();
            LoadStates();
            Search();
        }

        private void RegisterProvider<T>(T provider) where T : IGraphProvider
        {
            _graphProviders[provider.GetType()] = provider;
        }

        private void RegisterHandler<T>(T handler) where T : IMatchHandler
        {
            _matchHandlers[handler.SupportedType] = handler;
        }

        private void RegisterProvidersFromAttributes()
        {
            var providerTypes = Codebase.editorTypes.Where(t => !t.IsAbstract && typeof(IGraphProvider).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<GraphProviderAttribute>() != null);

            foreach (var type in providerTypes)
            {
                try
                {
                    var provider = (IGraphProvider)Activator.CreateInstance(type, new object[] { this });
                    RegisterProvider(provider);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to register provider {type.Name}: {ex}");
                }
            }
        }

        private void RegisterHandlersFromAttributes()
        {
            var handlerTypes = Codebase.editorTypes.Where(t => !t.IsAbstract && typeof(IMatchHandler).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<MatchHandlerAttribute>() != null);

            foreach (var type in handlerTypes)
            {
                try
                {
                    var handler = (IMatchHandler)Activator.CreateInstance(type, false);
                    RegisterHandler(handler);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to register handler {type.Name}: {ex}");
                }
            }
        }

        [SerializeField]
        private Vector2 _scroll;

        [SerializeField] private string _searchQuery = "";

        private GUIStyle _buttonStyle;

        private MatchObject _selectedMatch;
        [SerializeField]
        private SearchMode _searchMode;

        [MenuItem("Window/Community Addons/Node Finder &f")]
        public static NodeFinderWindow Open()
        {
            var window = GetWindow<NodeFinderWindow>();
            window.titleContent = new GUIContent("Node Finder");

            GUIContent searchIconContent = EditorGUIUtility.IconContent("d_ViewToolZoom");
            window.titleContent = new GUIContent("Node Finder", searchIconContent.image);
            window._focusSearchNextFrame = true;
            return window;
        }

        private void Init()
        {
            LoadStates();
            _buttonStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                wordWrap = false,
#if UNITY_2021_2_OR_NEWER
                clipping = TextClipping.Ellipsis
#else
                clipping = TextClipping.Clip
#endif
            };
        }

        private class CachedMatchGUI
        {
            public MatchObject Match;
            public GUIContent RowContent;
            public GUIContent GroupContent;
            public GUIContent PathContent;
            public int Depth;
            public GraphGroup Group;

            public bool MatchIsInGroup => Group != null;
        }

        private readonly List<CachedMatchGUI> _cachedGUI = new List<CachedMatchGUI>();

        private const string SearchControlName = "CommunityAddons_NodeFinder_SearchTextField";
        private bool _focusSearchNextFrame;

        private void OnGUI()
        {
            Init();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(Temp("Search:", null, EvaluateQuery(_searchQuery)), GUILayout.Width(50));
                GUI.SetNextControlName(SearchControlName);
                var newQuery = EditorGUILayout.DelayedTextField(_searchQuery, GUILayout.MinWidth(150));

                if (_focusSearchNextFrame)
                {
                    _focusSearchNextFrame = false;

                    EditorGUI.FocusTextInControl(SearchControlName);
                    EditorApplication.delayCall += () =>
                    {
                        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        if (editor != null)
                        {
                            editor.SelectAll();
                        }
                    };
                }

                if (_searchQuery != newQuery)
                {
                    _searchQuery = newQuery;
                    Search();
                }

                if (GUILayout.Button("Search", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    GUI.FocusControl(null);
                    _searchQuery = newQuery;
                    Search();
                }

                var prevSearchMode = _searchMode;

                _searchMode = (SearchMode)EditorGUILayout.EnumPopup(_searchMode, GUILayout.Width(100));

                if (prevSearchMode != _searchMode)
                {
                    GUI.FocusControl(null);
                    Search();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Temp("Types", null, "Type of elements to look for"), EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    GUI.FocusControl(null);
                    PopupWindow.Show(
                        new Rect(Event.current.mousePosition, Vector2.zero),
                        new ToggleMenu<IMatchHandler>(
                            _matchHandlers.Values,
                            h => { h.SetEnabled(!h.IsEnabled); SaveStates(); Search(); },
                            h => h.Name,
                            h => h.IsEnabled
                        )
                    );
                }

                var currentGraph = _graphProviders[typeof(CurrentGraphProvider)];
                bool currentGraphActive = currentGraph != null && currentGraph.IsEnabled;

                Color oldColor = GUI.backgroundColor;
                if (currentGraphActive)
                    GUI.backgroundColor = new Color(102, 102, 102, 0.07f);

                string buttonLabel = currentGraphActive ? "This Graph" : "All Graphs";
                string tooltip = currentGraphActive ? "Only search graph that is currently open" : "Search all enabled graph providers";

                Rect fullRect = GUILayoutUtility.GetRect(new GUIContent(buttonLabel, tooltip), EditorStyles.toolbarButton, GUILayout.Width(100));

                Rect labelRect = fullRect;
                labelRect.width -= 16f;

                Rect arrowRect = fullRect;
                arrowRect.x = labelRect.xMax;
                arrowRect.width = 16f;

                if (GUI.Button(labelRect, new GUIContent(currentGraphActive ? "This Graph" : "All Graphs", tooltip), EditorStyles.toolbarButton))
                {
                    if (currentGraphActive)
                    {
                        currentGraph?.SetEnabled(false);
                        GUI.FocusControl(null);
                        SaveStates();
                        Search();
                    }
                    else
                    {
                        currentGraph?.SetEnabled(true);
                        SaveStates();
                        Search();
                    }
                }
                EditorGUI.BeginDisabledGroup(currentGraphActive);
                if (GUI.Button(arrowRect, GUIContent.none, EditorStyles.toolbarDropDown))
                {
                    PopupWindow.Show(
                        new Rect(Event.current.mousePosition, Vector2.zero),
                        new ToggleMenu<IGraphProvider>(
                            _graphProviders.Values.Where(p => !(p is CurrentGraphProvider)),
                            p =>
                            {
                                p.ToggleProvider();
                                SaveStates();
                                Search();
                            },
                            p => p.Name,
                            p => p.IsEnabled
                        )
                    );
                }
                EditorGUI.EndDisabledGroup();

                GUI.backgroundColor = oldColor;

                if (GUILayout.Button(Temp("?", null, "Show help popup with info on how the search works"), EditorStyles.toolbarButton, GUILayout.Width(30)))
                {
                    GUI.FocusControl(null);
                    PopupWindow.Show(
                        new Rect(new Vector2(Event.current.mousePosition.x + 30, Event.current.mousePosition.y + 30), Vector2.zero),
                        new HelpPopup()
                    );
                }

            }

            const float padding = 35;

            const float lineHeight = 22f;

            const float lineSpacing = 4f;

            float headerHeight = lineHeight + lineSpacing;
            float rowsHeight = (_cachedGUI.Count * lineHeight) + ((_cachedGUI.Count - 1) * lineSpacing);
            float totalHeight = rowsHeight + headerHeight;

            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(position.height - headerHeight + lineSpacing));
            EditorGUILayout.LabelField($"Results: {_cachedGUI.Count}", EditorStyles.boldLabel);

            Rect viewRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - padding, totalHeight, GUILayout.ExpandHeight(false));

            float columnWidth = (EditorGUIUtility.currentViewWidth - padding) / 3f;

            Rect headerRect = new Rect(viewRect.x + 5, viewRect.y, viewRect.width - 10, lineHeight);

            GUI.Box(headerRect, GUIContent.none);
            EditorGUI.LabelField(new Rect(headerRect.x + 5, headerRect.y, columnWidth, lineHeight), "Element", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft });
            EditorGUI.LabelField(new Rect(headerRect.x + columnWidth, headerRect.y, columnWidth, lineHeight), "Group", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter });
            EditorGUI.LabelField(new Rect(headerRect.x + columnWidth * 2, headerRect.y, columnWidth, lineHeight), "Path", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight });

            viewRect.y += lineHeight + lineSpacing;

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt((_scroll.y - lineHeight) / (lineHeight + lineSpacing)));
            int lastVisible = Mathf.Min(_cachedGUI.Count - 1, Mathf.CeilToInt((_scroll.y + position.height) / (lineHeight + lineSpacing)));

            HashSet<CachedMatchGUI> invalidElements = new HashSet<CachedMatchGUI>();

            const float indentPerLevel = 15f;

            for (int i = firstVisible; i <= lastVisible; i++)
            {
                var cached = _cachedGUI[i];
                if (!cached.Match.Reference.isValid || !cached.Match.Reference.Context().canvas.widgetProvider.IsValid(cached.Match.Element))
                {
                    invalidElements.Add(cached);
                    continue;
                }

                float y = viewRect.y + i * (lineHeight + lineSpacing);
                Rect rowRect = new Rect(viewRect.x + 5, y, viewRect.width - 10, lineHeight);

                for (int d = 1; d <= cached.Depth; d++)
                {
                    float lineX = rowRect.x + indentPerLevel * d - indentPerLevel / 2;
                    Handles.color = Color.gray;
                    Handles.DrawLine(
                        new Vector3(lineX, rowRect.y, 0),
                        new Vector3(lineX, rowRect.y + rowRect.height + lineSpacing, 0)
                    );
                }

                Color oldColor = GUI.backgroundColor;

                if (cached.Match.IsErrorUnit) GUI.backgroundColor = Color.red;
                if (cached.Match == _selectedMatch) GUI.backgroundColor = Color.black;

                GUI.Box(rowRect, GUIContent.none);
                GUI.backgroundColor = oldColor;

                float xOffset = indentPerLevel * cached.Depth;
                Rect elementRect = new Rect(rowRect.x + xOffset, rowRect.y, columnWidth - xOffset, lineHeight);

                _buttonStyle.alignment = TextAnchor.MiddleLeft;
                _buttonStyle.normal.textColor = Color.white;
                _buttonStyle.clipping = TextClipping.Overflow;

                string fullText = cached.RowContent.text;
                float textWidth = _buttonStyle.CalcSize(Temp(fullText, null)).x;
#if UNITY_2021_2_OR_NEWER
                _buttonStyle.clipping = TextClipping.Ellipsis;
#else
                _buttonStyle.clipping = TextClipping.Clip;
#endif
                GUIContent elementContent = Temp(fullText, cached.RowContent.image,
                    cached.RowContent.tooltip + (textWidth > elementRect.width
                        ? "\n\nFullText: " + fullText.Replace("<b>", string.Empty).Replace("</b>", string.Empty)
                        : ""));

                if (GUI.Button(elementRect, elementContent, _buttonStyle))
                {
                    GUI.FocusControl(null);
                    FocusMatchObject(cached.Match);
                    _selectedMatch = cached.Match;
                }

                _buttonStyle.alignment = TextAnchor.MiddleCenter;
                Rect groupRect = new Rect(rowRect.x + columnWidth, rowRect.y, columnWidth, lineHeight);
                if (cached.MatchIsInGroup && GUI.Button(groupRect, cached.GroupContent, _buttonStyle))
                {
                    GUI.FocusControl(null);
                    var groupMatch = new MatchObject(cached.Group, "");
                    groupMatch.Initialize(cached.Match.Reference);
                    FocusMatchObject(groupMatch);
                    _selectedMatch = cached.Match;
                }
                else if (GUI.Button(groupRect, "", _buttonStyle))
                {
                    GUI.FocusControl(null);
                    FocusMatchObject(cached.Match);
                    _selectedMatch = cached.Match;
                }

                _buttonStyle.alignment = TextAnchor.MiddleRight;
                _buttonStyle.normal.textColor = Color.gray;
                Rect pathRect = new Rect(rowRect.x + columnWidth * 2, rowRect.y, columnWidth, lineHeight);
                if (GUI.Button(pathRect, cached.PathContent, _buttonStyle))
                {
                    GUI.FocusControl(null);
                    HighlightObject(cached.Match);
                    _selectedMatch = cached.Match;
                }
            }

            foreach (var cached in invalidElements)
            {
                _cachedGUI.Remove(cached);
            }
            EditorGUILayout.EndScrollView();
        }

        private string EvaluateQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
                return "";

            var groups = query
                .Split('|')
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            List<string> groupDescriptions = new List<string>();

            foreach (var group in groups)
            {
                var rawParts = group.Split('>');
                var parts = rawParts.Where(p => !string.IsNullOrEmpty(p)).Take(2).Select(p => Community.SearchUtility.Normalize(p.Trim())).ToArray();
                string unitQuery = Community.SearchUtility.Normalize(parts[0].Trim());
                string portQuery = parts.Length > 1 ? parts[1].Trim() : null;

                string portTag = null;
                if (unitQuery.EndsWith("@CI", StringComparison.OrdinalIgnoreCase)) { portTag = "Control Input"; unitQuery = unitQuery.Substring(0, unitQuery.Length - 3); }
                else if (unitQuery.EndsWith("@CO", StringComparison.OrdinalIgnoreCase)) { portTag = "Control Output"; unitQuery = unitQuery.Substring(0, unitQuery.Length - 3); }
                else if (unitQuery.EndsWith("@VI", StringComparison.OrdinalIgnoreCase)) { portTag = "Value Input"; unitQuery = unitQuery.Substring(0, unitQuery.Length - 3); }
                else if (unitQuery.EndsWith("@VO", StringComparison.OrdinalIgnoreCase)) { portTag = "Value Output"; unitQuery = unitQuery.Substring(0, unitQuery.Length - 3); }
                else if (unitQuery.EndsWith("@I", StringComparison.OrdinalIgnoreCase)) { portTag = "Input"; unitQuery = unitQuery.Substring(0, unitQuery.Length - 2); }
                else if (unitQuery.EndsWith("@O", StringComparison.OrdinalIgnoreCase)) { portTag = "Output"; unitQuery = unitQuery.Substring(0, unitQuery.Length - 2); }

                string description = string.Empty;

                if (unitQuery == "*")
                {
                    description = "Any element";
                }
                else if (!string.IsNullOrEmpty(unitQuery))
                {
                    description = $"Element {(_searchMode == SearchMode.Contains ? "containing" : "starting with")} '{unitQuery}'";
                }

                if (portQuery != null)
                {
                    if (!string.IsNullOrEmpty(portTag))
                        description += $" with a connected {portTag} matching '{EvaluateQuery(portQuery)}'";
                    else
                        description += $" with any connected port matching '{EvaluateQuery(portQuery)}'";
                }

                groupDescriptions.Add(description);
            }

            return string.Join("\n\nOR\n\n", groupDescriptions);
        }

        private Vector2 ExtractElementPosition(IGraphElement element)
        {
            if (element is GraphGroup group) return group.position.position;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (element is StickyNote note) return note.position.position;
#endif
            if (element is Unit unit) return unit.position;

            if (element is State state) return state.position;

            return default;
        }

        #region Search
        private void Search()
        {
            _cachedGUI.Clear();
            var visited = new HashSet<IGraphElement>();

            void AddMatchRecursive(IGraphElement element, MatchObject match, GraphReference reference, int depth = 0)
            {
                if (element == null || match == null || visited.Contains(element))
                    return;

                visited.Add(element);
                match.Initialize(reference);
                CacheMatchObject(element, match, depth);

                if (match.SubMatches != null)
                {
                    foreach (var sub in match.SubMatches)
                    {
                        AddMatchRecursive(sub.Element, sub, reference, depth + 1);
                    }
                }
            }

            if (_graphProviders.TryGetValue(typeof(CurrentGraphProvider), out var graphProvider) && graphProvider.IsEnabled)
            {
                foreach (var (reference, element) in graphProvider.GetElements())
                {
                    if (TryGetHandlerForElement(element, reference, out IMatchHandler handler))
                    {
                        if (!handler.IsEnabled) continue;

                        if (handler is ErrorMatchHandler errorHandler)
                        {
                            errorHandler.graphPointer = reference;
                            var errorMatch = errorHandler.HandleMatch(element, _searchQuery, _searchMode);
                            if (errorMatch != null)
                            {
                                AddMatchRecursive(element, errorMatch, reference);
                            }
                            continue;
                        }

                        if (string.IsNullOrEmpty(_searchQuery)) continue;
                        if (!handler.CanHandle(element)) continue;

                        var match = handler.HandleMatch(element, _searchQuery, _searchMode);
                        if (match != null)
                        {
                            AddMatchRecursive(element, match, reference);
                        }
                    }
                }
                return;
            }

            foreach (var provider in _graphProviders.Values)
            {
                if (provider is CurrentGraphProvider || !provider.IsEnabled) continue;

                foreach (var (reference, element) in provider.GetElements())
                {
                    if (TryGetHandlerForElement(element, reference, out IMatchHandler handler))
                    {
                        if (!handler.IsEnabled) continue;

                        if (handler is ErrorMatchHandler errorHandler)
                        {
                            errorHandler.graphPointer = reference;
                            var errorMatch = errorHandler.HandleMatch(element, _searchQuery, _searchMode);
                            if (errorMatch != null)
                            {
                                AddMatchRecursive(element, errorMatch, reference);
                            }
                            continue;
                        }

                        if (string.IsNullOrEmpty(_searchQuery)) continue;
                        if (!handler.CanHandle(element)) continue;

                        var match = handler.HandleMatch(element, _searchQuery, _searchMode);
                        if (match != null)
                        {
                            AddMatchRecursive(element, match, reference);
                        }
                    }
                }
            }

            Repaint();
        }

        private void CacheMatchObject(IGraphElement element, MatchObject match, int depth)
        {
            GraphElementCollection<GraphGroup> groups = element.graph is FlowGraph fgraph ? fgraph.groups : element.graph is StateGraph sgraph ? sgraph.groups : null;
            GraphGroup group = groups?.FirstOrDefault(gr => element != gr && gr.position.Contains(ExtractElementPosition(element)));
            bool inGroup = group != null;

            var sourceInfo = GetSourceInfo(match.Target, match.Reference);

            GUIContent rowContent = new GUIContent(match.MatchString(_searchQuery, _searchMode, match.Element is Unit unit ? unit : null), GetElementIcon(element), "ElementPath: " + sourceInfo.text);

            GUIContent groupContent = inGroup ? new GUIContent(GroupMatchHandler.GetGroupFullName(group), GetElementIcon(group)) : null;

            GUIContent pathContent = new GUIContent("<i>" + match.GraphPath + "</i>", sourceInfo.image);

            _cachedGUI.Add(new CachedMatchGUI
            {
                Match = match,
                RowContent = rowContent,
                GroupContent = groupContent,
                PathContent = pathContent,
                Group = group,
                Depth = depth
            });
        }

        private Dictionary<Type, IMatchHandler> cachedElementHandlers = new Dictionary<Type, IMatchHandler>();
        private bool TryGetHandlerForElement(IGraphElement element, GraphReference reference, out IMatchHandler handler)
        {
            if (element is Unit unit)
            {
                if (unit.GetException(reference) != null)
                {
                    handler = _matchHandlers[typeof(ErrorMatchHandler)];
                    return true;
                }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
                if (unit is MissingType)
                {
                    handler = _matchHandlers[typeof(ErrorMatchHandler)];
                    return true;
                }
#endif
            }

            var type = element.GetType();

            if (cachedElementHandlers.TryGetValue(type, out handler)) return true;

            while (type != null)
            {
                if (_matchHandlers.TryGetValue(type, out handler))
                {
                    cachedElementHandlers[type] = handler;
                    return true;
                }

                type = type.BaseType;
            }

            handler = null;
            return false;
        }

        #endregion

        #region Helpers
        private Texture2D GetElementIcon(IGraphElement element)
        {
            if (element is null) return typeof(Null).Icon()[IconSize.Small];
            if (element is MemberUnit)
            {
                if (element is InvokeMember invokeMember)
                {
                    var descriptor = invokeMember.Descriptor<InvokeMemberDescriptor>();
                    return descriptor.Icon()[IconSize.Small];
                }
                else if (element is GetMember getMember)
                {
                    var descriptor = getMember.Descriptor<GetMemberDescriptor>();
                    return descriptor.Icon()[IconSize.Small];
                }
                else
                {
                    var descriptor = element.Descriptor<SetMemberDescriptor>();
                    return descriptor.Icon()[IconSize.Small];
                }
            }
            else if (element is Literal literal)
            {
                var descriptor = literal.Descriptor<LiteralDescriptor>();
                return descriptor.Icon()[IconSize.Small];
            }
            else if (element is UnifiedVariableUnit unifiedVariableUnit)
            {
                var descriptor = unifiedVariableUnit.Descriptor<UnitDescriptor<UnifiedVariableUnit>>();
                return descriptor.Icon()[IconSize.Small];
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (element is StickyNote)
            {
                return typeof(StickyNote).Icon()[IconSize.Small];
            }
#endif
            else if (element is GraphGroup)
            {
                return typeof(GraphGroup).Icon()[IconSize.Small];
            }
            else if (element is CommentNode commentNode)
            {
                var descriptor = commentNode.Descriptor<CommentDescriptor>();
                return descriptor.Icon()[IconSize.Small];
            }
            else
            {
                var icon = element.GetType().Icon();
                return icon[IconSize.Small];
            }
        }

        private static readonly GUIContent _tempContent = new GUIContent();
        private const string PathDivider = " -> ";
        private GUIContent GetSourceInfo(Object key, GraphReference reference)
        {
            if (key is ScriptGraphAsset)
            {
                return Temp(GraphTraversal.GetElementPath(reference), GetIcon<ScriptGraphAsset>());
            }
            else if (key is StateGraphAsset)
            {
                return Temp(GraphTraversal.GetElementPath(reference), GetIcon<StateGraphAsset>());
            }
            else if (key is IEventMachine || key is GameObject)
            {
                return Temp(GraphTraversal.GetElementPath(reference), GetIcon<GameObject>());
            }
            else if (key is ClassAsset classAsset)
            {
                return classAsset.icon != null
                    ? Temp((classAsset.title ?? classAsset.name) + PathDivider + GraphTraversal.GetElementPath(reference), classAsset.icon)
                    : Temp((classAsset.title ?? classAsset.name) + PathDivider + GraphTraversal.GetElementPath(reference), GetIcon<ClassAsset>());
            }
            else if (key is StructAsset structAsset)
            {
                return structAsset.icon != null
                    ? Temp((structAsset.title ?? structAsset.name) + PathDivider + GraphTraversal.GetElementPath(reference), structAsset.icon)
                    : Temp((structAsset.title ?? structAsset.name) + PathDivider + GraphTraversal.GetElementPath(reference), GetIcon<StructAsset>());
            }
            else
            {
                return Temp(key.name, key.GetType().Icon()[IconSize.Small]);
            }
        }

        private Texture GetIcon<T>()
        {
            return typeof(T).Icon()[IconSize.Small];
        }

        private GUIContent Temp(string text, Texture image, string toolTip = null)
        {
            _tempContent.text = text;
            _tempContent.image = image;
            _tempContent.tooltip = toolTip;
            return _tempContent;
        }

        private void FocusMatchObject(MatchObject match)
        {
            GraphUtility.OverrideContextIfNeeded(() =>
            {
                GraphWindow.OpenActive(match.Reference);
                var context = match.Reference.Context();
                if (context == null) return;

                Selection.activeObject = match.Target;
                context.BeginEdit();
                context.canvas?.ViewElements(match.Element.Yield());
                context.EndEdit();
            });
        }

        private void HighlightObject(MatchObject match)
        {
            if (match.Target != null)
            {
                EditorGUIUtility.PingObject(match.Target);
                Selection.activeObject = match.Target;
            }
        }
        #endregion

        private void SaveStates()
        {
            foreach (var handler in _matchHandlers.Values)
            {
                EditorPrefs.SetBool($"NodeFinder.Handler.{handler.GetType().FullName}", handler.IsEnabled);
            }
            foreach (var provider in _graphProviders.Values)
            {
                EditorPrefs.SetBool($"NodeFinder.Provider.{provider.GetType().FullName}", provider.IsEnabled);
            }
        }

        private void LoadStates()
        {
            if (_loadedStates) return;

            _loadedStates = true;

            foreach (var handler in _matchHandlers.Values)
            {
                handler.SetEnabled(EditorPrefs.GetBool($"NodeFinder.Handler.{handler.GetType().FullName}", true));
            }
            foreach (var provider in _graphProviders.Values)
            {
                provider.SetEnabled(EditorPrefs.GetBool($"NodeFinder.Provider.{provider.GetType().FullName}", true));
            }
        }

        private class ToggleMenu<T> : PopupWindowContent
        {
            private readonly IEnumerable<T> items;
            private readonly Action<T> onToggle;
            private readonly Func<T, string> getName;
            private readonly Func<T, bool> getState;

            public ToggleMenu(IEnumerable<T> items, Action<T> onToggle, Func<T, string> getName, Func<T, bool> getState)
            {
                this.items = items;
                this.onToggle = onToggle;
                this.getName = getName;
                this.getState = getState;
            }

            public override Vector2 GetWindowSize() => new Vector2(200, items.Count() * 20 + 5);

            public override void OnGUI(Rect rect)
            {
                foreach (var item in items)
                {
                    bool newState = EditorGUILayout.ToggleLeft(getName(item), getState(item));
                    if (newState != getState(item))
                    {
                        onToggle(item);
                    }
                }
            }
        }

        private class HelpPopup : PopupWindowContent
        {
            private Vector2 _scroll;

            public override Vector2 GetWindowSize() => new Vector2(450, 450);

            public override void OnGUI(Rect rect)
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);

                GUILayout.Label("Node Finder Help", EditorStyles.boldLabel);
                GUILayout.Space(5);

                GUILayout.Label("Search Basics", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "• Enter a search term to filter results.\n" +
                    "• Search Modes:\n" +
                    "    - Contains: matches if text contains the query.\n" +
                    "    - Starts With: matches if text begins with the query.\n" +
                    "• Use '*' (asterisk) to match everything, ignoring the search mode.",
                    MessageType.Info
                );

                GUILayout.Space(5);

                GUILayout.Label("Advanced Search", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Advanced search lets you combine multiple conditions:\n\n" +
                    "• Use '|' (pipe) to separate alternative groups. Any group can match.\n" +
                    "   Example:  Move | Jump   → matches elements containing 'Move' OR 'Jump'\n\n" +
                    "• Use '>' (arrow) to match connected ports of a unit.\n" +
                    "   Example:  Player > Target  → matches a unit named 'Player' with a port connected to something matching 'Target'\n\n" +
                    "• You can group Connection searches:\n" +
                    "   Move > Position | Rotate > Angle\n" +
                    "   → matches (Move connected to Position) OR (Rotate connected to Angle)\n\n" +
                    "   Move > Position > Target\n" +
                    "   → matches a Move unit which has any port connected to Position, which has any port connected to Target.\n\n" +
                    "   Note: You cannot have nested groups for example Move Connected to (Position OR Angle) this is not supported\n\n" +
                    "• Use '@' tags to filter by port type:\n" +
                    "    - @CI> → Control Input\n" +
                    "    - @CO> → Control Output\n" +
                    "    - @VI> → Value Input\n" +
                    "    - @VO> → Value Output\n" +
                    "    - @I>  → Any Input (ControlInput or ValueInput)\n" +
                    "    - @O>  → Any Output (ControlOutput or ValueOutput)\n" +
                    "   Example: Move @CO> Target\n" +
                    "   → matches a 'Move' unit's Control Output connected to a 'Target' unit match.\n\n" +
                    "   Note: Text after `>` or `|` is searched separately and still follows the selected search mode (Contains or Starts With).\n\n" +
                    "• '*' can be used at either side:\n" +
                    "   * > Debug  → any unit connected to something containing 'Debug'\n" +
                    "   Debug > *  → something containing 'Debug' connected to any unit",
                    MessageType.None
                );

                GUILayout.Space(5);

                GUILayout.Label("Tips", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    $"• Press Alt + F to quickly focus the search field or window if not focused or open.\n" +
                    $"• Hover over the 'Search:' label to see your query displayed in a more readable format.\n" +
                    "• Error nodes are always shown regardless of query.",
                    MessageType.None
                );

                EditorGUILayout.EndScrollView();
            }
        }
    }
}
