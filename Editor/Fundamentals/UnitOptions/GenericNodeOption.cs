using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(GenericNode))]
    internal sealed class GenericNodeOption : UnitOption<GenericNode>
    {
        public GenericNodeOption() : base() { }

        public GenericNodeOption(GenericNode unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return "typeof " + unit.genericParameter.name;
        }

        public override bool favoritable => false;

        protected override int Order()
        {
            return 0;
        }

        public override string SearchResultLabel(string query)
        {
            return "typeof " + unit.genericParameter.name;
        }

        protected override string Haystack(bool human)
        {
            return "typeof " + unit.genericParameter.name;
        }
    }

}