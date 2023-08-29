using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/CodeGenerators")]
    public abstract class GeneratedUnit : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Exit;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Enter;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), Logic);
            Exit = ControlOutput(nameof(Exit));
            Succession(Enter, Exit);
        }

        public virtual ControlOutput Logic(Flow flow)
        {
            Debug.LogWarning("This node is only for the code generators to understand what to do it does not work in a normal graph");
            return Exit;
        }

        public virtual string GeneratorLogic(ControlGenerationData data, int indent, NodeGenerator generator)
        {
            return $"/* Create logic Generator */";
        }

        public virtual string GeneratorOutput(NodeGenerator generator)
        {
            return $"/* Create output Generator */";
        }
    }
}

