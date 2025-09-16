using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnTransformChildrenChanged))]
    public class OnTransformChildrenChangedGenerator : UnityMethodGenerator<OnTransformChildrenChanged, EmptyEventArgs>
    {
        public OnTransformChildrenChangedGenerator(Unit unit) : base(unit)
        {
        }

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override List<TypeParam> Parameters => new List<TypeParam>();
    }
}