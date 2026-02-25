using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    internal static class VariableInspectorState
    {
        public static void Save(UnityEngine.Object rootObject, Guid[] parentGuids, string variableName, bool value)
        {
            if (rootObject == null)
                return;

            var key = BuildInspectorKey(rootObject, parentGuids);

            SessionState.SetBool($"{key}.{variableName}", value);
        }

        public static bool Load(UnityEngine.Object rootObject, Guid[] parentGuids, string variableName)
        {
            if (rootObject == null)
                return false;

            var key = BuildInspectorKey(rootObject, parentGuids);

            bool state = SessionState.GetBool($"{key}.{variableName}", false);
            return state;
        }

        private static string BuildInspectorKey(UnityEngine.Object rootObject, Guid[] parentGuids)
        {
            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(rootObject);

            var builder = new StringBuilder();
            builder.Append(globalId.ToString());

            if (parentGuids != null)
            {
                foreach (var guid in parentGuids)
                {
                    builder.Append("_");
                    builder.Append(guid.ToString());
                }
            }

            return "Community.Session." + builder.ToString();
        }
    }
}