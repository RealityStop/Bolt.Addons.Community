using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;
using Void = Unity.VisualScripting.Community.Libraries.CSharp.Void;

namespace Unity.VisualScripting.Community
{
    public sealed class ControlGenerationData
    {
        public Type returns;
        public bool mustBreak;
        public bool hasBroke;
        public bool mustReturn;
        public bool hasReturned;
        public List<string> localNames = new List<string>();
        public Stack<GeneratorScope> scopes = new Stack<GeneratorScope>();
        public Stack<GeneratorScope> preservedScopes = new Stack<GeneratorScope>();
        public Type expectedType;
        private int scopeIdCounter = 0;

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

                while (ContainsNameInAnyScope(name + count))
                {
                    count++;
                }
                newName += count;

                scope.scopeNames.Add(newName, type);
                if (!scope.nameMapping.TryAdd(name, newName))
                {
                    scope.nameMapping[name] = newName;
                }
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
            return scopes.Any(scope => scope.scopeNames.ContainsKey(name)) || preservedScopes.Any(scope => scope.scopeNames.ContainsKey(name));
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
                if (scope.scopeNames.TryGetValue(name, out var type))
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

        public void SetVariableType(string name, Type type)
        {
            foreach (var scope in scopes)
            {
                if (scope.scopeNames.ContainsKey(name))
                {
                    scope.scopeNames[name] = type;
                    return;
                }
            }
        }

        public void RenameVariable(string oldName, string newName)
        {
            var scope = PeekScope();
            if (scope.scopeNames.ContainsKey(oldName))
            {
                var type = scope.scopeNames[oldName];
                scope.scopeNames.Remove(oldName);
                scope.scopeNames[newName] = type;

                if (scope.nameMapping.ContainsKey(oldName))
                {
                    scope.nameMapping.Remove(oldName);
                    scope.nameMapping.Add(newName, newName);
                }
            }
        }

        public void Reset()
        {
            returns = typeof(Void);
            mustBreak = false;
            hasBroke = false;
            mustReturn = false;
            hasReturned = false;
            localNames.Clear();
            scopes.Clear();
            preservedScopes.Clear();
        }

        public void RemoveVariable(string name)
        {
            foreach (var _scope in scopes)
            {
                if (_scope.scopeNames.ContainsKey(name))
                    _scope.scopeNames.Remove(name);
                if (_scope.nameMapping.ContainsKey(name))
                    _scope.nameMapping.Remove(name);
            }
        }

        public GeneratorScope CloneCurrentScope()
        {
            var currentScope = PeekScope();
            return new GeneratorScope(currentScope.id,
                new Dictionary<string, Type>(currentScope.scopeNames),
                new Dictionary<string, string>(currentScope.nameMapping), currentScope.ParentScope);
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
            scopes = new Stack<GeneratorScope>(data.scopes.Select(scope => new GeneratorScope(scope.id, new Dictionary<string, Type>(scope.scopeNames), new Dictionary<string, string>(scope.nameMapping), PeekScope()?.ParentScope)));
        }

        public class GeneratorScope
        {
            public string id { get; private set; } = "";
            public Dictionary<string, Type> scopeNames { get; private set; }
            public Dictionary<string, string> nameMapping { get; private set; }
            public GeneratorScope ParentScope { get; private set; }

            public GeneratorScope(string id, Dictionary<string, Type> scopeNames, Dictionary<string, string> nameMapping, GeneratorScope ParentScope)
            {
                this.id = id;
                this.scopeNames = scopeNames;
                this.nameMapping = nameMapping;
                this.ParentScope = ParentScope;
            }
        }
    }
}