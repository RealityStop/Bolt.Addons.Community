using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEditor;
using UnityEngine;
using Lasm.BoltAddons.VariableTags;

namespace Lasm.BoltAddons.VariableTags.Editor
{
    [Widget(typeof(VariableTagUnit))]
    public sealed class VariableTagWidget : UnitWidget<VariableTagUnit>
    {
        //private VariableTagUnitInspector inspector;
        private VariableTagUnitInspector _variableTagUnitInspector;
        private Func<Metadata, VariableTagUnitInspector> _variableTagConstructor;

        protected override NodeColorMix baseColor
        {
            get
            {
                return new NodeColorMix
                {
                    teal = 1f
                };
            }
        }

        public VariableTagWidget(VariableTagUnit unit) : base(unit)
        {
            this._variableTagConstructor = ((Metadata metadata) => new VariableTagUnitInspector(metadata));
            GUILayout.Button("iihiuhih");
        }

    }
}
