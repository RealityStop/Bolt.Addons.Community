
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

        private string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var machine = (ScriptMachine)asset;
            return Path.Combine(paths.ObjectsRelativePath, GetMachineName(machine).LegalMemberName() + ".cs");
        }
<<<<<<< Updated upstream

        protected override void PostProcess(UnityEngine.Object asset, PathConfig paths)
        {
            var machine = (ScriptMachine)asset;
            var scriptImporter = AssetImporter.GetAtPath(GetRelativeFilePath(asset, paths)) as MonoImporter;
            var type = scriptImporter.GetScript().GetClass();
            var component = machine.gameObject.GetComponent(type) ??
                          machine.gameObject.AddComponent(type);

=======

        protected override void PostProcess(UnityEngine.Object asset, PathConfig paths)
        {
            var machine = (ScriptMachine)asset;
            var scriptImporter = AssetImporter.GetAtPath(GetRelativeFilePath(asset, paths)) as MonoImporter;
            var type = scriptImporter.GetScript().GetClass();
            var component = machine.gameObject.GetComponent(type);
            if (component == null)
            {
                component = machine.gameObject.AddComponent(type);
            }

>>>>>>> Stashed changes
            var values = CodeGeneratorValueUtility.GetAllValues(machine, false);
            var variables = machine.graph.variables.Where(v =>
                typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(v.typeHandle.Identification)));

            var objects = variables.Select(v => (v.name, (UnityEngine.Object)v.value))
                .Concat(values.Select(v => (v.Key.LegalMemberName(), v.Value)))
                .ToArray();

            for (int i = 0; i < objects.Length; i++)
            {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
                type.GetFields()
                    .Where(f => f.IsPublic || f.HasAttribute<SerializeField>())
                    .FirstOrDefault(f => f.Name == names[i])
                    ?.SetValue(component, objects[i]);
=======
                type.GetFields().Where(f => f.IsPublic || f.HasAttribute<SerializeField>()).FirstOrDefault(f => f.Name == objects[i].Item1)?.SetValueOptimized(component, objects[i].Item2);
>>>>>>> Stashed changes
=======
                type.GetFields().Where(f => f.IsPublic || f.HasAttribute<SerializeField>()).FirstOrDefault(f => f.Name == objects[i].Item1)?.SetValueOptimized(component, objects[i].Item2);
>>>>>>> Stashed changes
            }

            if (!objects.Any(o => !EditorUnityObjectUtility.IsSceneBound(o.Item2))) return;

            // Set any default references
            var nonSceneObjects = objects.Where(o => !EditorUnityObjectUtility.IsSceneBound(o.Item2));
            string[] names = nonSceneObjects.Select(v => v.Item1).ToArray();
            UnityEngine.Object[] objs = nonSceneObjects.Select(v => v.Item2).ToArray();
            scriptImporter.SetDefaultReferences(names, objs);
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