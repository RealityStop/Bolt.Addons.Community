using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community 
{
    [InitializeOnLoad]
    public static class GraphBackupSceneProcessor
    {
        static GraphBackupSceneProcessor()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }
    
        private static void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var machine in root.GetComponentsInChildren<IEventMachine>(true))
                {
                    if (machine.nest.source == GraphSource.Embed && machine.nest.embed != null)
                    {
                        GraphBackupUtility.SaveBackup(machine as LudiqBehaviour);
                    }
                }
            }
        }
    } 
}