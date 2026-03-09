using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public sealed class WhileSurroundCommand : SurroundCommand
    {
        private While unit = new While();
        public override Unit SurroundUnit => unit;

        public override ControlOutput surroundSource => unit.body;

        public override ControlOutput surroundExit => unit.exit;

        public override ControlInput unitEnterPort => unit.enter;

        public override IUnitPort autoConnectPort => unit.condition;

        public override string DisplayName => "While Loop";

        public override bool SequenceExit => false;
    } 
}