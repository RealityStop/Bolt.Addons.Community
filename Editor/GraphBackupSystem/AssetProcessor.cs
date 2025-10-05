// using UnityEditor;
// using UnityEngine;

// namespace Unity.VisualScripting.Community
// {
//     public class GraphBackupProcessor : AssetModificationProcessor
//     {
//         static string[] OnWillSaveAssets(string[] paths)
//         {
//             foreach (var path in paths)
//             {
//                 var type = AssetDatabase.GetMainAssetTypeAtPath(path);
//                 if (typeof(MacroScriptableObject).IsAssignableFrom(type))
//                 {
//                     var graph = AssetDatabase.LoadAssetAtPath<MacroScriptableObject>(path);
//                     if (graph != null)
//                     {
//                         GraphBackupUtility.SaveBackup(graph);
//                     }
//                 }
//                 else if (type == typeof(GameObject))
//                 {
//                     var @object = AssetDatabase.LoadAssetAtPath<GameObject>(path);

//                     foreach (var machine in @object.GetComponentsInChildren<IEventMachine>(true))
//                     {
//                         if (machine.nest.source == GraphSource.Embed && machine.nest.embed != null)
//                         {
//                             GraphBackupUtility.SaveBackup(machine as LudiqBehaviour);
//                         }
//                     }
//                 }
//             }
//             return paths;
//         }
//     }
// }
