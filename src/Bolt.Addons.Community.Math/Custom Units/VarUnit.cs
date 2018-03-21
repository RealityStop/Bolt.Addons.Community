using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Math.Custom_Units
{
    public abstract class VarUnit : Unit, IUnifiedVariableUnit
    {
        protected VarUnit() : base() { }

        /// <summary>
        /// The kind of variable.
        /// </summary>
        [Serialize, Inspectable, UnitHeaderInspectable]
        public VariableKind kind { get; set; }

        /// <summary>
        /// The name of the variable.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name { get; private set; }

        /// <summary>
        /// The source of the variable.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput @object { get; private set; }

        protected override void Definition()
        {
            name = ValueInput(nameof(name), string.Empty);

            if (kind == VariableKind.Object)
            {
                @object = ValueInput<GameObject>(nameof(@object), null).NullMeansSelf();
            }
        }
    }
}