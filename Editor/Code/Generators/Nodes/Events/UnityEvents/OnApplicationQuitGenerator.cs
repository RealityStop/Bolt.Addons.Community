using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnApplicationQuit))]
    public class OnApplicationQuitGenerator : UnityMethodGenerator<OnApplicationQuit, EmptyEventArgs>
    {
        public override List<ValueOutput> OutputValues => new();

        public override List<TypeParam> Parameters => new();

        public OnApplicationQuitGenerator(Unit unit) : base(unit) { }
    }
}