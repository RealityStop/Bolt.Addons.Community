using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnApplicationLostFocus))]
    public class OnApplicationLostFocusGenerator : UnityMethodGenerator<OnApplicationLostFocus, EmptyEventArgs>
    {
        public OnApplicationLostFocusGenerator(Unit unit) : base(unit) { }

        public override List<ValueOutput> OutputValues => new();

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(bool), "focus") };
    }
}