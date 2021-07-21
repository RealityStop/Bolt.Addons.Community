using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Bolt.Addons.Libraries.Humility;
using System.Collections.Generic;
using Bolt.Addons.Libraries.CSharp;
using Bolt.Addons.Community.Utility.Editor;

namespace Bolt.Addons.Community.Code.Editor
{
    public abstract class MemberTypeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration> : CodeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator>
        where TMemberTypeAsset : MemberTypeAsset<TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TMemberTypeGenerator : MemberTypeAssetGenerator<TMemberTypeAsset, TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration>
        where TFieldDeclaration : FieldDeclaration
        where TMethodDeclaration : MethodDeclaration
        where TConstructorDeclaration : ConstructorDeclaration
    {
        protected Metadata constructors;
        protected SerializedProperty constructorsProp;

        protected Metadata variables;
        protected SerializedProperty variablesProp;

        protected Metadata methods;
        protected SerializedProperty methodsProp;

        private int constructorsCount;
        private int fieldsCount;
        private int methodsCount;

        private Color boxBackground => HUMColor.Grey(0.15f);

        protected virtual Texture2D DefaultIcon() { return PathUtil.Load("object_32", CommunityEditorPath.Code).Single(); }

        private BoltCore resources;

        protected override void OnEnable()
        {
            base.OnEnable();

            //if (constructors == null)
            //{
            //    constructors = Metadata.FromProperty(serializedObject.FindProperty("constructors"));
            //    constructorsProp = serializedObject.FindProperty("constructors");
            //    hidden = true;
            //}
            if (variables == null || variablesProp == null)
            {
                variables = Metadata.FromProperty(serializedObject.FindProperty("variables"));
                variablesProp = serializedObject.FindProperty("variables");
            }

            if (methods == null || methodsProp == null)
            {
                methods = Metadata.FromProperty(serializedObject.FindProperty("methods"));
                methodsProp = serializedObject.FindProperty("methods");
            }

            if (Target.icon == null) Target.icon = DefaultIcon();

            shouldUpdate = true;
        }

        protected override void OnTypeHeaderGUI()
        {
            HUMEditor.Horizontal(() =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(7,7,7,7), new RectOffset(1,1,1,1), () => 
                {
                    Target.icon = (Texture2D)EditorGUILayout.ObjectField(GUIContent.none, Target.icon, typeof(Texture2D), false, GUILayout.Width(32), GUILayout.Height(32));
                }, false, false);

                GUILayout.Space(2);

                HUMEditor.Vertical(() =>
                {
                    base.OnTypeHeaderGUI();
                });
            });
        }

        protected override void OptionsGUI()
        {
            Target.serialized = EditorGUILayout.ToggleLeft("Serialized", Target.serialized);
            Target.inspectable = EditorGUILayout.ToggleLeft("Inspectable", Target.inspectable);
            Target.includeInSettings = EditorGUILayout.ToggleLeft("Include In Settings", Target.includeInSettings);
            Target.definedEvent = EditorGUILayout.ToggleLeft("Flag for Defined Event Filtering", Target.definedEvent);
            OnExtendedOptionsGUI();
        }

        protected virtual void OnExtendedOptionsGUI()
        {

        }

        protected override void BeforePreview()
        {
            Variables();
            GUILayout.Space(4);
            Methods();
        }

        private void Variables()
        {
            Target.fieldsOpened = HUMEditor.Foldout(Target.fieldsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("variables_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Variables");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    Inspector.BeginBlock(variables, new Rect());
                    LudiqGUI.InspectorLayout(variables, GUIContent.none);
                    if (Inspector.EndBlock(variables))
                    {
                        shouldUpdate = true;
                    }
                });
            });
        }

        private void Methods()
        {
            Target.methodsOpened = HUMEditor.Foldout(Target.methodsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("method_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Methods");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    var listOfMethods = methods.value as List<TMethodDeclaration>;

                    for (int i = 0; i < listOfMethods.Count; i++)
                    {
                        var index = i;
                        listOfMethods[index].opened = HUMEditor.Foldout(listOfMethods[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            HUMEditor.Changed(() =>
                            {
                                listOfMethods[index].methodName = GUILayout.TextField(listOfMethods[index].methodName);
                            }, () =>
                            {
                                listOfMethods[index].name = listOfMethods[index].methodName.LegalMemberName();
                                var funcionUnit = (listOfMethods[index].graph.units[0] as FunctionUnit);
                                funcionUnit.Define();
                                funcionUnit.Describe();
                            });

                            if (GUILayout.Button("Edit", GUILayout.Width(60)))
                            {
                                GraphWindow.OpenActive(listOfMethods[index].GetReference() as GraphReference);
                            }

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    methods.Remove(obj as TMethodDeclaration);
                                }, listOfMethods[index]);

                                if (index > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (obj) =>
                                    {
                                        // To Do
                                    }, listOfMethods[index]);
                                }

                                if (index < methods.Count - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (obj) =>
                                    {
                                        // To Do
                                    }, listOfMethods[index]);
                                }
                                menu.ShowAsContext();
                            }
                        }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMColor.Grey(0.15f), Color.black, new RectOffset(6, 6, 6, 6), new RectOffset(1, 1, 0, 1), () =>
                            {
                                listOfMethods[index].scope = (AccessModifier)EditorGUILayout.EnumPopup("Scope", listOfMethods[index].scope);
                                listOfMethods[index].modifier = (MethodModifier)EditorGUILayout.EnumPopup("Modifier", listOfMethods[index].modifier);
                                Inspector.BeginBlock(methods[index]["returnType"], new Rect());
                                LudiqGUI.InspectorLayout(methods[index]["returnType"], new GUIContent("Returns"));
                                if (Inspector.EndBlock(methods[index]["returnType"]))
                                {
                                    shouldUpdate = true;
                                }

                                GUILayout.Space(4);

                                listOfMethods[index].parametersOpened = HUMEditor.Foldout(listOfMethods[index].parametersOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                                {
                                    GUILayout.Label("Parameters");
                                }, () =>
                                {
                                    var paramMeta = methods[index]["parameters"];
                                    Inspector.BeginBlock(paramMeta, new Rect());
                                    LudiqGUI.InspectorLayout(paramMeta, GUIContent.none);
                                    if (Inspector.EndBlock(paramMeta))
                                    {
                                        shouldUpdate = true;
                                        var funcionUnit = (listOfMethods[index].graph.units[0] as FunctionUnit);
                                        funcionUnit.Define();
                                        funcionUnit.Describe();
                                    }
                                });

                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }

                    if (GUILayout.Button("+ Add Methods"))
                    {
                        var declaration = CreateInstance<TMethodDeclaration>();
                        AssetDatabase.AddObjectToAsset(declaration, Target);
                        listOfMethods.Add(declaration);
                        var functionUnit = new FunctionUnit(FunctionType.Method);
                        functionUnit.declaration = declaration;
                        declaration.graph.units.Add(functionUnit);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    if (methods.Count != methodsCount)
                    {
                        if (Target is ClassAsset)
                        {
                            for (int i = 0; i < methods.Count; i++)
                            {
                                ((TMethodDeclaration)methods[i].value).classAsset = Target as ClassAsset;
                            }
                        }
                        else
                        {
                            if (Target is StructAsset)
                            {
                                for (int i = 0; i < methods.Count; i++)
                                {
                                    ((TMethodDeclaration)methods[i].value).structAsset = Target as StructAsset;
                                }
                            }
                        }

                        methodsCount = methods.Count;
                    }
                });
            });
        }
    }
}
