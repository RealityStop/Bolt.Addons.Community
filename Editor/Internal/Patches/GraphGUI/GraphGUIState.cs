using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    internal static class GraphGUIState
    {
        public static WindowState Get(GraphWindow window)
        {
            if (!(window.rootVisualElement.userData is WindowState state))
            {
                state = new WindowState();
                window.rootVisualElement.userData = state;
            }

            return state;
        }

        public static void Remove(GraphWindow window)
        {
            if (!(window.rootVisualElement.userData is WindowState state))
                return;

            state.Toolbar?.RemoveFromHierarchy();
            state.FloatingToolbar?.RemoveFromHierarchy();

            window.rootVisualElement.userData = null;
        }
    }

    internal class WindowState
    {
        public SearchState SearchState = new SearchState();
        public VisualElement Toolbar;
        public VisualElement FloatingToolbar;
        public IGraphContext Context;
        public System.Action contextChanged;

        public void SetContext(IGraphContext context)
        {
            if (context == Context) return;
            Context = context;
            contextChanged?.Invoke();
        }
    }

    internal class SearchState
    {
        public string text;
        public System.Collections.Generic.List<IGraphElement> matches = new System.Collections.Generic.List<IGraphElement>();
        public int currentIndex = -1;
        public System.Collections.Generic.Dictionary<IGraphElement, float> highlightTimers = new System.Collections.Generic.Dictionary<IGraphElement, float>();
        public GraphReference reference;
        public Label counterLabel;
    }
}