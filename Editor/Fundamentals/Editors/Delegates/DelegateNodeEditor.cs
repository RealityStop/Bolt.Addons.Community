using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    public abstract class DelegateNodeEditor<T, TDelegate> : UnitEditor 
        where T : DelegateNode
        where TDelegate : IDelegate
    {
        public DelegateNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected abstract string DefaultName { get; }

        protected override void OnInspectorGUI(Rect position)
        {
            var buttonRect = position;
            buttonRect.height = 20;
            buttonRect.x += 40;

            var labelRect = position;
            labelRect.height = 20;
            labelRect.width = 40;

            var baseRect = position;
            baseRect.y += 24;

            GUI.Label(labelRect, DefaultName);

            var unit = metadata.value as DelegateNode;
            var buttonLabel = unit?._delegate == null ? "( None Selected )" : unit._delegate?.DisplayName;
            buttonRect.width = GUI.skin.label.CalcSize(new GUIContent(buttonLabel)).x + 40;

            if (GUI.Button(buttonRect, buttonLabel))
            {
                GenericMenu menu = new GenericMenu();

                List<Type> result = new List<Type>();
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                for (int assembly = 0; assembly < assemblies.Length; assembly++)
                {
                    Type[] types = assemblies[assembly].GetTypes();

                    for (int type = 0; type < types.Length; type++)
                    {
                        if (!types[type].IsAbstract && typeof(TDelegate).IsAssignableFrom(types[type]))
                        {
                            var _type = types[type];
                            var del = (TDelegate)Activator.CreateInstance(_type as System.Type);
                            menu.AddItem(new GUIContent(del.DisplayName), false, () =>
                            {
                                unit._delegate = del;
                                unit.Define();
                            });
                        }
                    }
                }

                menu.ShowAsContext();
            }

            base.OnInspectorGUI(position);
        }

        protected override float GetInspectorHeight(float width)
        {
            return base.GetInspectorHeight(width) + 24;
        }
    }
}