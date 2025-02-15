using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Void = Unity.VisualScripting.Community.Libraries.CSharp.Void;

namespace Unity.VisualScripting.Community
{
    public sealed class ControlGenerationData
    {
        public Type returns { get; set; } = typeof(void);
        public bool mustBreak { get; set; }
        public bool hasBroke { get; set; }
        public bool mustReturn { get; set; }
        public bool hasReturned { get; set; }
        
        private readonly List<string> localNames = new();
        private readonly Stack<GeneratorScope> scopes = new();
        private readonly Stack<GeneratorScope> preservedScopes = new();
        private readonly Stack<(Type type, bool isMet)> expectedTypes = new();
        public readonly Dictionary<object, object> generatorData = new();
        private readonly Dictionary<Unit, UnitSymbol> unitSymbols = new();
        
        private int scopeIdCounter;
        private GraphPointer graphPointer;
        public GameObject gameObject { get; set; }
        public Type ScriptType { get; set; } = typeof(object);

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
            if (string.IsNullOrEmpty(name)) return false;
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
            if (!unitSymbols.ContainsKey(unit))
            {
                unitSymbols.Add(unit, new UnitSymbol(unit, Type, CodeRepresentation, Metadata));
            }
        }

        public bool TryGetSymbol(Unit unit, out UnitSymbol symbol)
        {
            return unitSymbols.TryGetValue(unit, out symbol);
        }

        public void SetSymbolType(Unit unit, Type type)
        {
            if (unitSymbols.TryGetValue(unit, out var symbol))
            {
                symbol.Type = type;
            }
            else
                throw new MissingReferenceException($"No symbol found for {unit}");
        }

        public bool TryGetGameObject(out GameObject gameObject)
        {
            if (this.gameObject != null)
            {
                gameObject = this.gameObject;
                return true;
            }
            gameObject = null;
            return false;
        }

        public bool TryGetGraphPointer(out GraphPointer graphPointer)
        {
            if (this.graphPointer != null)
            {
                graphPointer = this.graphPointer;
                return true;
            }
            graphPointer = null;
            return false;
        }

        public void SetGraphPointer(GraphReference graphReference)
        {
            if(gameObject == null && graphReference != null && graphReference.gameObject == null)
            {
                var target = SceneManager.GetActiveScene().GetRootGameObjects()[0] ?? new GameObject("C# Preview Placeholder");
                typeof(GraphPointer).GetProperty("gameObject").SetValue(graphReference, target);
            }
            graphPointer = graphReference;
        }

        public ControlGenerationData(GraphPointer graphPointer)
        {
            this.graphPointer = graphPointer;
        }

        public ControlGenerationData(ControlGenerationData data)
        {
            returns = data.returns ?? typeof(Void);
            mustBreak = data.mustBreak;
            hasBroke = data.hasBroke;
            mustReturn = data.mustReturn;
            hasReturned = data.hasReturned;
            localNames = new List<string>(data.localNames);
            scopes = new Stack<GeneratorScope>(data.scopes.Select(scope => new GeneratorScope(scope.Id, new Dictionary<string, Type>(scope.scopeVariables), new Dictionary<string, string>(scope.nameMapping), PeekScope()?.ParentScope)));
            expectedTypes = data.expectedTypes;
            ScriptType = data.ScriptType;
            gameObject = data.gameObject;
            graphPointer = data.graphPointer;
        }

        public sealed class GeneratorScope
        {
            public string Id { get; private set; } = string.Empty;
            public Dictionary<string, Type> scopeVariables { get; private set; }
            public Dictionary<string, string> nameMapping { get; private set; }
            public GeneratorScope ParentScope { get; private set; }

            public GeneratorScope(string id, Dictionary<string, Type> scopeVariables, Dictionary<string, string> nameMapping, GeneratorScope parentScope)
            {
                Id = id;
                this.scopeVariables = scopeVariables;
                this.nameMapping = nameMapping;
                ParentScope = parentScope;
            }
        }
    }
}