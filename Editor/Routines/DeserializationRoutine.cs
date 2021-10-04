using System;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public static class DeserializationRoutine
    {
        private static int ticks;
        private static bool isInitializing;
        private static GlobalUpdate update = new GlobalUpdate();

        [InitializeOnLoadMethod]
        private static void StartInitializing()
        {
            EditorApplication.delayCall += DelayInitialize;
        }

        private static void DelayInitialize()
        {
            isInitializing = true;
            update.Bind();
            RunRoutines();
        }

        public static void Disable()
        {
            if (isInitializing)
            {
                EditorApplication.update -= DelayInitialize;
                update.Unbind();
            }
        }

        private static void RunRoutines()
        {
            var routines = typeof(DeserializedRoutine).Get().Derived();

            for (int i = 0; i < routines.Length; i++)
            {
                var routine = (DeserializedRoutine)Activator.CreateInstance(routines[i]);
                routine.Run();
            }
        }
    }
}
