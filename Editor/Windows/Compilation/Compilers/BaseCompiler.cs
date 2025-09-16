using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEditor.Compilation;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseCompiler : IAssetCompiler
    {
        private static Dictionary<System.Reflection.Assembly, bool> _editorAssemblyCache = new Dictionary<System.Reflection.Assembly, bool>();

        protected abstract string GetFilePath(UnityEngine.Object asset, PathConfig paths);
        protected abstract string GenerateCode(UnityEngine.Object asset);
        protected abstract void PostProcess(UnityEngine.Object asset, PathConfig paths);

        public void Compile(UnityEngine.Object asset, PathConfig paths)
        {
            string fullPath = GetFilePath(asset, paths);
            HUMIO.Delete(fullPath);
            HUMIO.Ensure(fullPath).Path();

            string code = GenerateCode(asset);
            HUMIO.Save(code).Custom(fullPath).Text(false);

            CompilationPipeline.compilationFinished += (v) =>
            {
                PostProcess(asset, paths);
            };
        }

        protected bool IsEditorAssembly(System.Reflection.Assembly assembly, HashSet<string> visited)
        {
            if (_editorAssemblyCache.TryGetValue(assembly, out var isEditor))
            {
                return isEditor;
            }

            var name = assembly.GetName().Name;

            // Early exit for known runtime assemblies
            if (IsRuntimeAssembly(name))
            {
                _editorAssemblyCache[assembly] = false;
                return false;
            }

            // Early return for explicit editor assemblies
            if (IsExplicitEditorAssembly(assembly, name))
            {
                _editorAssemblyCache[assembly] = true;
                return true;
            }

            // Prevent circular references
            if (visited.Contains(name))
            {
                return false;
            }
            visited.Add(name);

            // Check direct editor dependencies only
            var editorDependency = assembly.GetReferencedAssemblies()
                .Where(dep => !visited.Contains(dep.Name))
                .Any(dep =>
                {
                    try
                    {
                        var dependencyAssembly = System.Reflection.Assembly.Load(dep);
                        return IsEditorAssembly(dependencyAssembly, visited);
                    }
                    catch
                    {
                        return false;
                    }
                });

            _editorAssemblyCache[assembly] = editorDependency;
            return editorDependency;
        }

        private static bool IsRuntimeAssembly(string name)
        {
            return name == "UnityEngine.UI" ||
                   name == "Unity.TextMeshPro" ||
                   name == "Assembly-CSharp" ||
                   name == "Assembly-CSharp-firstpass" ||
                   name == "Unity.VisualScripting.Flow" ||
                   name == "Unity.VisualScripting.Core" ||
                   name == "Unity.VisualScripting.State" ||
                   name.StartsWith("UnityEngine.") ||
                   name.StartsWith("Unity.") && !name.Contains("Editor");
        }

        private static bool IsExplicitEditorAssembly(System.Reflection.Assembly assembly, string name)
        {
            return Attribute.IsDefined(assembly, typeof(AssemblyIsEditorAssembly)) ||
                   name == "Assembly-CSharp-Editor" ||
                   name == "Assembly-CSharp-Editor-firstpass" ||
                   name == "UnityEditor" ||
                   name == "UnityEditor.CoreModule" ||
                   name.StartsWith("UnityEditor.") ||
                   name.EndsWith(".Editor") ||
                   name.Contains(".Editor.");
        }
    }
}
