
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

        protected override string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var machine = (ScriptMachine)asset;
            return Path.Combine(paths.ObjectsRelativePath, GetMachineName(machine).LegalMemberName() + ".cs");
        }

        public override void PostProcess(UnityEngine.Object asset, PathConfig paths, Type type)
        {
            if (asset is not ScriptMachine || type == null) return;
            
            var machine = (ScriptMachine)asset;

            var scriptImporter = AssetImporter.GetAtPath(GetRelativeFilePath(asset, paths)) as MonoImporter;
            var component = machine.gameObject.GetComponent(type);
            if (component == null)
            {
                component = machine.gameObject.AddComponent(type);
            }

            var values = CodeGeneratorValueUtility.GetAllValues(machine, false);
            var variables = machine.graph.variables.Where(v =>
                typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(v.typeHandle.Identification)));

            var objects = variables.Select(v => (v.name, (UnityEngine.Object)v.value))
                .Concat(values.Select(v => (v.Key.LegalMemberName(), v.Value)))
                .ToArray();

            for (int i = 0; i < objects.Length; i++)
            {
                type.GetFields().Where(f => f.IsPublic || f.HasAttribute<SerializeField>()).FirstOrDefault(f => f.Name == objects[i].Item1)?.SetValueOptimized(component, objects[i].Item2);
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
            return machine.nest?.graph.title?.Length > 0
                ? machine.nest.graph.title.LegalMemberName()
                : machine.gameObject.name.Capitalize().First().Letter() + "_ScriptMachine_" + Array.IndexOf(machine.gameObject.GetComponents<ScriptMachine>(), machine);
        }
    }
}