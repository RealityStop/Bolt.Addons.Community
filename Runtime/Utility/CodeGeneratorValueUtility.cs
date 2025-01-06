using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using Unity.VisualScripting.Community;
using System.IO;

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

    private static bool isLoaded = false;

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
    private const string SAVE_PATH = "Library/CodeGeneratorValues.json";
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
            // Only save if the main object is a scene object
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

        File.WriteAllText(SAVE_PATH, JsonUtility.ToJson(wrapper, true));
    }

    private static void LoadValues()
    {
        if (!File.Exists(SAVE_PATH)) return;

        var json = File.ReadAllText(SAVE_PATH);
        var wrapper = JsonUtility.FromJson<SerializableWrapper>(json);

        foreach (var handler in wrapper.handlers)
        {
            Object target = null;

            // Try load scene object
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

            // Check if target already exists in dictionary
            if (ObjectValueHandlers.ContainsKey(target)) continue;

            var valueDict = new Dictionary<string, Object>();

            // Load asset values
            foreach (var pair in handler.assetValues)
            {
                var valuePath = AssetDatabase.GUIDToAssetPath(pair.value);
                var value = AssetDatabase.LoadAssetAtPath<Object>(valuePath);
                if (value != null && !valueDict.ContainsValue(value))
                {
                    valueDict.Add(pair.key, value);
                }
            }

            // Load scene object values
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
#endif
    private static Object EnsureCurrentAsset()
    {
#if UNITY_EDITOR
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
            else if (Selection.activeObject is CodeAsset or ScriptGraphAsset)
            {
                currentAsset = Selection.activeObject;
            }
        }
        return currentAsset;
#else
        return null;
#endif
    }
    /// <summary>
    /// Used to communicate with the CodeGenerator to get the current scriptmachine from the target object.
    /// </summary>
    public static System.Func<GameObject, ScriptMachine> requestMachine;
    public static Object currentAsset;
    public static void AddValue(string variableName, Object variableValue)
    {
        EnsureLoaded();

        var target = EnsureCurrentAsset();
        if (target == null)
        {
            return;
        }

        // Check if this value already exists with any variable name
        if (TryGetVariable(variableValue, out string existingVariable))
        {
            return; // Skip if we already have this value stored
        }

        if (!ObjectValueHandlers.ContainsKey(target))
        {
            ObjectValueHandlers.Add(target, new Dictionary<string, Object>() { { variableName, variableValue } });
        }
        else if (!ObjectValueHandlers[target].ContainsKey(variableName) &&
                 !ObjectValueHandlers[target].ContainsValue(variableValue))
        {
            ObjectValueHandlers[target].Add(variableName, variableValue);
        }
        else
            return;

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

        // Only look for variables in the current ScriptMachine's context
        if (target is ScriptMachine scriptMachine)
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
    public static Dictionary<string, Object> GetAllValues(Object target)
    {
        EnsureCurrentAsset();
        EnsureLoaded();
        if (ObjectValueHandlers.ContainsKey(target))
            return ObjectValueHandlers[target];
        else return new Dictionary<string, Object>();
    }
}