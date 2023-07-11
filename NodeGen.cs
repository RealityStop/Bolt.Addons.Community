using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

public class CustomNodeGeneratorWindow : EditorWindow
{
    private List<InputData> controlInputs = new List<InputData>();
    private List<InputData> controlOutputs = new List<InputData>();
    private List<ValueInputData> valueInputs = new List<ValueInputData>();
    private List<ValueOutputData> valueOutputs = new List<ValueOutputData>();
    private string fileName = "MyCustomNode";
    private bool useUnit;
    private bool useMachineEventUnit;
    private bool useEventUnit;


    private Vector2 scrollPosition;

    [MenuItem("Window/Community Addons/Custom Node Generator")]
    public static void ShowWindow()
    {
        GetWindow<CustomNodeGeneratorWindow>("Custom Node Generator");
    }

    private int selectedOption = 0;
    private string[] options = { "Unit", "MachineEventUnit", "EventUnit" };

    private void OnGUI()
    {
        GUILayout.Label("Custom Node Generator", EditorStyles.boldLabel);

        fileName = EditorGUILayout.TextField("File Name", fileName);

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUI.BeginChangeCheck();

        selectedOption = EditorGUILayout.Popup("Select Option", selectedOption, options);

        if (EditorGUI.EndChangeCheck())
        {
            useUnit = selectedOption == 0;
            useMachineEventUnit = selectedOption == 1;
            useEventUnit = selectedOption == 2;
        }

        DrawInputList(controlInputs, "Control Inputs", false);
        DrawInputList(controlOutputs, "Control Outputs", false);
        DrawValueInputList(valueInputs, "Value Inputs");
        DrawValueOutputList(valueOutputs, "Value Outputs");

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate Custom Node"))
        {
            GenerateCustomNodeCode();
        }
    }



    private void DrawInputList(List<InputData> inputs, string title, bool showTypeOptions = true)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);

        for (int i = 0; i < inputs.Count; i++)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Input {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                inputs.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();

            inputs[i].name = EditorGUILayout.TextField("Name", inputs[i].name);
            inputs[i].HideLabel = EditorGUILayout.Toggle("Hide Label", inputs[i].HideLabel);

            if (showTypeOptions)
            {
                inputs[i].type = DrawTypeDropdown(inputs[i].type);
            }

            GUILayout.EndVertical();

            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add " + title.Substring(0, title.Length - 1)))
        {
            inputs.Add(new InputData());
        }

        EditorGUILayout.Space();
    }

    private void DrawValueInputList(List<ValueInputData> valueInputs, string title)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);

        for (int i = 0; i < valueInputs.Count; i++)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Value Input {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                valueInputs.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();

            valueInputs[i].name = EditorGUILayout.TextField("Name", valueInputs[i].name);
            valueInputs[i].type = DrawTypeDropdown(valueInputs[i].type);
            valueInputs[i].HideLabel = EditorGUILayout.Toggle("Hide Label", valueInputs[i].HideLabel);

            if (valueInputs[i].type == typeof(GameObject))
            {
                valueInputs[i].nullMeansSelf = EditorGUILayout.Toggle("Null Means Self", valueInputs[i].nullMeansSelf);
            }

            GUILayout.EndVertical();

            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add " + title.Substring(0, title.Length - 1)))
        {
            valueInputs.Add(new ValueInputData());
        }

        EditorGUILayout.Space();
    }

    private void DrawValueOutputList(List<ValueOutputData> valueOutputs, string title)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);

        for (int i = 0; i < valueOutputs.Count; i++)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Value Output {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                valueOutputs.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();

            valueOutputs[i].name = EditorGUILayout.TextField("Name", valueOutputs[i].name);
            valueOutputs[i].HideLabel = EditorGUILayout.Toggle("Hide Label", valueOutputs[i].HideLabel);
            valueOutputs[i].type = DrawTypeDropdown(valueOutputs[i].type);

            GUILayout.EndVertical();

            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add " + title.Substring(0, title.Length - 1)))
        {
            valueOutputs.Add(new ValueOutputData());
        }

        EditorGUILayout.Space();
    }

    private Type DrawTypeDropdown(Type selectedType)
    {
        Type[] types = GetCompatibleTypes();

        int selectedIndex = Array.IndexOf(types, selectedType);
        if (selectedIndex < 0)
            selectedIndex = 0;

        selectedIndex = EditorGUILayout.Popup("Type", selectedIndex, GetReadableTypeNames(types));

        return types[selectedIndex];
    }

    private Type[] GetCompatibleTypes()
    {
        return new Type[]
        {
        typeof(bool),
        typeof(int),
        typeof(float),
        typeof(string),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(GameObject),
        typeof(Transform),
        typeof(Color),
            // Add more compatible types here...
        };
    }

    private string[] GetReadableTypeNames(Type[] types)
    {
        string[] typeNames = new string[types.Length];
        for (int i = 0; i < types.Length; i++)
        {
            typeNames[i] = GetShortTypeName(types[i]);
        }
        return typeNames;
    }

    private string GetShortTypeName(Type type)
    {
        if (type == typeof(bool)) return "bool";
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(string)) return "string";
        return type.Name;
    }

    private void GenerateCustomNodeCode()
{
    string code = GenerateCustomNodeTemplate(fileName, GetSelectedUnitType());
    SaveGeneratedCode(code, fileName);
}

