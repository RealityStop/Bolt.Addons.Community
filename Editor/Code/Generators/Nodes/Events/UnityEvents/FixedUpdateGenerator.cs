using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(FixedUpdate))]
    public class FixedUpdateGenerator : UnityMethodGenerator<FixedUpdate, EmptyEventArgs>
    {
        public FixedUpdateGenerator(FixedUpdate unit) : base(unit) { }
        public override List<ValueOutput> OutputValues => new List<ValueOutput>();
        public override List<TypeParam> Parameters => new List<TypeParam>();
    }
}
