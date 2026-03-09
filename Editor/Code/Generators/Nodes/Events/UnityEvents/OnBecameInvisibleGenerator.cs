using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnBecameInvisible))]
    public class OnBecameInvisibleGenerator : UnityMethodGenerator<OnBecameInvisible, EmptyEventArgs>
    {
        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnBecameInvisibleGenerator(Unit unit) : base(unit) { }
    }
}