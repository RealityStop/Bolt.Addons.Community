using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    public abstract class DelegateNodeWidget<TDelegate> : UnitWidget<DelegateNode> 
        where TDelegate : IDelegate
    {
        public DelegateNodeWidget(FlowCanvas canvas, DelegateNode unit) : base(canvas, unit)
        {
        }

        public override bool foregroundRequiresInput => true;

        protected override bool showHeaderAddon => unit._delegate == null;

        protected override float GetHeaderAddonHeight(float width)
        {
            return 20;
        }

        protected override void DrawHeaderAddon()
        {
            var buttonRect = position;
            buttonRect.x += 42;
            buttonRect.y += 22;
            buttonRect.height = 20;

            var buttonLabel = unit._delegate == null ? "( None Selected )" : unit._delegate?.DisplayName;
            buttonRect.width = GUI.skin.label.CalcSize(new GUIContent(buttonLabel)).x + 8;

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
        }

        protected override float GetHeaderAddonWidth()
        {
            return GUI.skin.label.CalcSize(new GUIContent(unit._delegate == null ? "( None Selected )" : unit._delegate.DisplayName)).x + 8;
        }
    }
}