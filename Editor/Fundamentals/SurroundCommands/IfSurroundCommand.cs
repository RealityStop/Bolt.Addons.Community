using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public sealed class IfSurroundCommand : SurroundCommand
    {
        private If unit = new If();
        public override Unit SurroundUnit => unit;

        public override ControlOutput surroundSource => unit.ifTrue;

        public override ControlOutput surroundExit => sequenceUnit.multiOutputs[1];

        public override ControlInput unitEnterPort => unit.enter;

        public override IUnitPort autoConnectPort => unit.condition;

        public override string DisplayName => "If (true)";

        public override bool SequenceExit => true;
    } 
}
