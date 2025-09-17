
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnDisable))]
    public class OnDisableGenerator : UnityMethodGenerator<OnDisable, EmptyEventArgs>
    {
        public OnDisableGenerator(OnDisable unit) : base(unit) { }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();
        public override List<TypeParam> Parameters => new List<TypeParam>();
    }
}
