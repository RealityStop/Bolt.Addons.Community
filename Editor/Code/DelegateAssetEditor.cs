using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [CustomEditor(typeof(DelegateAsset))]
    public class DelegateAssetEditor : CodeAssetEditor<DelegateAsset, DelegateAssetGenerator>
    {
        private Metadata type;
        private List<Type> types = new List<Type>();
        private Type[] delegateTypes = new Type[0];

        protected override bool showOptions => false;
        protected override bool showTitle => false;
        protected override bool showCategory => false;

        protected override void OnEnable()
        {
            base.OnEnable();

            types = typeof(object).Get().Derived().Where((type) => { return !type.IsGenericType && !type.IsAbstract; }).ToList();
            delegateTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => typeof(Delegate).IsAssignableFrom(type))).ToArray();

            type ??= Metadata.FromProperty(serializedObject.FindProperty("type"))["type"];
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
            });
        }

        private string GetCompoundTitle()
        {
            return (Target.type.type == typeof(Action) ? "_Generic" : string.Empty) + Target.type.type.HumanName(true).LegalMemberName();
        }
    }
}