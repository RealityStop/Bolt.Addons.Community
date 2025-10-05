using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public class GraphBackup
    {
        public string timestamp;
        public string json;
        public string id;
        public string scenePath;
        public string name;
    }

    public class BackupHolder : ScriptableObject
    {
        public List<GraphBackupList> backups = new List<GraphBackupList>();
    }

    [Serializable]
    public class GraphBackupList
    {
        public string id;
        public List<GraphBackup> entries = new List<GraphBackup>();
    }
}