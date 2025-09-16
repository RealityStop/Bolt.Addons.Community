using UnityEngine;
using UnityEditor;
<<<<<<< Updated upstream:Editor/Windows/UnitDescriptorGenerator.cs
using Unity.VisualScripting;
=======
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
>>>>>>> Stashed changes:Editor/Windows/DescriptorCreationWizard.cs

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
<<<<<<< Updated upstream:Editor/Windows/UnitDescriptorGenerator.cs
            return;
=======
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (input.Length == 1)
                return char.ToUpper(input[0]).ToString();

            return char.ToUpper(input[0]) + input[1..];
>>>>>>> Stashed changes:Editor/Windows/DescriptorCreationWizard.cs
        }

        string unitScriptName = unitScript.GetClass().Name;
        string descriptorClassName = unitScriptName + "Descriptor";
        string targetNodeName = unitScriptName;

<<<<<<< Updated upstream:Editor/Windows/UnitDescriptorGenerator.cs
        string descriptorContent = $@"using Unity.VisualScripting;
=======
            string defaultPath = EditorUtility.SaveFilePanelInProject("Save Descriptor Script (It needs to be in a folder named Editor)", unitScript.name + "Descriptor.cs", "cs", "Enter a file name to save the descriptor script as");

            if (string.IsNullOrEmpty(defaultPath))
            {
                return;
            }

            Type type = unitScript.GetClass();
            string unitScriptName = type.Name;
            string unitNamespace = type.Namespace;
            string descriptorClassName = unitScriptName + "Descriptor";
            string targetNodeName = unitScriptName;

            string descriptorContent = $@"using Unity.VisualScripting;
>>>>>>> Stashed changes:Editor/Windows/DescriptorCreationWizard.cs
using UnityEditor;
using UnityEngine;{(!string.IsNullOrEmpty(unitNamespace) ? $"\nusing {unitNamespace};" : "")}

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
