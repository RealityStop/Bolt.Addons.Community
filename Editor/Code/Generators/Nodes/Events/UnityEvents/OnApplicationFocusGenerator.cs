using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnApplicationFocus))]
    public class OnApplicationFocusGenerator : UnityMethodGenerator<OnApplicationFocus, EmptyEventArgs>
    {
        public OnApplicationFocusGenerator(Unit unit) : base(unit) { }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(bool), "focus") };
    }
}