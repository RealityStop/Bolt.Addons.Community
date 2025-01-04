using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public sealed class ForEachSurroundCommand : SurroundCommand
    {
        private ForEach unit = new ForEach();
        public override Unit SurroundUnit => unit;

        public override ControlOutput surroundSource => unit.body;

        public override ControlOutput surroundExit => unit.exit;

        public override ControlInput unitEnterPort => unit.enter;

        public override IUnitPort autoConnectPort => unit.collection;

        public override string DisplayName => "For Each Loop";

        public override bool SequenceExit => false;
    } 
}
