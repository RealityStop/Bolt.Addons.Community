using System;
using System.Linq;

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
                var types = AppDomain.CurrentDomain.GetAssemblies()
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