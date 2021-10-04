using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(InterfaceAsset))]
    public class InterfaceAssetEditor : CodeAssetEditor<InterfaceAsset, InterfaceAssetGenerator>
    {
        private Metadata variables, methods;
        private SerializedProperty variablesProp, methodsProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (variables == null)
            {
                variables = Metadata.FromProperty(serializedObject.FindProperty("variables"));
                variablesProp = serializedObject.FindProperty("variables");
            }

            if (methods == null)
            {
                methods = Metadata.FromProperty(serializedObject.FindProperty("methods"));
                methodsProp = serializedObject.FindProperty("methods");
            }

            shouldUpdate = true;
        }

        protected override void OnTypeHeaderGUI()
        {
            HUMEditor.Horizontal(() =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(7, 7, 7, 7), new RectOffset(1, 1, 1, 1), () =>
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

        protected override void BeforePreview()
        {
            Variables();
            GUILayout.Space(4);
            Methods();
        }

        private void Variables()
        {
            Target.propertiesOpen = HUMEditor.Foldout(Target.propertiesOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
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
            Target.methodsOpen = HUMEditor.Foldout(Target.methodsOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () =>
            {
                HUMEditor.Image(PathUtil.Load("method_16", CommunityEditorPath.Code).Single(), 16, 16, new RectOffset(), new RectOffset(4, 8, 4, 4));
                GUILayout.Label("Methods");
            }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    var listOfMethods = methods.value as List<InterfaceMethodItem>;

                    for (int i = 0; i < listOfMethods.Count; i++)
                    {
                        var index = i;
                        listOfMethods[index].opened = HUMEditor.Foldout(listOfMethods[index].opened, HUMEditorColor.DefaultEditorBackground.Darken(0.15f), Color.black, 1, () =>
                        {
                            HUMEditor.Changed(() =>
                            {
                                listOfMethods[index].name = GUILayout.TextField(listOfMethods[index].name);
                            }, () =>
                            {
                                listOfMethods[index].name = listOfMethods[index].name.LegalMemberName();
                                shouldUpdate = true;
                            });

                            if (GUILayout.Button("...", GUILayout.Width(19)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, (obj) =>
                                {
                                    methods.Remove(obj as InterfaceMethodItem);
                                    shouldUpdate = true;
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
                                    }
                                });

                            }, true, false);
                        });

                        GUILayout.Space(4);
                    }

                    if (GUILayout.Button("+ Add Methods"))
                    {
                        listOfMethods.Add(new InterfaceMethodItem());
                    }
                });
            });
        }
    }
}
