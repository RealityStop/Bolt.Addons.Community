﻿using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using System;
using System.Collections.Generic;
using Bolt.Addons.Integrations.Continuum.Humility;
using Bolt.Addons.Integrations.Continuum.CSharp;
using System.Linq;

namespace Bolt.Addons.Community.Code.Editor
{
    [CustomEditor(typeof(ActionAsset))]
    public class ActionAssetEditor : CodeAssetEditor<ActionAsset, ActionAssetGenerator>
    {
        private Metadata type;
        private Metadata generics;
        private List<Type> types;
        private List<Type> allTypes;

        private void OnEnable()
        {
            allTypes = typeof(object).Get().Derived().Where((type) => { return type.BaseType != null; }).ToList();
            types = typeof(object).Get().Derived().Where((type) => { return type.IsSubclassOf(typeof(Delegate)) && type.GetMethod("Invoke").ReturnType == typeof(void); }).ToList();
        }

        protected override void Cache()
        {
            if (type == null)
            {
                type = Metadata.FromProperty(serializedObject.FindProperty("type"))["type"];
                generics = Metadata.FromProperty(serializedObject.FindProperty("generics"));
                hidden = true;
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
                        LudiqGUI.FuzzyDropdown(lastRect, new TypeOptionTree(types), type.value, (val) =>
                        {
                            generics.value = new List<GenericDeclaration>();

                            if (type.value != val)
                            {
                                type.value = val;

                                if (((Type)type.value).IsGenericTypeDefinition)
                                {
                                    var generic = ((Type)type.value)?.GetGenericTypeDefinition();
                                    var _generics = generic?.GetGenericArguments();

                                    for (int i = 0; i < _generics.Length; i++)
                                    {
                                        var declaration = new GenericDeclaration();
                                        declaration.name = _generics[i].Name;
                                        declaration.constraint.type = _generics[i];
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
                                    LudiqGUI.FuzzyDropdown(lastRect, new TypeOptionTree(allTypes.Where((t) =>
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
