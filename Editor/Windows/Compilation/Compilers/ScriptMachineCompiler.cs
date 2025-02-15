
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ScriptMachineCompiler : BaseCompiler
    {
        private GameObject currentObject;
        private int machineIndex;

        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var machine = (ScriptMachine)asset;
            return Path.Combine(paths.ObjectsPath, GetMachineName(machine).LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var machine = (ScriptMachine)asset;
            var generator = (GameObjectGenerator)GameObjectGenerator.GetSingleDecorator(machine.gameObject);
            generator.current = machine;
            var code = generator.GenerateClean(0);
            return code;
        }

        protected override void PostProcess(UnityEngine.Object asset, string relativePath)
        {
            var machine = (ScriptMachine)asset;
            var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
            var type = scriptImporter.GetScript().GetClass();
            var component = machine.gameObject.GetComponent(type) ??
                          machine.gameObject.AddComponent(type);

            var values = CodeGeneratorValueUtility.GetAllValues(machine);
            var variables = machine.graph.variables.Where(v =>
                typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(v.typeHandle.Identification)));

            var names = variables.Select(v => v.name)
                .Concat(values.Select(v => v.Key.LegalMemberName()))
                .ToArray();

            var objects = variables.Select(v => (UnityEngine.Object)v.value)
                .Concat(values.Select(v => v.Value))
                .ToArray();

            for (int i = 0; i < objects.Length; i++)
            {
                type.GetFields()
                    .Where(f => f.IsPublic || f.HasAttribute<SerializeField>())
                    .FirstOrDefault(f => f.Name == names[i])
                    ?.SetValue(component, objects[i]);
            }
        }

        private string GetMachineName(ScriptMachine machine)
        {
            if (currentObject != machine.gameObject)
            {
                machineIndex = 0;
                currentObject = machine.gameObject;
            }

            return machine.nest?.graph.title?.Length > 0
                ? machine.nest.graph.title.LegalMemberName()
                : machine.gameObject.name + "_ScriptMachine" + machineIndex++;
        }
    }
}