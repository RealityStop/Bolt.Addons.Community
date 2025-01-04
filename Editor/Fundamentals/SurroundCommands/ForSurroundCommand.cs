using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public sealed class ForSurroundCommand : SurroundCommand
    {
        private For unit = new For();
        public override Unit SurroundUnit => unit;

        public override ControlOutput surroundSource => unit.body;

        public override ControlOutput surroundExit => unit.exit;

        public override ControlInput unitEnterPort => unit.enter;

        public override IUnitPort autoConnectPort => unit.lastIndex;

        public override string DisplayName => "For Loop";

        public override bool SequenceExit => false;
    } 
}
