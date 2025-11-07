using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;

namespace Unity.VisualScripting.Community.CSharp
{
    public class ClassAssetCompiler : BaseCompiler
    {
        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var classAsset = (ClassAsset)asset;
            if (classAsset.inheritsType && IsEditorAssembly(classAsset.GetInheritedType().Assembly, new HashSet<string>()))
                return Path.Combine(paths.EditorPath, classAsset.title.LegalMemberName() + ".cs");
            return Path.Combine(paths.ObjectsPath, classAsset.title.LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var classAsset = (ClassAsset)asset;
            var code = ClassAssetGenerator.GetSingleDecorator(classAsset).GenerateClean(0);
            return code;
        }

        protected override string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var classAsset = (ClassAsset)asset;
            if (classAsset.inheritsType && IsEditorAssembly(classAsset.GetInheritedType().Assembly, new HashSet<string>()))
                return Path.Combine(paths.EditorRelativePath, classAsset.title.LegalMemberName() + ".cs");
            return Path.Combine(paths.ObjectsRelativePath, classAsset.title.LegalMemberName() + ".cs");
        }

        public override void PostProcess(UnityEngine.Object asset, PathConfig paths, Type type)
        {
            var classAsset = (ClassAsset)asset;
            var scriptImporter = AssetImporter.GetAtPath(GetRelativeFilePath(asset, paths)) as MonoImporter;
            var values = CodeGeneratorValueUtility.GetAllValues(classAsset);
            var variables = classAsset.variables.Where(v => typeof(UnityEngine.Object).IsAssignableFrom(v.type));

            string[] names = variables.Select(v => v.name)
                                                .Concat(values.Select(v => v.Key.LegalMemberName()))
                                                .ToArray();

            UnityEngine.Object[] objects = variables.Select(v => (UnityEngine.Object)v.value)
                                                            .Concat(values.Select(v => v.Value))
                                                            .ToArray();

#if UNITY_2023_1_OR_NEWER
            scriptImporter.SetIcon(classAsset.icon);
#endif
            scriptImporter.SetDefaultReferences(names, objects);
            if (!classAsset.lastCompiledNames.Contains(classAsset.GetFullTypeName()))
                classAsset.lastCompiledNames.Add(classAsset.GetFullTypeName());
        }
    }
}
