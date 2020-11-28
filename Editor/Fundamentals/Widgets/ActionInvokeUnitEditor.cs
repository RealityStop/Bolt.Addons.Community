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
    [Editor(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitEditor : UnitEditor
    {
        public ActionInvokeUnitEditor(Metadata metadata) : base(metadata)
        {
        }

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

            GUI.Label(labelRect, "Action");

            var unit = metadata.value as ActionInvokeUnit;
            var buttonLabel = unit._action == null ? "( None Selected )" : unit._action?.GetType().Name.Prettify();
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
                        if (!types[type].IsAbstract && typeof(IAction).IsAssignableFrom(types[type]))
                        {
                            var _type = types[type];
                            menu.AddItem(new GUIContent(types[type].Name.Prettify()), false, () => { unit._action = Activator.CreateInstance(_type as System.Type) as IAction; });
                            unit.Define();
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