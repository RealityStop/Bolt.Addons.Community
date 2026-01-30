using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community.CSharp
{
    public sealed class ControlGenerationData
    {
        public readonly SymbolTable Symbols = new SymbolTable();
        public readonly ExpectedTypeContext ExpectedTypes = new ExpectedTypeContext();
        private readonly Stack<GeneratorScope> scopes = new Stack<GeneratorScope>();
        private readonly Stack<GeneratorScope> preservedScopes = new Stack<GeneratorScope>();

        /// <summary>
        /// Store any extra info
        /// </summary>
        public Dictionary<object, object> scopeGeneratorData { get => PeekScope().generatorData; }
        public Dictionary<object, object> globalGeneratorData = new Dictionary<object, object>();

        public bool isDisposed { get; private set; } = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                scopes.Clear();
                preservedScopes.Clear();
                ExpectedTypes.Clear();
                globalGeneratorData.Clear();
                Symbols.Clear();
            }
        }

        private GraphPointer graphPointer;
        public GameObject gameObject { get; set; }
        public Type ScriptType { get; private set; } = typeof(object);
        #region Expected Types

        public Type GetExpectedType()
        {
            return ExpectedTypes.Current;
        }

        public bool IsCurrentExpectedTypeMet()
        {
            return ExpectedTypes.IsSatisfied;
        }

        public void MarkExpectedTypeMet(Type resolvedAs = null)
        {
            ExpectedTypes.MarkSatisfied(resolvedAs);
        }

        public IDisposable Expect(Type type)
        {
            return ExpectedTypes.Expect(type, out _);
        }

        public IDisposable Expect(Type type, out ExpectedTypeResult result)
        {
            return ExpectedTypes.Expect(type, out result);
        }

        #endregion

        public string AddLocalNameInScope(string name, Type type = null, bool checkAnscestorsOnly = false)
        {
            if (scopes.Count > 0)
            {
                string newName = name;
                int count = 0;

                var scope = PeekScope();

                while (checkAnscestorsOnly ? ContainsNameInAncestorScope(newName) : ContainsNameInAnyScope(newName))
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
                return AddLocalNameInScope(name, type, checkAnscestorsOnly);
            }
        }
        #region Scope Management

        Dictionary<string, int> methodIndex = new Dictionary<string, int>();

        public string AddMethodName(string originalName)
        {
            if (!methodIndex.TryGetValue(originalName, out var index))
            {
                methodIndex[originalName] = 0;
            }
            else
            {
                index++;
                methodIndex[originalName] = index;
                originalName += index;
            }
            return originalName;
        }

        int methodId;
        public void EnterMethod()
        {
            methodId++;
            NewScope();
        }
        public void ExitMethod()
        {
            GeneratorScope result;
            while (preservedScopes.Count > 0 && (result = preservedScopes.Peek()).methodId == methodId)
            {
                preservedScopes.Pop();
            }
            methodId--;
            ExitScope(true);
        }
        public void NewScope()
        {
            var parent = PeekScope();
            var newScope = new GeneratorScope(parent, methodId);

            if (parent != null)
            {
                newScope.MustBreak = parent.MustBreak;
                newScope.Returns = parent.Returns;
            }

            scopes.Push(newScope);
        }


        public void ExitScope(bool exitingMethod = false)
        {
            var exitingScope = scopes.Pop();
            if (!exitingMethod)
                preservedScopes.Push(exitingScope);

            if (exitingScope.ParentScope != null)
            {
                exitingScope.ParentScope.HasReturned = exitingScope.HasReturned;
                exitingScope.ParentScope.HasBroke = exitingScope.HasBroke;
            }
        }

        private GeneratorScope PeekScope()
        {
            if (scopes.Count > 0)
                return scopes.Peek();
            else
                return null;
        }

        public Type Returns => PeekScope()?.Returns ?? typeof(void);
        public bool MustReturn => PeekScope()?.MustReturn ?? false;
        public bool HasReturned => PeekScope()?.HasReturned ?? false;
        public bool MustBreak => PeekScope()?.MustBreak ?? false;
        public bool HasBroke => PeekScope()?.HasBroke ?? false;

        public void SetHasReturned(bool value)
        {
            var scope = PeekScope();
            if (scope != null) scope.HasReturned = value;
        }
        public void SetMustBreak(bool value)
        {
            var scope = PeekScope();
            if (scope != null) scope.MustBreak = value;
        }
        public void SetHasBroke(bool value)
        {
            var scope = PeekScope();
            if (scope != null) scope.HasBroke = value;
            if (value)
                scope.MustBreak = false;
        }
        public void SetReturns(Type type)
        {
            var scope = PeekScope();
            if (scope != null) scope.Returns = type;
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

        public bool ContainsNameInAncestorScope(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (var scope in scopes)
            {
                if (scope.scopeVariables.ContainsKey(name)) return true;
            }
            return false;
        }

        #endregion

        #region Variable Management
        public string GetVariableName(string name, bool errorIfNotFound = false, string error = "")
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

            if (errorIfNotFound)
            {
                return error;
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
        #endregion

        #region Symbol Management
        public void CreateSymbol(Unit unit, Type Type, Dictionary<string, object> Metadata = null)
        {
            Symbols.CreateSymbol(unit, Type, Metadata);
        }

        public bool TryGetSymbol(Unit unit, out UnitSymbol symbol)
        {
            if (unit == null)
            {
                symbol = null;
                return false;
            }
            return Symbols.TryGet(unit, out symbol);
        }

        public void SetSymbolType(Unit unit, Type type)
        {
            if (Symbols.TryGet(unit, out var symbol))
            {
                symbol.Type = type;
            }
            else
                throw new MissingReferenceException($"No symbol found for {unit}");
        }
        #endregion

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
            // This should only happen if this is not a scriptmachine
            // It's set so that Scene variables are predicatable in assets
            // because GraphPointer.scene uses GameObject.scene
            if (gameObject == null && graphReference != null && graphReference.gameObject == null)
            {
                var firstObject = SceneManager.GetActiveScene().GetRootGameObjects()[0];
                var target = firstObject.IsUnityNull() ? new GameObject("C# Preview Placeholder") : firstObject;
                typeof(GraphPointer).GetProperty("gameObject").SetValue(graphReference, target);
            }
            else if (gameObject == null && graphReference != null && graphReference.gameObject != null)
            {
                gameObject = graphReference.gameObject;
            }
            graphPointer = graphReference;
        }

        public ControlGenerationData(Type ScriptType, GraphPointer graphPointer)
        {
            this.ScriptType = ScriptType;
            this.graphPointer = graphPointer;
            if (gameObject == null && graphPointer != null && graphPointer.gameObject != null)
            {
                gameObject = graphPointer.gameObject;
            }
        }

        private sealed class GeneratorScope
        {
            public Dictionary<string, Type> scopeVariables { get; private set; }
            public Dictionary<string, string> nameMapping { get; private set; }
            public GeneratorScope ParentScope { get; private set; }
            public readonly Dictionary<object, object> generatorData = new Dictionary<object, object>();

            public Type Returns { get; set; } = typeof(void);
            public bool MustBreak { get; set; }
            public bool HasBroke { get; set; }
            public bool MustReturn { get => !Returns.Is().Void(); }
            public bool HasReturned { get; set; }

            public int methodId { get; private set; }

            public GeneratorScope(GeneratorScope parentScope, int methodId)
            {
                scopeVariables = new Dictionary<string, Type>();
                nameMapping = new Dictionary<string, string>();
                ParentScope = parentScope;
                this.methodId = methodId;
            }
        }
    }
}