private string GetSelectedUnitType()
{
    if (useMachineEventUnit)
        return "MachineEventUnit<EmptyEventArgs>";
    else if (useEventUnit)
        return "EventUnit<EmptyEventArgs>";
    else
        return "Unit";
}


    private string GenerateCustomNodeTemplate(string fileName, string unitType)
    {
        string template = $@"using Bolt.Addons.Community.Fundamentals;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle(""{fileName}"")]//Unit title
[TypeIcon(typeof(object))]//Unit icon
public class {fileName.Replace(" ", "")} : {unitType}
{{
    ";

        if (useEventUnit)
        {
            template += @"
    protected override bool register => throw new System.NotImplementedException();
";
        }

            // Setup variables for ControlInputs
            for (int i = 0; i < controlInputs.Count; i++)
        {
            string inputName = string.IsNullOrEmpty(controlInputs[i].name) ? $"ControlInput{i}" : controlInputs[i].name;
            template += $@"
    [DoNotSerialize]";
if (controlInputs[i].HideLabel) 
                {{
                    template += $@"
    [PortLabelHidden]";
                }}
                template += $@"
    public ControlInput {inputName};";
        }

        // Setup variables for ControlOutputs
        for (int i = 0; i < controlOutputs.Count; i++)
        {
            string outputName = string.IsNullOrEmpty(controlOutputs[i].name) ? $"ControlOutput{i}" : controlOutputs[i].name;
            template += $@"
    [DoNotSerialize]";
if (controlOutputs[i].HideLabel) 
                {{
                    template += $@"
    [PortLabelHidden]";
                }}
                template += $@"
    public ControlOutput {outputName};";
        }

        // Setup variables for ValueInputs
        for (int i = 0; i < valueInputs.Count; i++)
        {
            if (valueInputs[i].type == typeof(GameObject) && valueInputs[i].nullMeansSelf)
            {
                string inputName = string.IsNullOrEmpty(valueInputs[i].name) ? $"ValueInput{i}" : valueInputs[i].name;
                template += $@"
    [NullMeansSelf]";
                if (valueInputs[i].HideLabel) 
                {
                    template += $@"
    [PortLabelHidden]";
                }
                template += $@"
    [DoNotSerialize]
    public ValueInput {inputName};";
            }
            else
            {
                string inputName = string.IsNullOrEmpty(valueInputs[i].name) ? $"ValueInput{i}" : valueInputs[i].name;
                template += $@"
    [DoNotSerialize]";
    if (valueInputs[i].HideLabel) 
                {{
                    template += $@"
    [PortLabelHidden]";
                }}
                template += $@"
    public ValueInput {inputName};";
            }
        }

        // Setup variables for ValueOutputs
        for (int i = 0; i < valueOutputs.Count; i++)
        {
            string outputName = string.IsNullOrEmpty(valueOutputs[i].name) ? $"ValueOutput{i}" : valueOutputs[i].name;
            template += $@"
    [DoNotSerialize]";
    if (valueOutputs[i].HideLabel) 
                {{
                    template += $@"
    [PortLabelHidden]";
                }}
                template += $@"
    public ValueOutput {outputName};";
        }

        // Definition method
        template += @"
    protected override void Definition()
    {";
        if (useEventUnit || useMachineEventUnit)
        {
            template += @"
        base.Definition();";
        }

            // ControlInputs
            foreach (InputData input in controlInputs)
        {
            string inputName = string.IsNullOrEmpty(input.name) ? $"ControlInput{controlInputs.IndexOf(input)}" : input.name;
            template += $@"
        {inputName} = ControlInput(nameof({inputName}), _Enter_);";
        }

        // ControlOutputs
        foreach (InputData output in controlOutputs)
        {
            string outputName = string.IsNullOrEmpty(output.name) ? $"ControlOutput{controlOutputs.IndexOf(output)}" : output.name;
            template += $@"
        {outputName} = ControlOutput(nameof({outputName}));";
        }

        // ValueInputs
        foreach (ValueInputData valueInput in valueInputs)
        {
            string valueInputName = string.IsNullOrEmpty(valueInput.name) ? $"ValueInput{valueInputs.IndexOf(valueInput)}" : valueInput.name;

            if (valueInput.type == typeof(GameObject) && valueInput.nullMeansSelf)
            {
                template += $@"
        {valueInputName} = ValueInput<{GetShortTypeName(valueInput.type)}>(nameof({valueInputName}), default).NullMeansSelf();";
            }
            else
            {
                template += $@"
        {valueInputName} = ValueInput<{GetShortTypeName(valueInput.type)}>(nameof({valueInputName}), default);";
            }
        }

        // ValueOutputs
        foreach (ValueOutputData valueOutput in valueOutputs)
        {
            string valueOutputName = string.IsNullOrEmpty(valueOutput.name) ? $"ValueOutput{valueOutputs.IndexOf(valueOutput)}" : valueOutput.name;
            template += $@"
        {valueOutputName} = ValueOutput<{GetShortTypeName(valueOutput.type)}>(nameof({valueOutputName}));";
        }

        foreach (InputData input in controlInputs)
        {
            foreach (InputData outputData in controlOutputs)
            {
                string outputName = string.IsNullOrEmpty(outputData.name) ? $"ControlOutput{controlOutputs.IndexOf(outputData)}" : outputData.name;
                template += $@"
        Succession({input.name}, {outputName});";
            }
        }

        template += @"
    }

    public ControlOutput _Enter_(Flow flow) 
    {

        //Enter your logic here, You can add more methods if needed.

        //Put the name of the output you want to trigger when the node is entered.
        return null;

    }
}";

        return template;
    }

    private void SaveGeneratedCode(string code, string fileName)
    {
        string filePath = $"Assets/{fileName.Replace(" ", "")}.cs";
        System.IO.File.WriteAllText(filePath, code);
        AssetDatabase.Refresh();
        Debug.Log($"Generated custom node code saved at {filePath}");
    }
}


[Serializable]
public class InputData
{
    public string name;
    public bool HideLabel;
    public Type type;
}

[Serializable]
public class ValueInputData
{
    public string name;
    public Type type;
    public bool HideLabel;
    public bool nullMeansSelf;
}

[Serializable]
public class ValueOutputData
{
    public string name;
    public bool HideLabel;
    public Type type;
}
