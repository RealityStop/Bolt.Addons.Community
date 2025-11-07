using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnBecameVisible))]
    public class OnBecameVisibleGenerator : UnityMethodGenerator<OnBecameVisible, EmptyEventArgs>
    {
        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnBecameVisibleGenerator(Unit unit) : base(unit) { }
    }
}