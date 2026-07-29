using System;
using System.Linq;
using UnityEngine.Assemblies;

namespace Unity.VisualScripting.Community.Utility
{
    [RenamedFrom("Bolt.Addons.Community.Utility.EditorState")]
    public static class EditorState
    {
        public static IEditorStateFetcher Fetcher { get; set; }

        static object lockObject = new object();

        static EditorState()
        {
            lock (lockObject)
            {
                var type = typeof(IEditorStateFetcher);
#if UNITY_6000_4_OR_NEWER
                var types = CurrentAssemblies.GetLoadedAssemblies()
#else
                var types = AppDomain.CurrentDomain.GetAssemblies()
#endif
                    .SelectMany(s => s.GetTypes())
                    .Where(p =>
                    {
                        if (type.IsAssignableFrom(p))
                        {
                            return p.Assembly != type.Assembly;
                        }
                        return false;
                    }).ToList();

                if (types.Count() == 1)
                {
                    Fetcher = (IEditorStateFetcher)Activator.CreateInstance(types.First());
                    return;
                }
            }
        }
        
        public static bool IsEditorPaused()
        {
            if (Fetcher != null)
                return Fetcher.IsEditorPaused();
            return false;
        }
    }
}