using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class SnippetControlSourceUnit : SnippetSourceUnit
    {
        [PortLabelHidden]
        [DoNotSerialize]
        public ControlOutput source { get; private set; }

        public override bool isControlRoot { get => true; protected set => base.isControlRoot = value; }

        protected override void Definition()
        {
            source = ControlOutput(nameof(source));
        }
    }
}