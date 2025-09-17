using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("CustomScriptGeneratorWindow")]
    public class DescriptorCreationWizard : EditorWindow
    {
        private MonoScript unitScript;
        private bool includeDescription = false;
        private string scriptDescription = "";
        private bool iconEnabled = false;
        private bool portsEnabled = false;
        private Texture2D customIcon = null;
        private string iconPath = "";

        private Dictionary<MemberInfo, PortInfo> portInfos = new Dictionary<MemberInfo, PortInfo>();
        private class PortInfo
        {
            public string Label { get; set; }
            public string Description { get; set; }
            public string FieldName { get; set; }
            public bool IsOpen { get; set; }
        }

        [MenuItem("Window/Community Addons/Descriptor Creation Wizard")]
        public static void ShowWindow()
        {
            GetWindow<DescriptorCreationWizard>("Descriptor Creation Wizard");
        }

        private void OnGUI()
        {
            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, 1, () =>
            {
                GUILayout.Label("Descriptor Creation Wizard", EditorStyles.boldLabel);

                GUILayout.Space(10);

                unitScript = EditorGUILayout.ObjectField("Unit Script", unitScript, typeof(MonoScript), false) as MonoScript;
                var unitType = unitScript != null ? unitScript.GetClass() : null;
                if (unitScript == null || !typeof(Unit).IsAssignableFrom(unitType))
                {
                    EditorGUILayout.HelpBox("The selected script does not contain a valid class or is not a Unit.", MessageType.Error);
                    return;
                }

                includeDescription = EditorGUILayout.Toggle("Include Description", includeDescription);

                if (includeDescription)
                {
                    scriptDescription = EditorGUILayout.TextField("Unit Description", scriptDescription);
                }

                iconEnabled = EditorGUILayout.Toggle("Use Custom Icon", iconEnabled);

                if (iconEnabled)
                {
                    customIcon = EditorGUILayout.ObjectField("Custom Icon", customIcon, typeof(Texture2D), false) as Texture2D;
                    iconPath = AssetDatabase.GetAssetPath(customIcon);
                }

                portsEnabled = EditorGUILayout.Toggle("Customize ports", portsEnabled);

                if (portsEnabled)
                {
                    var ports = new List<MemberInfo>();

                    ports.AddRange(unitType.GetFields().Where(f => typeof(IUnitPort).IsAssignableFrom(f.FieldType)));

                    ports.AddRange(unitType.GetProperties().Where(p => typeof(IUnitPort).IsAssignableFrom(p.PropertyType) && p.GetIndexParameters().Length == 0 && p.CanRead && p.GetGetMethod(true) != null));

                    foreach (var port in ports)
                    {
                        portInfos.TryGetValue(port, out var info);

                        string label = info?.Label ?? string.Empty;
                        string description = info?.Description ?? string.Empty;
                        bool isOpen = info?.IsOpen ?? false;

                        isOpen = HUMEditor.Foldout(isOpen, new GUIContent(CapitalizeFirst(port.Name)), HUMColor.Grey(0.15f), Color.black, 1, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, 1, () =>
                            {
                                label = EditorGUILayout.TextField("Port Label", label);
                                description = EditorGUILayout.TextField("Port Description", description);
                            });
                        });

                        portInfos[port] = new PortInfo
                        {
                            Label = label,
                            Description = description,
                            FieldName = port.Name,
                            IsOpen = isOpen
                        };
                    }

                }

                GUILayout.Space(10);

                if (GUILayout.Button("Generate"))
                {
                    GenerateDescriptor();
                }
            }, true, true);
        }

        string CapitalizeFirst(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (input.Length == 1)
                return char.ToUpper(input[0]).ToString();

            return char.ToUpper(input[0]) + input[1..];
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

            Type type = unitScript.GetClass();
            string unitScriptName = type.Name;
            string unitNamespace = type.Namespace;
            string descriptorClassName = unitScriptName + "Descriptor";
            string targetNodeName = unitScriptName;

            string descriptorContent = $@"using Unity.VisualScripting;

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
            if (iconEnabled && customIcon != null)
            {descriptorContent += $@"
    protected override EditorTexture DefinedIcon()
    {{
        string iconFullPath = ""{iconPath}"";
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
        return EditorTexture.Single(icon);
    }}"}
";
            }

            descriptorContent += $@"
    protected override string DefinedSummary()
    {{
        return {(!string.IsNullOrEmpty(scriptDescription) ? $"\"{scriptDescription}\"" : "base.DefinedSummary()")};
    }}";

            if (portsEnabled)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"\n{CodeBuilder.Indent(1)}protected override void DefinedPort(IUnitPort port, UnitPortDescription description)");
                builder.AppendLine(CodeBuilder.Indent(1) + "{");
                builder.AppendLine(CodeBuilder.Indent(2) + "base.DefinedPort(port, description);");

                foreach (var kvp in portInfos)
                {
                    var fieldName = kvp.Value.FieldName;
                    var label = kvp.Value.Label;
                    var summary = kvp.Value.Description;

                    if (string.IsNullOrEmpty(label) && string.IsNullOrEmpty(summary))
                        continue;

                    builder.AppendLine($"{CodeBuilder.Indent(2)}if (port == unit.{fieldName})");
                    builder.AppendLine(CodeBuilder.Indent(2) + "{");

                    if (!string.IsNullOrEmpty(label))
                        builder.AppendLine(CodeBuilder.Indent(3) + $"description.label = \"{label}\";");

                    if (!string.IsNullOrEmpty(summary))
                        builder.AppendLine(CodeBuilder.Indent(3) + $"description.summary = \"{summary}\";");

                    builder.AppendLine(CodeBuilder.Indent(2) + "}");
                }

                builder.AppendLine(CodeBuilder.Indent(1) + "}");

                descriptorContent += builder.ToString();
            }
            descriptorContent += $@"
}}";

            System.IO.File.WriteAllText(defaultPath, descriptorContent);

            AssetDatabase.Refresh();

            Debug.Log("Descriptor script generated successfully at " + defaultPath);
        }
    }
}