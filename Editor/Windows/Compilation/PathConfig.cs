using UnityEngine;
using System.IO;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public class PathConfig
    {
        public readonly string BasePath;
        public readonly string BaseRelativePath;
        public readonly string ScriptsPath;
        public readonly string ScriptsRelativePath;
        public readonly string EditorPath;
        public readonly string EditorRelativePath;
        public readonly string ObjectsPath;
        public readonly string DelegatesPath;
        public readonly string DelegatesRelativePath;
        public readonly string EnumsPath;
        public readonly string EnumsRelativePath;
        public readonly string InterfacesPath;
        public readonly string ObjectsRelativePath;
        public readonly string InterfacesRelativePath;

        public PathConfig()
        {
            BasePath = Path.Combine(Application.dataPath, AssetCompiler.GeneratedPath);
            BaseRelativePath = $"Assets/{AssetCompiler.GeneratedPath}";
            ScriptsPath = Path.Combine(BasePath, "Scripts");
            ScriptsRelativePath = Path.Combine(BaseRelativePath, "Scripts");
            EditorPath = Path.Combine(BasePath, "Editor");
            EditorRelativePath = Path.Combine(BaseRelativePath, "Editor");
            ObjectsPath = Path.Combine(ScriptsPath, "Objects");
            ObjectsRelativePath = Path.Combine(ScriptsRelativePath, "Objects");
            DelegatesPath = Path.Combine(ScriptsPath, "Delegates");
            DelegatesRelativePath = Path.Combine(ScriptsRelativePath, "Delegates");
            EnumsPath = Path.Combine(ScriptsPath, "Enums");
            EnumsRelativePath = Path.Combine(ScriptsRelativePath, "Enums");
            InterfacesPath = Path.Combine(ScriptsPath, "Interfaces");
            InterfacesRelativePath = Path.Combine(ScriptsRelativePath, "Interfaces");
        }

        public void EnsureDirectories()
        {
            HUMIO.Ensure(BasePath).Path();
            HUMIO.Ensure(ScriptsPath).Path();
            HUMIO.Ensure(EditorPath).Path();
            HUMIO.Ensure(ObjectsPath).Path();
            HUMIO.Ensure(DelegatesPath).Path();
            HUMIO.Ensure(EnumsPath).Path();
            HUMIO.Ensure(InterfacesPath).Path();
        }

        public void ClearGeneratedFiles()
        {
            if (Directory.Exists(ScriptsPath))
                Directory.Delete(ScriptsPath, true);
            if (Directory.Exists(EditorPath))
                Directory.Delete(EditorPath, true);
            EnsureDirectories();
        }
    }
}
