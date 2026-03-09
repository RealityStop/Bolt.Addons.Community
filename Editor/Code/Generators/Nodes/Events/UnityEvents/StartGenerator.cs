using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Start))]
    public class StartGenerator : UnityMethodGenerator<Start, EmptyEventArgs>
    {
        public override List<ValueOutput> OutputValues => new List<ValueOutput>();
        public override List<TypeParam> Parameters => new List<TypeParam>();
        public StartGenerator(Start unit) : base(unit) { }
    }
}
