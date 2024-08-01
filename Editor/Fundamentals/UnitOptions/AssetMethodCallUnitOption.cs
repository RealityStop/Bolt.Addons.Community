using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(AssetMethodCallUnit))]
    public class AssetMethodCallUnitOption : UnitOption<AssetMethodCallUnit>
    {
        public AssetMethodCallUnitOption(AssetMethodCallUnit unit) : base(unit)
        {
            sourceScriptGuids = VisualScripting.LinqUtility.ToHashSet(UnitBase.GetScriptGuids(unit.method.returnType));
        }

        protected override string Label(bool human)
        {
            string[] parameters = unit.method.parameters.Select(param => param.type.CSharpName()).ToArray();
            return GetInvocationType($"({string.Join(", ", parameters)})");
        }

        public override bool favoritable => false;

        protected override int Order()
        {
            return 0;
        }

        public override string SearchResultLabel(string query)
        {
            string[] parameters = unit.method.parameters.Select(param => param.type.CSharpName()).ToArray();
            return GetInvocationType($"({string.Join(", ", parameters)})");
        }

        protected override string Haystack(bool human)
        {
            return $"{unit.method.methodName}{(human ? ": " : ".")}{Label(human)}";
        }

        private string GetInvocationType(string parameters)
        {
            return unit.method.methodName + parameters + (unit.methodType == MethodType.Invoke ? " (Invoke) " : " (Return) ");
        }
    }

}