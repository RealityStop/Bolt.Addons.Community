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
    
        static GraphMinimapManager()
        {
            GraphMiniMapStorage.Load();
            EditorApplication.update += Update;
        }
    
        private static void Update()
        {
            if (!ProviderPatcher.isWidgetsPatched)
                return;
    
            if (GraphGUIUtilities.DisableUI)
            {
                DisposeAll();
                return;
            }
    
            var tabs = GraphWindow.tabs;
            if (tabs == null)
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
            GraphMiniMapStorage.SaveIfNeeded();
        }
    
        private static void CleanupClosedWindows()
        {
            var closed = instances.Keys.Where(w => w == null).ToList();
            foreach (var window in closed)
            {
                instances[window].Dispose();
                instances.Remove(window);
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