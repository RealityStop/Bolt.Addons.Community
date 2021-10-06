using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using SFile = System.IO.File;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMIO_Children
    {
        /// <summary>
        /// When saving, also do an accompanied action.
        /// </summary>
        public static HUMIO.Data.And And(this HUMIO.Data.Save saveData)
        {
            return new HUMIO.Data.And();
        }

        public static string EndSlash(this HUMIO.Data.Remove remove)
        {
            var lastIndexString = remove.path[remove.path.Length - 1].ToString();

            return (lastIndexString == "/" || lastIndexString == @"\") ? remove.path.Remove(remove.path.Length - 1, 1) : remove.path;
        }

        /// <summary>
        /// Sets the save to be in the persistant data path.
        /// </summary>
        public static HUMIO.Data.Persistant Persistant(this HUMIO.Data.Save saveData, string fileName)
        {
            var path = HUMIO.PersistantPath();

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return new HUMIO.Data.Persistant(path, saveData, new HUMIO.Data.Load());
        }

        /// <summary>
        /// Sets the save to be in a custom folder.
        /// </summary>
        public static HUMIO.Data.Custom Custom(this HUMIO.Data.Save saveData, string filePath, string fileName)
        {
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

            return new HUMIO.Data.Custom(filePath + fileName, saveData, new HUMIO.Data.Load());
        }

        /// <summary>
        /// Sets the save to be in a custom folder.
        /// </summary>
        public static HUMIO.Data.Custom Custom(this HUMIO.Data.Save saveData, string fullPath)
        {
            return new HUMIO.Data.Custom(fullPath, saveData, new HUMIO.Data.Load());
        }


        /// <summary>
        /// Deletes a file in the persistant data path.
        /// </summary>
        public static void Persistant(this HUMIO.Data.Delete deleteData, string fileName)
        {
            var path = HUMIO.PersistantPath(fileName);
            if (SFile.Exists(path))
            {
                SFile.Delete(path);
            }
        }

        /// <summary>
        /// Deletes a file in a custom folder.
        /// </summary>
        public static void Custom(this HUMIO.Data.Delete deleteData, string filePath)
        {
            if (SFile.Exists(filePath))
            {
                SFile.Delete(filePath);
            }
        }

        /// <summary>
        /// Copies a file from the persistant data path, into the persistant data path, with a new file name;
        /// </summary>
        public static void Persistant(this HUMIO.Data.Copy copyData, string fileName, string newFileName)
        {
            var path = HUMIO.PersistantPath(fileName);
            var newPath = HUMIO.PersistantPath(newFileName);

            if (SFile.Exists(path))
            {
                SFile.Copy(path, newPath);
            }
        }

        /// <summary>
        /// Copies a file from a custom folder, into another custom folder.
        /// </summary>
        public static void Custom(this HUMIO.Data.Copy copyData, string filePath, string newFilePath)
        {
            if (SFile.Exists(filePath))
            {
                SFile.Copy(filePath, newFilePath);
            }
        }

        /// <summary>
        /// Saves a text file to the persistant data path.
        /// </summary>
        public static void Text(this HUMIO.Data.Persistant persistantData)
        {
            using (var fileStream = new FileStream(persistantData.path, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(fileStream);
                writer.Write(persistantData.save.value.ToString());
            }
        }

        /// <summary>
        /// Saves a text file to a custom folder. If we are in the editor, the editor will attempt a save and refresh.
        /// </summary>
        public static void Text(this HUMIO.Data.Custom customData, bool refresh = true)
        {
            using (var fileStream = new FileStream(customData.path, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(fileStream);
                writer.Write(customData.save.value.ToString());
                writer.Close();
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
            if (refresh) UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Ensures this path will exist if it does not already.
        /// </summary>
        public static void Path(this HUMIO.Data.Ensure ensure)
        {
            var legalPath = !ensure.path.Contains(".") ? ensure.path : ensure.path.Remove(ensure.path.LastIndexOf("/") + 1, (ensure.path.Length - 1) - ensure.path.LastIndexOf("/"));

            if (!Directory.Exists(legalPath))
            {
                Directory.CreateDirectory(legalPath);
            }
        }

        /// <summary>
        /// Ensures this path will exist if it does not already.
        /// </summary>
        public static void File(this HUMIO.Data.Ensure ensure, Action create)
        {
            var legalPath = !ensure.path.Contains(".") ? ensure.path : ensure.path.Remove(ensure.path.LastIndexOf("/") + 1, (ensure.path.Length - 1) - ensure.path.LastIndexOf("/"));

            if (!SFile.Exists(legalPath))
            {
                create?.Invoke();
            }
        }

        /// <summary>
        /// Begins the operation of deleting a file or folder.
        /// </summary>
        public static HUMIO.Data.Delete Delete()
        {
            return new HUMIO.Data.Delete();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Saves an asset at path. Optionally you can immediately refresh the database after saving.
        /// </summary>
        public static void Asset(this HUMIO.Data.Save saveData, string path, bool refresh = false)
        {
            if (saveData.value.GetType() == typeof(GameObject))
            {
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(saveData.value as GameObject, path);
            }
            else
            {
                UnityEditor.AssetDatabase.CreateAsset(saveData.value as UnityEngine.Object, path);
            }

            if (refresh) saveData.And().Refresh();
        }

        /// <summary>
        /// Saves a new asset instance at path if it doesn't already exist. Optionally you can immediately refresh the database after saving.
        /// </summary>
        public static T Asset<T>(this HUMIO.Data.Ensure ensure, string filename, bool ensurePath = false, bool refresh = false)
            where T : ScriptableObject
        {
            if (ensurePath) ensure.Path();
            var asset = AssetDatabase.LoadAssetAtPath<T>(ensure.path + filename);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                asset.Save().Asset(ensure.path.Remove().EndSlash() + "/" + filename, refresh);
            }
            return asset;
        }


        /// <summary>
        /// Saves and refreshes the asset database.
        /// </summary>
        public static void Refresh(this HUMIO.Data.And andData)
        {
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// Saves the file, and then saves and refreshes the asset database.
        /// </summary>
        public static void Refresh()
        {
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
    }
}
