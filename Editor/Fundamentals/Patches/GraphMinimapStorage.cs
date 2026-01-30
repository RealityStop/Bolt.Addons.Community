using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public class GraphMiniMapSettings
    {
        public float width = 200f;
        public float height = 150f;
        public Dictionary<string, bool> minimized = new Dictionary<string, bool>();
    }

    public static class GraphMiniMapStorage
    {
        private static readonly string SettingsPath = "ProjectSettings/GraphMiniMapSettings.json";
        private static GraphMiniMapSettings _settings;
        private static bool _isDirty;

        public static GraphMiniMapSettings Settings
        {
            get
            {
                if (_settings == null)
                    Load();
                return _settings;
            }
        }

        public static void Load()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
                    _settings = (GraphMiniMapSettings)new SerializationData(json).Deserialize() ?? new GraphMiniMapSettings();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[GraphMiniMap] Failed to load settings; recreating defaults.\n{ex.Message}");
                    _settings = new GraphMiniMapSettings();
                }
            }
            else
            {
                _settings = new GraphMiniMapSettings();
            }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                string json = _settings.Serialize().json;
                File.WriteAllText(SettingsPath, json);
                _isDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GraphMiniMap] Failed to save settings: {e}");
            }
        }

        public static void MarkDirty()
        {
            _isDirty = true;
        }

        public static void SaveIfNeeded()
        {
            if (_isDirty)
                Save();
        }
    }
}