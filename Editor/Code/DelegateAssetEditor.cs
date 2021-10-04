using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(DelegateAsset))]
    public class DelegateAssetEditor : CodeAssetEditor<DelegateAsset, DelegateAssetGenerator>
    {
        private Metadata type;
        private Metadata generics;
        private List<Type> types = new List<Type>();
        private List<Type> delegateTypes = new List<Type>();

        protected override bool showOptions => false;
        protected override bool showTitle => false;
        protected override bool showCategory => false;

        protected override void OnEnable()
        {
            base.OnEnable();

            types = typeof(object).Get().Derived().Where((type) => { return !type.IsGenericType && !type.IsAbstract; }).ToList();
            delegateTypes = typeof(object).Get().Derived().Where((type) => { return type.IsSubclassOf(typeof(Delegate)); }).ToList();

            if (type == null)
            {
                type = Metadata.FromProperty(serializedObject.FindProperty("type"))["type"];
                generics = Metadata.FromProperty(serializedObject.FindProperty("generics"));
            }
        }

        protected override void AfterCategoryGUI()
        {
            GUILayout.Label(" ", new GUIStyle(GUI.skin.label) { stretchWidth = true });
            var lastRect = GUILayoutUtility.GetLastRect();
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 1, 1), () =>
            {
                var isGeneric = ((Type)type.value).IsGenericType;

                HUMEditor.Horizontal(() =>
                {
                    GUILayout.Label("Delegate", GUILayout.Width(80));
                    if (GUILayout.Button(((Type)type.value)?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown()))
                    {
                        LudiqGUI.FuzzyDropdown(lastRect, new TypeOptionTree(delegateTypes), type.value, (val) =>
                        {
                            generics.value = new List<GenericDeclaration>();

                            if (type.value != val)
                            {
                                type.value = val;

                                Type[] constraints = null;
                                var _type = ((Type)type.value);

                                if (((Type)type.value).IsGenericTypeDefinition)
                                {
                                    var generic = ((Type)type.value)?.GetGenericTypeDefinition();
                                    var _generics = generic?.GetGenericArguments();
                                    if (_type.IsGenericParameter) constraints = ((Type)type.value).GetGenericParameterConstraints();

                                    for (int i = 0; i < _generics.Length; i++)
                                    {
                                        var declaration = new GenericDeclaration();
                                        declaration.name = _generics[i].Name;
                                        if (_type.IsGenericParameter) declaration.constraint.type = constraints[i];
                                        ((List<GenericDeclaration>)generics.value).Add(declaration);
                                    }
                                }
                            }
                        });
                    }
                });

                HUMEditor.Vertical(() =>
                {
                    if (isGeneric)
                    {
                        var gen = ((List<GenericDeclaration>)generics.value);

                        for (int i = 0; i < gen.Count; i++)
                        {
                            var index = i;
                            HUMEditor.Horizontal(() =>
                            {
                                GUILayout.Label(string.IsNullOrEmpty(gen[index].name) ? "T" + index.ToString() : gen[index].name);
                                if (GUILayout.Button(gen[index].type.type?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown()))
                                {
                                    LudiqGUI.FuzzyDropdown(lastRect, new TypeOptionTree(types.Where((t) =>
                                    {
                                        return t.Inherits(gen[index].constraint.type);
                                    })), type.value, (val) =>
                                    {
                                        gen[index].type.type = (Type)val;
                                    });
                                }
                            });
                        }
                    }
                });
            });
        }
    }
}
