using UnityEditor;

[InitializeOnLoad]
public class PackageImportHandler : AssetPostprocessor
{
    static PackageImportHandler()
    {
        AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
    }

    private static void OnImportPackageCompleted(string packageName)
    {
        if (packageName == "dev.bolt.addons")
        {
            AssetDatabase.MoveAsset("Packages\\dev.bolt.addons\\Runtime\\Fundamentals\\Nodes\\GeneratedUnits", "Assets/");
        }
    }
}
