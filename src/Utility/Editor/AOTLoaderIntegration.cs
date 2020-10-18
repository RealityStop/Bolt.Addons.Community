using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Bolt.Addons.Community.Utility.Editor
{
    [CustomEditor(typeof(CommunityUnitsAOTLoader), true)]
    class AOTLoaderIntegration : UnityEditor.Editor
    {
        [MenuItem("Tools/Bolt/Add Community AOT Loader")]
        static void AddAOTLoader()
        {
            CommunityUnitsAOTLoader.Construct();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Scans for any loaders in dlls, and adds them to this object so that all dlls will be forced into memory on scene start and kept in memory for the duration of the application.", MessageType.Info);
            if (GUILayout.Button("Rescan for Loaders"))
                (target as CommunityUnitsAOTLoader).FindAndStoreLoaders();
        }
    }
}