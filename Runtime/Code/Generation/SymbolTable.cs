using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class SymbolTable
    {
        private readonly Dictionary<Unit, UnitSymbol> symbols = new Dictionary<Unit, UnitSymbol>();

        public void CreateSymbol(Unit unit, Type type = null, Dictionary<string, object> Metadata = null)
        {
            if (!symbols.ContainsKey(unit))
            {
                symbols.Add(unit, new UnitSymbol(unit, type, Metadata));
            }
        }

        public bool TryGet(Unit unit, out UnitSymbol symbol)
            => symbols.TryGetValue(unit, out symbol);

        public void Clear()
        {
            symbols.Clear();
        }
    }
}