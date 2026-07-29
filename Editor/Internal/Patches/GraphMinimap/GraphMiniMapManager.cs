using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [InitializeAfterPlugins]
    public static class GraphMinimapManager
    {
#if ENABLE_GRAPH_MINIMAP
        private static readonly Dictionary<GraphWindow, GraphMinimapInstance> instances =
            new Dictionary<GraphWindow, GraphMinimapInstance>();

        private static double lastUpdateTime;
        private const double UpdateInterval = 1.0 / 30.0;
        static GraphMinimapManager()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastUpdateTime < UpdateInterval)
                return;

            lastUpdateTime = currentTime;

            if (!ProviderPatcher.isWidgetsPatched)
                return;

            if (GraphGUIUtilities.DisableUI)
            {
                DisposeAll();
                return;
            }

            var tabs = GraphWindow.tabsNoAlloc;
            if (tabs == null || tabs.Count == 0)
                return;

            foreach (var window in tabs)
            {
                if (window == null)
                    continue;

                if (!instances.TryGetValue(window, out var instance))
                {
                    if (window.context == null)
                        continue;

                    instance = new GraphMinimapInstance(window);
                    instances.Add(window, instance);
                }

                instance.Tick();
            }

            CleanupClosedWindows();
        }

        private static void CleanupClosedWindows()
        {
            List<GraphWindow> closed = null;

            foreach (var key in instances.Keys)
            {
                if (key == null)
                {
                    closed ??= new List<GraphWindow>();
                    closed.Add(key);
                }
            }

            if (closed != null)
            {
                for (int i = 0; i < closed.Count; i++)
                {
                    var window = closed[i];
                    instances[window]?.Dispose();
                    instances.Remove(window);
                }
            }
        }

        private static void DisposeAll()
        {
            foreach (var instance in instances.Values)
                instance.Dispose();

            instances.Clear();
        }
#endif
    }
}