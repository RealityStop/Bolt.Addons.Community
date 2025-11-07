using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnMouseExit))]
    public class OnMouseExitGenerator : UnityMethodGenerator<OnMouseExit, EmptyEventArgs>
    {
        public OnMouseExitGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override List<TypeParam> Parameters => new List<TypeParam>();
    }
}