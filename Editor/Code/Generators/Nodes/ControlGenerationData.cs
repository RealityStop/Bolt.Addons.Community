using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Void = Unity.VisualScripting.Community.Libraries.CSharp.Void;

namespace Unity.VisualScripting.Community
{
    public sealed class ControlGenerationData
    {
        public Type returns = typeof(void);
        public bool mustBreak;
        public bool hasBroke;
        public bool mustReturn;
        public bool hasReturned;
        public List<string> localNames = new List<string>();
        public Stack<GeneratorScope> scopes = new Stack<GeneratorScope>();
        private Stack<GeneratorScope> preservedScopes = new Stack<GeneratorScope>();
        private int scopeIdCounter = 0;
        private Stack<(Type type, bool isMet)> expectedTypes = new Stack<(Type type, bool isMet)>();

        private Dictionary<Unit, UnitSymbol> UnitSymbols = new Dictionary<Unit, UnitSymbol>();

        public Type ScriptType = typeof(object);

        public void NewScope()
        {
            scopeIdCounter++;
            string uniqueId = $"scope_{scopeIdCounter}";
            scopes.Push(new GeneratorScope(uniqueId, new Dictionary<string, Type>(), new Dictionary<string, string>(), PeekScope()));
        }

        public GeneratorScope ExitScope()
        {
            var exitingScope = scopes.Pop();
            preservedScopes.Push(exitingScope);
            return exitingScope;
        }

        public Type GetExpectedType()
        {
            if (expectedTypes.Count > 0)
            {
                return expectedTypes.Peek().type;
            }
            else return null;
        }

        public bool IsCurrentExpectedTypeMet()
        {
            if (expectedTypes.Count > 0)
            {
                return expectedTypes.Peek().isMet;
            }
            else return false;
        }

        public bool SetCurrentExpectedTypeMet(bool isMet, Type metAs)
        {
            if (expectedTypes.Count > 0)
            {
                var type = metAs ?? expectedTypes.Peek().type;
                var currentExpectedType = expectedTypes.Pop();
                currentExpectedType.isMet = isMet;
                currentExpectedType.type = type;
                expectedTypes.Push(currentExpectedType);
                return isMet;
            }
            else
            {
                return false;
            }
        }

        public void SetExpectedType(Type type)
        {
            if (type != null)
            {
                expectedTypes.Push((type, false));
            }
        }

        public (Type type, bool isMet) RemoveExpectedType()
        {
            if (expectedTypes.Count > 0)
                return expectedTypes.Pop();
            return (typeof(object), false);
        }

        public GeneratorScope PeekScope()
        {
            if (scopes.Count > 0)
                return scopes.Peek();
            else
                return null;
        }

        public string AddLocalNameInScope(string name, Type type = null)
        {
            if (scopes.Count > 0)
            {
                string newName = name;
                int count = 0;

                var scope = PeekScope();

                while (ContainsNameInAnyScope(newName))
                {
                    count++;
                    newName = name + count;
                }

                scope.scopeVariables.Add(newName, type);
                scope.nameMapping[name] = newName;

                return newName;
            }
            else
            {
                NewScope();
                return AddLocalNameInScope(name, type);
            }
        }

        public bool ContainsNameInAnyScope(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.scopeVariables.ContainsKey(name)) return true;
            }
            foreach (var preservedScope in preservedScopes)
            {
                if (preservedScope.scopeVariables.ContainsKey(name)) return true;
            }
            return false;
        }

        public string GetVariableName(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.nameMapping.TryGetValue(name, out string variableName))
                {
                    return variableName;
                }
            }

            foreach (var preservedScope in preservedScopes)
            {
                if (preservedScope.nameMapping.TryGetValue(name, out string variableName))
                {
                    return variableName;
                }
            }

            return name;
        }


        public Type GetVariableType(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.scopeVariables.TryGetValue(name, out var type))
                {
                    return type;
                }
            }

            return null;
        }

        public bool TryGetVariableType(string name, out Type type)
        {
            if (GetVariableType(name) == null)
            {
                type = null;
                return false;
            }
            type = GetVariableType(name);
            return true;
        }

        public void CreateSymbol(Unit unit, Type Type, string CodeRepresentation, Dictionary<string, object> Metadata = null)
        {
            if (!UnitSymbols.ContainsKey(unit))
            {
                UnitSymbols.Add(unit, new UnitSymbol(unit, Type, CodeRepresentation, Metadata));
            }
        }

        public bool TryGetSymbol(Unit unit, out UnitSymbol symbol)
        {
            return UnitSymbols.TryGetValue(unit, out symbol);
        }

        public void SetSymbolType(Unit unit, Type type)
        {
            if (UnitSymbols.TryGetValue(unit, out var symbol))
            {
                symbol.Type = type;
            }
            else
                throw new MissingReferenceException($"No symbol found for {unit}");
        }

        public ControlGenerationData() { }

        public ControlGenerationData(ControlGenerationData data)
        {
            returns = data.returns ?? typeof(Void);
            mustBreak = data.mustBreak;
            hasBroke = data.hasBroke;
            mustReturn = data.mustReturn;
            hasReturned = data.hasReturned;
            localNames = new List<string>(data.localNames);
            scopes = new Stack<GeneratorScope>(data.scopes.Select(scope => new GeneratorScope(scope.id, new Dictionary<string, Type>(scope.scopeVariables), new Dictionary<string, string>(scope.nameMapping), PeekScope()?.ParentScope)));
            expectedTypes = data.expectedTypes;
            ScriptType = data.ScriptType;
        }

        public class GeneratorScope
        {
            public string id { get; private set; } = "";
            public Dictionary<string, Type> scopeVariables { get; private set; }
            public Dictionary<string, string> nameMapping { get; private set; }
            public GeneratorScope ParentScope { get; private set; }

            public GeneratorScope(string id, Dictionary<string, Type> scopeVariables, Dictionary<string, string> nameMapping, GeneratorScope ParentScope)
            {
                this.id = id;
                this.scopeVariables = scopeVariables;
                this.nameMapping = nameMapping;
                this.ParentScope = ParentScope;
            }
        }
    }
}