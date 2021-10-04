using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.CommunityOptionFetcher")]
    public static class CommunityOptionFetcher
    {
        static object lockObject = new object();
        static CommunityOptionFetcher()
        {
            lock (lockObject)
            {
                var type = typeof(CommunityOptions);
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

                if (types.Count() > 1)
                {
                    Debug.LogError("Multiple Community Options scripts found.");
                    return;
                }

                if (types.Count() == 1)
                {
                    
                    CommunityOptions options = (CommunityOptions)Activator.CreateInstance(types.First());
                    DefinedEvent_ForceOptimizedInEditor = options.DefinedEvent_ForceOptimizedInEditor;
                    DefinedEvent_RestrictEventTypes = options.DefinedEvent_RestrictEventTypes;
                    SilenceLogMessages = options.SilenceLogMessages;

                    if (!SilenceLogMessages)
                    {
                        Debug.Log("Custom Community Options script found.");
                        LogOptionsSelected();
                    }

                    return;
                }
                Debug.Log("No Community Options script found; using defaults");
            }
        }

        private static void LogOptionsSelected()
        {
            Debug.Log($"Defined Events optimized in editor: {FormatBool(DefinedEvent_ForceOptimizedInEditor)}");
            Debug.Log($"Defined Events restrict type to interface implementors: {FormatBool(DefinedEvent_RestrictEventTypes)}");
        }

        private static object FormatBool(bool input)
        {
            return input ? "True" : "False";
        }



        public static bool DefinedEvent_ForceOptimizedInEditor { get; } = false;
        public static bool DefinedEvent_RestrictEventTypes { get; } = true;
        public static bool SilenceLogMessages { get; } = false;
    }
}
