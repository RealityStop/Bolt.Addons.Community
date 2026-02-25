using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [TypeIcon(typeof(EventUnit<CustomEventArgs>))]
    [UnitCategory("Events/Community")]
    [RenamedFrom("Lasm.AssetLibrary.Nodes.TriggerAssetCustomEvent")]
    public sealed class TriggerAssetCustomEvent : Unit
    {
        [Serialize]
        private int _count;
        [UnitHeaderInspectable]
        [Inspectable]
        [InspectorLabel("Count")]
        public int count
        {
            get => _count;

            set
            {
                _count = Mathf.Clamp(value, 0, 10);
            }
        }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput asset;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name;
        [DoNotSerialize]
        public List<ValueInput> args = new List<ValueInput>();

        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                var argList = new List<object>();

                for (int i = 0; i < args.Count; i++)
                {
                    argList.Add(flow.GetValue<object>(args[i]));
                }

                GraphReference.New(flow.GetValue<ScriptGraphAsset>(asset), true).TriggerEventHandler<CustomEventArgs>(hook => hook == "Custom", new CustomEventArgs(flow.GetValue<string>(name), argList.ToArray()), parent => true, true);

                return exit;
            });

            asset = ValueInput<ScriptGraphAsset>("asset", (ScriptGraphAsset)null);
            name = ValueInput<string>("name", string.Empty);

            args.Clear();

            for (int i = 0; i < count; i++)
            {
                args.Add(ValueInput<object>($"arg {i.ToString()}"));
                Requirement(args[i], enter);
            }

            exit = ControlOutput("exit");

            Requirement(asset, enter);
            Succession(enter, exit);
        }
    }
}