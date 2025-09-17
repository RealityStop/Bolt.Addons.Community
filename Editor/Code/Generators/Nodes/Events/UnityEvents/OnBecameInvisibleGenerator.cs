using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnBecameInvisible))]
    public class OnBecameInvisibleGenerator : UnityMethodGenerator<OnBecameInvisible, EmptyEventArgs>
    {
        public override List<ValueOutput> OutputValues => new();

        public override List<TypeParam> Parameters => new();

        public OnBecameInvisibleGenerator(Unit unit) : base(unit) { }
    }
}