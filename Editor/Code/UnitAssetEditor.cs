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

        private Type[] allTypes;
        private Type[] UnitTypes;

        protected override void OnEnable()
        {
            base.OnEnable();

            allTypes = Codebase.settingsAssemblies
               .SelectMany(a => a.GetTypes().Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition))
               .ToArray();

            UnitTypes = Codebase.settingsAssemblies
            .SelectMany(a => a.GetTypes().Where(type => type.IsAbstract && typeof(Unit).IsAssignableFrom(type)))
            .ToArray();

            //Reload();
        }

        protected override void OnExtendedOptionsGUI()
        {
            int selectedIndex = Array.IndexOf(allTypes, selectedType);
            int selectedIndexUnit = Array.IndexOf(UnitTypes, selectedUnitType);

            GUIContent buttonContent;
            GUIContent UnitbuttonContent;

            buttonContent = new GUIContent(Target.unitArgs.As().CSharpName(false).RemoveHighlights().RemoveMarkdown(), Target.unitArgs.Icon()?[IconSize.Small]);

            UnitbuttonContent = new GUIContent(Target.unitType.As().CSharpName(false).RemoveHighlights().RemoveMarkdown(), Target.unitType.Icon()?[IconSize.Small]);

            if (GUILayout.Button(UnitbuttonContent))
            {
                lastRect = GUILayoutUtility.GetLastRect();

                LudiqGUI.FuzzyDropdown(lastRect, new TypeOptionTree(UnitTypes), selectedIndex, (index) =>
                {
                    if (Target.unitType != (Type)index)
                    {
                        Target.onValueChanged.Invoke((Type)index);
                    }

                    Target.unitType = (Type)index;
                    selectedUnitType = Target.unitType;
                });
            }

            if (Target.unitType.ContainsGenericParameters)
            {
                lastRect = GUILayoutUtility.GetLastRect();

                if (GUILayout.Button(buttonContent))
                {
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


        //Trying to add a Preview for the custom Node

        //Problem: it adds a new Preview Every time you select a asset or Open the preview.
        //Also Very Buggy does not always work or gets errors.

        /*protected override void AfterCategoryGUI()
          {
              base.AfterCategoryGUI();

              var PreviewNodeButtonContent = new GUIContent("Preview Node");
              if (GUILayout.Button(PreviewNodeButtonContent))
              {
                  var definitionGraph = Target.methods.First(method => method.name == "Definition").graph;
                  var preview = CreateInstance<NodePreview>();
                  AssetDatabase.RemoveObjectFromAsset(preview);
                  preview.hideFlags = HideFlags.HideInHierarchy;
                  AssetDatabase.AddObjectToAsset(preview, Target);

                  var displayUnit = new DisplayUnit(new List<string>(), new List<string>(), new List<string>(), new List<string>());
                  preview.graph.units.Add(displayUnit);
                  var DisplayUnit = preview.graph.units.First(unit => unit is DisplayUnit) as DisplayUnit;
                  DisplayUnit.EnsureDefined();
                  DisplayUnit.controlInputCount = 0;
                  DisplayUnit.controlOutputCount = 0;
                  DisplayUnit.ValueInputCount = 0;
                  DisplayUnit.ValueOutputCount = 0;
                  DisplayUnit.title = Target.title;
                  DisplayUnit.typeIcon = Target.TypeIcon.type;
                  foreach (var Unit in definitionGraph.units)
                  {
                      if (Unit is AssignControlInput)
                      {
                          var _unit = (AssignControlInput)Unit;
                          DisplayUnit.controlInputNames.Add(_unit.GenerateValue(_unit._controlInput).RemoveHighlights().RemoveMarkdown());
                          DisplayUnit.controlInputCount++;
                      }
                      else if (Unit is AssignControlOutput)
                      {
                          var _unit = (AssignControlOutput)Unit;
                          DisplayUnit.controlOutputNames.Add(_unit.GenerateValue(_unit.controlOutput).RemoveHighlights().RemoveMarkdown());
                          DisplayUnit.controlOutputCount++;
                      }
                      else if (Unit is AssignValueInput)
                      {
                          var _unit = (AssignValueInput)Unit;
                          DisplayUnit.ValueInputNames.Add(_unit.GenerateValue(_unit.Input).RemoveHighlights().RemoveMarkdown());
                          DisplayUnit.ValueInputCount++;
                      }
                      else if (Unit is AssignValueOutput)
                      {
                          var _unit = (AssignValueOutput)Unit;
                          DisplayUnit.ValueOutputNames.Add(_unit.GenerateValue(_unit.valueOutput).RemoveHighlights().RemoveMarkdown());
                          DisplayUnit.ValueOutputCount++;
                      }
                  }
                  DisplayUnit.Define();
                  AssetDatabase.SaveAssets();
                  AssetDatabase.Refresh();
                  GraphWindow.OpenActive(preview.GetReference() as GraphReference);
              }
          }*/

        /*public void Reload()
          {
              var definitionGraph = Target.methods.First(method => method.name == "Definition").graph;
              var preview = CreateInstance<NodePreview>();
              AssetDatabase.RemoveObjectFromAsset(preview);
              preview.hideFlags = HideFlags.HideInHierarchy;
              AssetDatabase.AddObjectToAsset(preview, Target);

              var displayUnit = new DisplayUnit(new List<string>(), new List<string>(), new List<string>(), new List<string>());
              preview.graph.units.Add(displayUnit);
              var DisplayUnit = preview.graph.units.First(unit => unit is DisplayUnit) as DisplayUnit;
              DisplayUnit.EnsureDefined();
              DisplayUnit.controlInputCount = 0;
              DisplayUnit.controlOutputCount = 0;
              DisplayUnit.ValueInputCount = 0;
              DisplayUnit.ValueOutputCount = 0;
              DisplayUnit.title = Target.title;
              DisplayUnit.typeIcon = Target.TypeIcon.type;
              foreach (var Unit in definitionGraph.units)
              {
                  if (Unit is AssignControlInput)
                  {
                      var _unit = (AssignControlInput)Unit;
                      DisplayUnit.controlInputNames.Add(_unit.GenerateValue(_unit._controlInput).RemoveHighlights().RemoveMarkdown());
                      DisplayUnit.controlInputCount++;
                  }
                  else if (Unit is AssignControlOutput)
                  {
                      var _unit = (AssignControlOutput)Unit;
                      DisplayUnit.controlOutputNames.Add(_unit.GenerateValue(_unit.controlOutput).RemoveHighlights().RemoveMarkdown());
                      DisplayUnit.controlOutputCount++;
                  }
                  else if (Unit is AssignValueInput)
                  {
                      var _unit = (AssignValueInput)Unit;
                      DisplayUnit.ValueInputNames.Add(_unit.GenerateValue(_unit.Input).RemoveHighlights().RemoveMarkdown());
                      DisplayUnit.ValueInputCount++;
                  }
                  else if (Unit is AssignValueOutput)
                  {
                      var _unit = (AssignValueOutput)Unit;
                      DisplayUnit.ValueOutputNames.Add(_unit.GenerateValue(_unit.valueOutput).RemoveHighlights().RemoveMarkdown());
                      DisplayUnit.ValueOutputCount++;
                  }
              }
              DisplayUnit.Define();
              AssetDatabase.SaveAssets();
              AssetDatabase.Refresh();
          }*/
    }
}
