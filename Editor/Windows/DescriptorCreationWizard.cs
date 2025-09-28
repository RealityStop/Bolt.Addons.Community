using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("CustomScriptGeneratorWindow")]
    public sealed class DescriptorCreationWizard : EditorWindow
    {
        [SerializeField]
        private Type unitType;

        private bool customizeDefined = true;
        private bool customizeDefault = false;
        private bool customizeError = false;

        private string definedTitle = "";
        private string definedShortTitle = "";
        private string definedSurtitle = "";
        private string definedSubtitle = "";
        private string definedSummary = "";

        private string defaultTitle = "";
        private string defaultShortTitle = "";
        private string defaultSurtitle = "";
        private string defaultSubtitle = "";
        private string defaultSummary = "";

        private string errorTitle = "";
        private string errorShortTitle = "";
        private string errorSurtitle = "";
        private string errorSubtitle = "";
        private string errorSummary = "";

        private bool definedIconEnabled = false;
        private Texture2D definedIcon = null;
        private string definedIconPath = "";

        private bool defaultIconEnabled = false;
        private Texture2D defaultIcon = null;
        private string defaultIconPath = "";

        private bool errorIconEnabled = false;
        private Texture2D errorIcon = null;
        private string errorIconPath = "";

        private bool portsEnabled = false;
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

        Vector2 scrollPosition;
        private void OnGUI()
        {
            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, 1, () =>
            {
                GUILayout.Label("Descriptor Creation Wizard", EditorStyles.boldLabel);
                GUILayout.Space(10);

                if (TypeBuilderWindow.Button(unitType))
                {
                    var types = Codebase.settingsAssembliesTypes
                            .Where(t => typeof(Unit).IsAssignableFrom(t) && !t.IsDefined(typeof(ObsoleteAttribute)) && DescriptorProvider.instance.GetDecoratorType(t) == typeof(UnitDescriptor<IUnit>))
                            .ToArray() ?? Array.Empty<Type>();
                    if (unitType == null && types.Length > 0) { unitType = types[0]; }
                    TypeBuilderWindow.ShowWindow(
                        GUILayoutUtility.GetLastRect(),
                        t => unitType = t,
                        unitType,
                        false,
                        types
                    );
                }

                if (unitType == null || !typeof(Unit).IsAssignableFrom(unitType))
                {
                    EditorGUILayout.HelpBox("The selected script does not contain a valid class or is not a Unit.", MessageType.Error);
                    return;
                }
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawCustomizationGUI();
                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);
                if (GUILayout.Button("Generate"))
                {
                    GenerateDescriptor();
                }
            }, true, true);
        }

        private void DrawCustomizationGUI()
        {
            customizeDefined = EditorGUILayout.Foldout(customizeDefined, "Customize Defined State");
            if (customizeDefined)
            {
                definedTitle = EditorGUILayout.TextField("Title", definedTitle);
                definedShortTitle = EditorGUILayout.TextField("Short Title", definedShortTitle);
                definedSurtitle = EditorGUILayout.TextField("Surtitle", definedSurtitle);
                definedSubtitle = EditorGUILayout.TextField("Subtitle", definedSubtitle);
                definedSummary = EditorGUILayout.TextField("Summary", definedSummary);
                definedIconEnabled = EditorGUILayout.Toggle("Use Custom Icon", definedIconEnabled);
                if (definedIconEnabled)
                {
                    definedIcon = EditorGUILayout.ObjectField("Icon", definedIcon, typeof(Texture2D), false) as Texture2D;
                    definedIconPath = AssetDatabase.GetAssetPath(definedIcon);
                }

                portsEnabled = EditorGUILayout.Toggle("Customize Ports", portsEnabled);
                if (portsEnabled)
                {
                    var ports = new List<MemberInfo>();
                    ports.AddRange(unitType.GetFields().Where(f => typeof(IUnitPort).IsAssignableFrom(f.FieldType)));
                    ports.AddRange(unitType.GetProperties()
                        .Where(p => typeof(IUnitPort).IsAssignableFrom(p.PropertyType) && p.GetIndexParameters().Length == 0 && p.CanRead && p.GetGetMethod(true) != null));

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
            }

            customizeDefault = EditorGUILayout.Foldout(customizeDefault, "Customize Default State");
            if (customizeDefault)
            {
                defaultTitle = EditorGUILayout.TextField("Title", defaultTitle);
                defaultShortTitle = EditorGUILayout.TextField("Short Title", defaultShortTitle);
                defaultSurtitle = EditorGUILayout.TextField("Surtitle", defaultSurtitle);
                defaultSubtitle = EditorGUILayout.TextField("Subtitle", defaultSubtitle);
                defaultSummary = EditorGUILayout.TextField("Summary", defaultSummary);
                defaultIconEnabled = EditorGUILayout.Toggle("Use Custom Icon", defaultIconEnabled);
                if (defaultIconEnabled)
                {
                    defaultIcon = EditorGUILayout.ObjectField("Icon", defaultIcon, typeof(Texture2D), false) as Texture2D;
                    defaultIconPath = AssetDatabase.GetAssetPath(defaultIcon);
                }
            }

            customizeError = EditorGUILayout.Foldout(customizeError, "Customize Error State");
            if (customizeError)
            {
                errorTitle = EditorGUILayout.TextField("Title", errorTitle);
                errorShortTitle = EditorGUILayout.TextField("Short Title", errorShortTitle);
                errorSurtitle = EditorGUILayout.TextField("Surtitle", errorSurtitle);
                errorSubtitle = EditorGUILayout.TextField("Subtitle", errorSubtitle);
                errorSummary = EditorGUILayout.TextField("Summary", errorSummary);
                errorIconEnabled = EditorGUILayout.Toggle("Use Custom Icon", errorIconEnabled);
                if (errorIconEnabled)
                {
                    errorIcon = EditorGUILayout.ObjectField("Icon", errorIcon, typeof(Texture2D), false) as Texture2D;
                    errorIconPath = AssetDatabase.GetAssetPath(errorIcon);
                }
            }
        }

        string CapitalizeFirst(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length == 1 ? char.ToUpper(input[0]).ToString() : char.ToUpper(input[0]) + input.Substring(1);
        }

        private void GenerateDescriptor()
        {
            if (unitType == null)
            {
                Debug.Log("Please select a unit type.");
                return;
            }

            string defaultPath = EditorUtility.SaveFilePanelInProject(
                "Save Descriptor Script (It needs to be in a folder named Editor)",
                unitType.HumanName(true) + "Descriptor.cs",
                "cs",
                "Enter a file name to save the descriptor script as"
            );

            if (string.IsNullOrEmpty(defaultPath)) return;

            string unitName = unitType.As().CSharpName(false, true, false);
            string unitNamespace = unitType.Namespace;
            string descriptorClassName = unitType.HumanName(true).Replace(" ", "") + "Descriptor";

            var descriptorClass = ClassGenerator.Class(
                RootAccessModifier.Public,
                ClassModifier.None,
                descriptorClassName,
                $"UnitDescriptor<{unitName}>",
                unitNamespace, new List<string> { "Unity.VisualScripting", "UnityEditor", "UnityEngine", "System" }
            );

            descriptorClass.generateUsings = true;
            if (!string.IsNullOrEmpty(unitNamespace)) descriptorClass.AddUsings(unitNamespace.Yield().ToList());

            var constructor = ConstructorGenerator.Constructor(AccessModifier.Public, ConstructorModifier.None, ConstructorInitializer.Base, descriptorClassName);
            constructor.AddParameter(true, ParameterGenerator.Parameter("target", unitType, Libraries.CSharp.ParameterModifier.None));

            descriptorClass.AddConstructor(constructor);

            AddTextMethod(descriptorClass, "Title", definedTitle, defaultTitle, errorTitle);
            AddTextMethod(descriptorClass, "ShortTitle", definedShortTitle, defaultShortTitle, errorShortTitle);
            AddTextMethod(descriptorClass, "Surtitle", definedSurtitle, defaultSurtitle, errorSurtitle);
            AddTextMethod(descriptorClass, "Subtitle", definedSubtitle, defaultSubtitle, errorSubtitle);
            AddTextMethod(descriptorClass, "Summary", definedSummary, defaultSummary, errorSummary);

            AddIconMethod(descriptorClass, "DefinedIcon", definedIconEnabled, definedIconPath);
            AddIconMethod(descriptorClass, "DefaultIcon", defaultIconEnabled, defaultIconPath);
            AddIconMethod(descriptorClass, "ErrorIcon", errorIconEnabled, errorIconPath, includeException: true);

            if (portsEnabled && portInfos.Count > 0 && portInfos.Any(i => !string.IsNullOrEmpty(i.Value.Label) || string.IsNullOrEmpty(i.Value.Description)))
            {
                var portMethod = MethodGenerator.Method(AccessModifier.Protected, MethodModifier.Override, typeof(void), "DefinedPort");
                portMethod.AddParameter(ParameterGenerator.Parameter("port", typeof(IUnitPort), Libraries.CSharp.ParameterModifier.None));
                portMethod.AddParameter(ParameterGenerator.Parameter("description", typeof(UnitPortDescription), Libraries.CSharp.ParameterModifier.None));


                var body = new StringBuilder();
                body.AppendLine("base.DefinedPort(port, description);");

                foreach (var kvp in portInfos)
                {
                    var fieldName = kvp.Value.FieldName;
                    var label = kvp.Value.Label;
                    var summary = kvp.Value.Description;

                    if (string.IsNullOrEmpty(label) && string.IsNullOrEmpty(summary)) continue;

                    body.AppendLine($"if (port == unit.{fieldName})");
                    body.AppendLine("{");
                    if (!string.IsNullOrEmpty(label)) body.AppendLine($"    description.label = \"{label}\";");
                    if (!string.IsNullOrEmpty(summary)) body.AppendLine($"    description.summary = \"{summary}\";");
                    body.AppendLine("}");
                }

                portMethod.body = body.ToString();
                descriptorClass.AddMethod(portMethod);
            }

            string content = descriptorClass.GenerateClean(0);
            System.IO.File.WriteAllText(defaultPath, content);
            AssetDatabase.Refresh();
            Debug.Log("Descriptor script generated successfully at " + defaultPath);
        }

        private void AddTextMethod(ClassGenerator cls, string methodName, string defined, string def, string error)
        {
            if (!string.IsNullOrEmpty(defined))
            {
                var method = MethodGenerator.Method(AccessModifier.Protected, MethodModifier.Override, typeof(string), "Defined" + methodName);
                method.Body($"return \"{defined}\";");
                cls.AddMethod(method);
            }

            if (!string.IsNullOrEmpty(def))
            {
                var method = MethodGenerator.Method(AccessModifier.Protected, MethodModifier.Override, typeof(string), "Default" + methodName);
                method.Body($"return \"{def}\";");
                cls.AddMethod(method);
            }

            if (!string.IsNullOrEmpty(error))
            {
                cls.AddUsings(typeof(Exception).Namespace.Yield().ToList());
                var method = MethodGenerator.Method(AccessModifier.Protected, MethodModifier.Override, typeof(string), "Error" + methodName);
                method.AddParameter(ParameterGenerator.Parameter("exception", typeof(Exception), Libraries.CSharp.ParameterModifier.None));
                method.Body($"return $\"{ExtractCodes(error)}\";");
                cls.AddMethod(method);
            }
        }

        private string ExtractCodes(string error)
        {
            return error
            .Replace("{message}", "{exception.Message}", StringComparison.OrdinalIgnoreCase)
            .Replace("{ms}", "{exception.Message}", StringComparison.OrdinalIgnoreCase)
            .Replace("{st}", "{exception.StackTrace}", StringComparison.OrdinalIgnoreCase)
            .Replace("{stacktrace}", "{exception.StackTrace}", StringComparison.OrdinalIgnoreCase);
        }

        private void AddIconMethod(ClassGenerator cls, string methodName, bool enabled, string path, bool includeException = false)
        {
            if (!enabled || string.IsNullOrEmpty(path)) return;
            var method = MethodGenerator.Method(AccessModifier.Protected, MethodModifier.Override, typeof(EditorTexture), methodName);

            if (includeException)
            {
                method.AddParameter(ParameterGenerator.Parameter("exception", typeof(Exception), Libraries.CSharp.ParameterModifier.None));
                cls.AddUsings(typeof(Exception).Namespace.Yield().ToList());
            }

            method.body = $"Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(\"{path}\");\nreturn EditorTexture.Single(icon);";

            cls.AddMethod(method);
        }
    }
}