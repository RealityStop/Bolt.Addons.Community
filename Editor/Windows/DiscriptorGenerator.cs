using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class CustomScriptGeneratorWindow : EditorWindow
{
    private MonoScript unitScript;
    private bool includeDescription = false;
    private string scriptDescription = "";
    private bool iconEnabled = false;
    private Texture2D customIcon = null;
    private string iconPath = "";

    [MenuItem("Window/Community Addons/Unit Descriptor Generator")]
    public static void ShowWindow()
    {
        GetWindow<CustomScriptGeneratorWindow>("Node Descriptor Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Custom Node Descriptor Generator", EditorStyles.boldLabel);

        GUILayout.Space(10);

        unitScript = EditorGUILayout.ObjectField("Unit Script", unitScript, typeof(MonoScript), false) as MonoScript;

        if (includeDescription)
        {
            scriptDescription = EditorGUILayout.TextField("Unit Description", scriptDescription);
        }

        includeDescription = EditorGUILayout.Toggle("Include Description", includeDescription);


        iconEnabled = EditorGUILayout.Toggle("Use Custom Icon", iconEnabled);

        if (iconEnabled)
        {
            customIcon = EditorGUILayout.ObjectField("Custom Icon", customIcon, typeof(Texture2D), false) as Texture2D;
            iconPath = AssetDatabase.GetAssetPath(customIcon);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Generate"))
        {
            GenerateDescriptor();
        }
    }

    private void GenerateDescriptor()
    {
        if (unitScript == null)
        {
            Debug.Log("Please select a unit script.");
            return;
        }

        string defaultPath = EditorUtility.SaveFilePanelInProject("Save Descriptor Script (It needs to be in a folder named Editor)", unitScript.name + "Descriptor.cs", "cs", "Enter a file name to save the descriptor script as");

        if (string.IsNullOrEmpty(defaultPath))
        {
            return;
        }

        string unitScriptName = unitScript.GetClass().Name;
        string descriptorClassName = unitScriptName + "Descriptor";
        string targetNodeName = unitScriptName;

        string descriptorContent = $@"using Bolt.Addons.Community.Fundamentals;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Descriptor(typeof({unitScriptName}))]
public class {descriptorClassName} : UnitDescriptor<{targetNodeName}>
{{
    public {descriptorClassName}({targetNodeName} target) : base(target)
    {{
    }}";

        if (iconEnabled && customIcon != null)
        {
            descriptorContent += $@"
    protected override EditorTexture DefinedIcon()
    {{
        string iconFullPath = ""{iconPath}"";
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
        return EditorTexture.Single(icon);
    }}";
        }

        descriptorContent += $@"
    protected override string DefinedSummary()
    {{
        return {(!string.IsNullOrEmpty(scriptDescription) ? $"\"{scriptDescription}\"" : "base.DefinedSummary()")};
    }}
}}";

        System.IO.File.WriteAllText(defaultPath, descriptorContent);

        AssetDatabase.Refresh();

        Debug.Log("Descriptor script generated successfully at " + defaultPath);
    }
}
