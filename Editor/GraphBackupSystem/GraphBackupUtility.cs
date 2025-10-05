using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class GraphBackupUtility
    {
        private static fsSerializer serializer = new fsSerializer();

        public static void SaveBackup(UnityEngine.Object graphObject)
        {
            if (graphObject == null) return;

            var result = serializer.TrySerialize(graphObject.GetType(), graphObject, out fsData data);
            if (!result.Succeeded)
            {
                Debug.LogWarning("Graph backup serialization failed: " + result.FormattedMessages);
                return;
            }

            string json = fsJsonPrinter.CompressedJson(data);

            var path = AssetDatabase.FindAssets($"t:{typeof(BackupHolder).Name}").FirstOrDefault();
            BackupHolder holder;
            if (string.IsNullOrEmpty(path))
            {
                string folderPath = $"Assets/{AssetCompiler.GeneratedPath}/Editor";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                AssetDatabase.Refresh();
                string assetPath = folderPath + "/GraphBackups.asset";
                holder = ScriptableObject.CreateInstance<BackupHolder>();
                AssetDatabase.CreateAsset(holder, assetPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                path = AssetDatabase.GUIDToAssetPath(path);
                holder = AssetDatabase.LoadAssetAtPath<BackupHolder>(path);
            }

            string id = GetGraphID(graphObject);

            var list = holder.backups.FirstOrDefault(b => b.id == id);
            if (list == null)
            {
                list = new GraphBackupList { id = id };
                holder.backups.Add(list);
            }

            list.entries.Add(new GraphBackup
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                json = json,
                id = id,
                name = graphObject.name,
                scenePath = GetScenePath(graphObject)
            });

            if (list.entries.Count > 5)
                list.entries.RemoveAt(0);

            EditorUtility.SetDirty(holder);
            AssetDatabase.SaveAssets();
        }

        public static string GetGraphID(UnityEngine.Object obj)
        {
            if (obj == null) return string.Empty;

            if (AssetDatabase.Contains(obj))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                return AssetDatabase.AssetPathToGUID(path);
            }

            if (obj is IEventMachine em && em.nest.embed != null)
            {
                string prefabGuid = GetPrefabGUID((em as LudiqBehaviour).gameObject);
                if (!string.IsNullOrEmpty(prefabGuid))
                {
                    return prefabGuid + "_" + (em as LudiqBehaviour).GetInstanceID();
                }
                else
                {
                    string path = (em as LudiqBehaviour).gameObject.scene.path;
                    return path + "_" + (em as LudiqBehaviour).GetInstanceID();
                }
            }

            return obj.name + "_" + obj.GetInstanceID();
        }

        public static string GetPrefabGUID(GameObject go)
        {
            if (go == null) return string.Empty;

            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (prefab != null)
            {
                string path = AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(path))
                    return AssetDatabase.AssetPathToGUID(path);
            }

            return string.Empty;
        }

        public static string GetScenePath(UnityEngine.Object obj)
        {
            if (obj is Component comp)
            {
                if (comp.gameObject.scene.IsValid())
                    return comp.gameObject.scene.path;
            }
            return string.Empty;
        }

        public static void RestoreBackup(UnityEngine.Object graphObject, int backupIndex)
        {
            if (graphObject == null) return;

            var path = AssetDatabase.FindAssets($"t:{typeof(BackupHolder).Name}").FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return;

            path = AssetDatabase.GUIDToAssetPath(path);
            var holder = AssetDatabase.LoadAssetAtPath<BackupHolder>(path);
            if (holder == null || holder.backups == null) return;

            string id = GetGraphID(graphObject);

            var list = holder.backups.FirstOrDefault(b => b.id == id);
            if (list == null || list.entries.Count == 0) return;

            if (backupIndex < 0) backupIndex = 0;
            if (backupIndex >= list.entries.Count) backupIndex = list.entries.Count - 1;

            GraphBackup backup = list.entries[backupIndex];
            if (string.IsNullOrEmpty(backup.json)) return;

            var data = fsJsonParser.Parse(backup.json);
            object restored = null;

            var result = serializer.TryDeserialize(data, graphObject.GetType(), ref restored);
            if (!result.Succeeded)
            {
                Debug.LogWarning("Graph backup restore failed: " + result.FormattedMessages);
                return;
            }

            if (graphObject is ScriptGraphAsset || graphObject is StateGraphAsset || graphObject is MacroScriptableObject)
            {
                EditorUtility.CopySerialized(restored as UnityEngine.Object, graphObject);
            }
            else if (graphObject is ScriptMachine sm && sm.nest.source == GraphSource.Embed && sm.nest.embed != null)
            {
                EditorUtility.CopySerialized(sm, restored as ScriptMachine);
            }
            else if (graphObject is StateMachine stm && stm.nest.source == GraphSource.Embed && stm.nest.embed != null)
            {
                EditorUtility.CopySerialized(stm, restored as StateMachine);
            }

            EditorUtility.SetDirty(graphObject);
            AssetDatabase.SaveAssets();
        }
    }
}