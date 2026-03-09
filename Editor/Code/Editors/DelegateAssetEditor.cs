using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using System.Reflection;

namespace Unity.VisualScripting.Community.CSharp
{
    [CustomEditor(typeof(DelegateAsset))]
    public class DelegateAssetEditor : CodeAssetEditor<DelegateAsset, DelegateAssetGenerator>
    {
        private Metadata type;

        private Type[] delegateTypes = new Type[0];

        protected override bool showOptions => false;
        protected override bool showTitle => false;
        protected override bool showCategory => false;

        static Type[] cachedDelegateTypes;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (cachedDelegateTypes == null)
                cachedDelegateTypes = BuildDelegateTypeCache();

            delegateTypes = cachedDelegateTypes;

            if (type == null)
                type = Metadata.FromProperty(serializedObject.FindProperty("type"))["type"];
        }

        static Type[] BuildDelegateTypeCache()
        {
            List<Type> list = new List<Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int a = 0; a < assemblies.Length; a++)
            {
                Type[] types;

                try
                {
                    types = assemblies[a].GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (type == null)
                        continue;

                    if (typeof(Delegate).IsAssignableFrom(type))
                        list.Add(type);
                }
            }

            return list.ToArray();
        }

        protected override void AfterCategoryGUI()
        {
            HUMEditor.Horizontal(() =>
            {
                GUILayout.Label("Delegate", GUILayout.Width(80));
                var typeValue = (Type)type.value;
                if (GUILayout.Button(new GUIContent(typeValue.As().CSharpName(false, false, false), typeValue.Icon()?[IconSize.Small])))
                {
                    TypeBuilderWindow.ShowWindow(GUILayoutUtility.GetLastRect(), (result) => Target.type = new SystemType(result), Target.type.type, false, delegateTypes, null, (t) =>
                    {
                        shouldUpdate = true;
                        Target.title = GetCompoundTitle();
                    });
                }
            });
        }

        private string GetCompoundTitle()
        {
            return (Target.type.type == typeof(Action) ? "_Generic" : string.Empty) + Target.type.type.HumanName(true).LegalMemberName();
        }
    }
}