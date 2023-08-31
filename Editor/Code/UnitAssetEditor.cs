using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(UnitAsset))]
    public sealed class UnitAssetEditor : MemberTypeAssetEditor<UnitAsset, UnitAssetGenerator, UnitFieldDeclaration, UnitMethodDeclaration, UnitConstructorDeclaration>
    {
        [SerializeField]
        private Type selectedType;
        private Type selectedUnitType;
        Rect lastRect;
        protected override void OnExtendedOptionsGUI()
        {

            var allTypes = Codebase.settingsAssemblies
                .SelectMany(a => a.GetTypes().Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition))
                .ToArray();

            var UnitTypes = Codebase.settingsAssemblies
                .SelectMany(a => a.GetTypes().Where(type => type.IsSubclassOf(typeof(Unit)) || type == typeof(Unit)))
                .ToArray();

            int selectedIndex = Array.IndexOf(allTypes, selectedType);
            int selectedIndexUnit = Array.IndexOf(UnitTypes, selectedUnitType);

            GUIContent buttonContent;
            GUIContent UnitbuttonContent;

            buttonContent = new GUIContent(Target.unitArgs.As().CSharpName(false).RemoveHighlights().RemoveMarkdown());

            UnitbuttonContent = new GUIContent(Target.unitType.As().CSharpName(false).RemoveHighlights().RemoveMarkdown());


            if (GUILayout.Button(UnitbuttonContent))
            {
                lastRect = GUILayoutUtility.GetLastRect();
                LudiqGUI.FuzzyDropdown(lastRect, new TypeOptionTree(UnitTypes), selectedIndex, (index) =>
                {
                    Target.unitType = (Type)index;
                    selectedUnitType = Target.unitType;
                });
            }

            if (Target.unitType.ContainsGenericParameters)
            {
                if (GUILayout.Button(buttonContent))
                {
                    lastRect = GUILayoutUtility.GetLastRect();
                    LudiqGUI.FuzzyDropdown(lastRect, new TypeOptionTree(allTypes), selectedIndex, (index) =>
                    {
                        Target.unitArgs = (Type)index;
                        selectedType = Target.unitArgs;
                    });
                }
            }
        }

        protected override void OptionsGUI()
        {
            OnExtendedOptionsGUI();
        }


        protected override void OnTypeHeaderGUI()
        {
            base.OnTypeHeaderGUI();

            HUMEditor.Horizontal(() =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(3, 3, 3, 3), new RectOffset(1, 1, 1, 1), () =>
                {
                    Target.Namespace = EditorGUILayout.TextField("Namespace", Target.Namespace);
                }, false, true);
            });
        }
    }
}
