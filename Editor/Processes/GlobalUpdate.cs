using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public sealed class GlobalUpdate
    {
        private bool isInitialized;
        public static event Action processes = ()=> { };
        private static List<GlobalProcess> instances = new List<GlobalProcess>();

        public void Bind()
        {
            if (!isInitialized)
            {
                EditorApplication.update += UpdateProcess;
                InitProcesses();
                isInitialized = true;
            }
        }

        public void Unbind()
        {
            if (isInitialized)
            {
                EditorApplication.update -= UpdateProcess;
                ClearProcesses();
                isInitialized = false;
            } 
        }

        public static void Unbind(GlobalProcess process)
        {
            if (!instances.Contains(process))
            {
                process.OnUnbind();
                instances.Remove(process);
            }
        }

        public static void Bind(GlobalProcess process)
        {
            if (instances.Contains(process))
            {
                process.OnBind();
                instances.Add(process);
            }
        }

        private void UpdateProcess()
        {
            processes();
        }

        private void InitProcesses()
        {
            var _processes = typeof(GlobalProcess).Get().Derived();

            for (int i = 0; i < _processes.Length; i++)
            {
                var process = (GlobalProcess)Activator.CreateInstance(_processes[i]);
                instances.Add(process);
                process.OnInitialize();
                processes += process.Process;
                process.OnBind();
            }
        }

        private void ClearProcesses()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                processes -= instances[i].Process;
            }
        }
    }
}
