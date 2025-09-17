using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnApplicationResume))]
    public class OnApplicationResumeGenerator : UnityMethodGenerator<OnApplicationResume, EmptyEventArgs>
    {
        public OnApplicationResumeGenerator(Unit unit) : base(unit) { }

        public override List<ValueOutput> OutputValues => new();

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(bool), "paused") };
    }
}