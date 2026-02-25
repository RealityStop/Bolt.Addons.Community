using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using Unity.VisualScripting.Community;
using System.IO;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Unity.VisualScripting.Community
{
#if UNITY_EDITOR
    using UnityEditor;
    [InitializeOnLoad]
#endif
    public static class CodeGeneratorValueUtility
    {
        static CodeGeneratorValueUtility()
        {
#if UNITY_EDITOR
            LoadValues();
            isLoaded = true;
            Selection.selectionChanged += () =>
            {
                EnsureCurrentAsset();
            };
#endif
        }
#if UNITY_EDITOR
        private static bool isLoaded = false;
#endif
        private static HashSet<string> currentlyGeneratedVariables = new HashSet<string>();

        private static void EnsureLoaded()
        {
#if UNITY_EDITOR
            if (!isLoaded)
            {
                LoadValues();
                isLoaded = true;
            }
#endif
        }

        private static Dictionary<Object, Dictionary<string, Object>> ObjectValueHandlers = new Dictionary<Object, Dictionary<string, Object>>();

        private const string SAVE_PATH = "Assets/Unity.VisualScripting.Community.Generated/CodeGeneratorValues.json";
#if UNITY_EDITOR
        [System.Serializable]
        private class SerializableKeyValuePair
        {
            public string key;
            public string value;

            public SerializableKeyValuePair(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }

        [System.Serializable]
        private class SerializableValueHandler
        {
            public string targetGuid;
            public int targetInstanceID;
            public string scenePath;
            public List<SerializableKeyValuePair> assetValues = new List<SerializableKeyValuePair>();
            public List<SerializableKeyValuePair> sceneObjectValues = new List<SerializableKeyValuePair>();
        }

        [System.Serializable]
        private class SerializableWrapper
        {
            public List<SerializableValueHandler> handlers = new List<SerializableValueHandler>();
        }

        private static bool IsSceneObject(Object obj)
        {
            return obj != null && !EditorUtility.IsPersistent(obj);
        }

        private static string GetScenePath(Object obj)
        {
            if (obj is GameObject go)
            {
                return go.GetInstanceID().ToString();
            }
            else if (obj is Component comp)
            {
                // Add component type to make path unique for different components
                return comp.gameObject.GetInstanceID().ToString() + "," + comp.GetInstanceID().ToString() + "," + comp.GetType().FullName;
            }
            return null;
        }

        private static void SaveValues()
        {
            var wrapper = new SerializableWrapper();

            foreach (var kvp in ObjectValueHandlers)
            {
                if (!IsSceneObject(kvp.Key))
                    continue;

                var handler = new SerializableValueHandler();
                handler.targetInstanceID = kvp.Key.GetInstanceID();
                handler.scenePath = GetScenePath(kvp.Key);

                foreach (var valueKvp in kvp.Value)
                {
                    if (IsSceneObject(valueKvp.Value))
                    {
                        handler.sceneObjectValues.Add(new SerializableKeyValuePair(
                            valueKvp.Key,
                            GetScenePath(valueKvp.Value)
                        ));
                    }
                    else
                    {
                        handler.assetValues.Add(new SerializableKeyValuePair(
                            valueKvp.Key,
                            AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(valueKvp.Value))
                        ));
                    }
                }

                if (handler.assetValues.Count > 0 || handler.sceneObjectValues.Count > 0)
                {
                    wrapper.handlers.Add(handler);
                }
            }

            var json = JsonUtility.ToJson(wrapper, true);
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using var fs = new FileStream(SAVE_PATH, FileMode.Create, FileAccess.Write, FileShare.None);
                    using var writer = new StreamWriter(fs);
                    writer.Write(json);
                    break;
                }
                catch (IOException ex)
                {
                    Debug.LogWarning($"Failed to write file: {ex.Message}.");
                }
            }
        }

        private static void LoadValues()
        {
            try
            {
                if (!File.Exists(SAVE_PATH)) return;
                var json = File.ReadAllText(SAVE_PATH);
                var wrapper = JsonUtility.FromJson<SerializableWrapper>(json);

                foreach (var handler in wrapper.handlers)
                {
                    Object target = null;

                    if (!string.IsNullOrEmpty(handler.scenePath))
                    {
                        if (handler.scenePath.Contains(','))
                        {
                            var split = handler.scenePath.Split(',');
                            var instanceID = int.Parse(split[0]);
                            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                            if (go != null)
                            {
                                target = go.GetComponents<Component>()
                                    .FirstOrDefault(c => c != null && c.GetInstanceID() == int.Parse(split[1]));
                            }
                        }
                        else
                        {
                            var instanceID = int.Parse(handler.scenePath);
                            target = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                        }
                    }

                    if (target == null) continue;

                    if (ObjectValueHandlers.ContainsKey(target)) continue;

                    var valueDict = new Dictionary<string, Object>();

                    foreach (var pair in handler.assetValues)
                    {
                        var valuePath = AssetDatabase.GUIDToAssetPath(pair.value);
                        var value = AssetDatabase.LoadAssetAtPath<Object>(valuePath);
                        if (value != null && !valueDict.ContainsValue(value))
                        {
                            valueDict.Add(pair.key, value);
                        }
                    }

                    foreach (var pair in handler.sceneObjectValues)
                    {
                        Object sceneValue = null;
                        if (pair.value.Contains(','))
                        {
                            var split = pair.value.Split(',');
                            var instanceID = int.Parse(split[0]);
                            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                            if (go != null)
                            {
                                sceneValue = go.GetComponents<Component>()
                                    .FirstOrDefault(c => c.GetInstanceID() == int.Parse(split[1]));
                            }
                        }
                        else
                        {
                            var instanceID = int.Parse(pair.value);
                            sceneValue = EditorUtility.InstanceIDToObject(instanceID);
                        }

                        if (sceneValue != null && !valueDict.ContainsValue(sceneValue))
                        {
                            valueDict.Add(pair.key, sceneValue);
                        }
                    }

                    if (valueDict.Count > 0)
                        ObjectValueHandlers.Add(target, valueDict);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load serialized data: {ex.Message}");
            }
        }
#endif
        private static Object EnsureCurrentAsset(Object shouldBe = null)
        {
#if UNITY_EDITOR
            if (shouldBe != null)
            {
                if (currentAsset != null && currentAsset == shouldBe) return currentAsset;
            }
            else if (currentAsset != null) return currentAsset;
            if (Selection.activeGameObject != null)
            {
                currentAsset = requestMachine?.Invoke(Selection.activeGameObject);
            }
            else if (Selection.activeObject != null)
            {
                if (Selection.activeObject is GameObject gameObject)
                {
                    currentAsset = requestMachine?.Invoke(gameObject);
                }
                else if (Selection.activeObject is CodeAsset || Selection.activeObject is ScriptGraphAsset)
                {
                    currentAsset = Selection.activeObject;
                }
            }

            if (shouldBe != null && currentAsset != shouldBe) currentAsset = shouldBe;

            return currentAsset;
#else
            return null;
#endif
        }
        /// <summary>
        /// Used to communicate with the CodeGenerator to get the current scriptmachine from the target object.
        /// </summary>
        public static System.Func<GameObject, SMachine> requestMachine;
        public static Object currentAsset;
        public static void SetIsUsed(string variableName)
        {
            currentlyGeneratedVariables.Add(variableName);
        }
        public static void AddValue(string variableName, Object variableValue)
        {
            EnsureLoaded();
            var target = EnsureCurrentAsset();
            if (target == null) return;

            currentlyGeneratedVariables.Add(variableName);

            if (TryGetVariable(variableValue, out string existingVariable)) return;

            if (!ObjectValueHandlers.ContainsKey(target))
            {
                ObjectValueHandlers[target] = new Dictionary<string, Object>();
            }

            if (!ObjectValueHandlers[target].ContainsKey(variableName))
            {
                ObjectValueHandlers[target][variableName] = variableValue;
            }
#if UNITY_EDITOR
            SaveValues();
#endif
        }

        public static bool TryGetVariable(Object value, out string variableName)
        {
            var target = EnsureCurrentAsset();
            EnsureLoaded();
            if (target == null || value == null)
            {
                variableName = "";
                return false;
            }

            if (target is SMachine scriptMachine)
            {
                if (ObjectValueHandlers.TryGetValue(scriptMachine, out var values) &&
                    values.ContainsValue(value))
                {
                    variableName = values.First(kvp => kvp.Value == value).Key;
                    return true;
                }
            }
            else if (ObjectValueHandlers.ContainsKey(target) &&
                     ObjectValueHandlers[target].ContainsValue(value))
            {
                variableName = ObjectValueHandlers[target].First(kvp => kvp.Value == value).Key;
                return true;
            }

            variableName = "";
            return false;
        }
        public static Dictionary<string, Object> GetAllValues(Object target, bool clearObsolete = true)
        {
            EnsureCurrentAsset(target);
            EnsureLoaded();
            if (clearObsolete)
                RemoveObsoleteValues(target);
            if (ObjectValueHandlers.ContainsKey(target))
                return ObjectValueHandlers[target];
            else return new Dictionary<string, Object>();
        }
        public static void RemoveObsoleteValues(Object target)
        {
            EnsureLoaded();

            if (!ObjectValueHandlers.ContainsKey(target))
            {
                return;
            }

            var obsoleteVariables = ObjectValueHandlers[target]
                .Keys.Where(v => !currentlyGeneratedVariables.Contains(v))
                .ToList();

            foreach (var variableName in obsoleteVariables)
            {
                ObjectValueHandlers[target].Remove(variableName);
            }

            if (ObjectValueHandlers[target].Count == 0)
            {
                ObjectValueHandlers.Remove(target);
            }

            currentlyGeneratedVariables.Clear();

#if UNITY_EDITOR
            SaveValues();
#endif
        }
    }
}