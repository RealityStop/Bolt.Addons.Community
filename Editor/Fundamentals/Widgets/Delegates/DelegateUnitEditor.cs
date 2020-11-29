using Ludiq;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    public abstract class DelegateUnitEditor<TDelegate> : UnitEditor where TDelegate : IDelegate
    {
        public DelegateUnitEditor(Metadata metadata) : base(metadata)
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

            var unit = metadata.value as DelegateUnit;
            var buttonLabel = unit._delegate == null ? "( None Selected )" : unit._delegate?.GetType().Name.Prettify();
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
                            menu.AddItem(new GUIContent(types[type].Name.Prettify()), false, () =>
                            {
                                unit._delegate = Activator.CreateInstance(_type as System.Type) as IDelegate;
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