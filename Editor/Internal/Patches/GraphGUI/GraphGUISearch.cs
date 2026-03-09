using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    internal static class GraphGUISearch
    {
        public static VisualElement Create(GraphWindow window)
        {
            var state = GraphGUIState.Get(window).SearchState;

            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            var searchField = new ToolbarSearchField
            {
                value = state.text ?? "",
                style =
                {
                    height = 15,
                    width = 180
                }
            };

            ToolbarButton prevButton = null;
            ToolbarButton nextButton = null;
            var counterLabel = new Label
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    width = 50,
                    height = 20
                }
            };

            void UpdateCounter()
            {
                prevButton.SetEnabled(state.matches.Count > 0);
                nextButton.SetEnabled(state.matches.Count > 0);
                counterLabel.text = state.matches.Count == 0 ? "0/0" : $"{state.currentIndex + 1}/{state.matches.Count}";
            }

            state.counterLabel = counterLabel;

            searchField.RegisterValueChangedCallback(ev =>
            {
                state.text = ev.newValue;
                state.matches.Clear();
                state.currentIndex = -1;
                state.highlightTimers.Clear();
                Search(window, state);
                UpdateCounter();
            });

            container.Add(searchField);

            prevButton = CreateNavigationButton("Previous", 65, 20, () =>
            {
                if (state.matches.Count == 0) return;
                state.currentIndex--;
                if (state.currentIndex < 0) state.currentIndex = state.matches.Count - 1;
                HighlightCurrentMatch(window, state);
                UpdateCounter();
            });

            nextButton = CreateNavigationButton("Next", 45, 20, () =>
            {
                if (state.matches.Count == 0) return;
                state.currentIndex++;
                if (state.currentIndex >= state.matches.Count) state.currentIndex = 0;
                HighlightCurrentMatch(window, state);
                UpdateCounter();
            });

            container.Add(prevButton);
            container.Add(nextButton);
            container.Add(counterLabel);

            UpdateCounter();

            return container;
        }

        private static void Search(GraphWindow window, SearchState state, bool tween = true)
        {
            if (!string.IsNullOrEmpty(state.text))
            {
                foreach (var element in window.reference.graph.elements)
                {
                    if (element is IUnitConnection) continue;

                    if (SearchUtility.SearchMatches(state.text, SearchUtility.GetSearchName(element),
                        NodeFinderWindow.SearchMode.Contains, out _, null))
                    {
                        state.matches.Add(element);
                    }
                }

                if (state.matches.Count > 0 && tween)
                {
                    state.currentIndex = 0;
                    HighlightCurrentMatch(window, state);
                }
            }
        }

        private static void HighlightCurrentMatch(GraphWindow window, SearchState state)
        {
            if (state.currentIndex < 0 || state.currentIndex >= state.matches.Count) return;

            var element = state.matches[state.currentIndex];

            GraphUtility.OverrideContextIfNeeded(() =>
            {
                if (window.reference.graph.elements.Contains(element))
                    window.context.canvas.ViewElements(element.Yield());
            });

            state.highlightTimers[element] = (float)EditorApplication.timeSinceStartup + 1f;
        }

        private static ToolbarButton CreateNavigationButton(string text, float width, float height, Action callback)
        {
            var btn = new ToolbarButton(callback)
            {
                text = text,
                focusable = false,
                style =
                {
                    width = width,
                    height = height,
                    fontSize = 11,
                    backgroundColor = Color.clear,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            btn.RegisterCallback<MouseEnterEvent>(evt => btn.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f));
            btn.RegisterCallback<MouseLeaveEvent>(evt => btn.style.backgroundColor = Color.clear);
            return btn;
        }
    }
}