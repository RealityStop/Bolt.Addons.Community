using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

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
        public Stack<GeneratorScope> scopeNames = new Stack<GeneratorScope>();

        public string AddLocalName(string name)
        {
            var localName = name;
            var count = 0;

            while (localNames.Contains(localName))
            {
                localName = name + count;
                count++;
            }

            localNames.Add(localName);

            AddLocalNameInScope(localName);

            return localName;
        }

        public void NewScope()
        {
            scopeNames.Push(new GeneratorScope("", new()));
        }

        public GeneratorScope ExitScope()
        {
            return scopeNames.Pop();
        }

        public GeneratorScope PeekScope()
        {
            return scopeNames.Peek();
        }

        public void AddLocalNameInScope(string name)
        {
            if (scopeNames.Count > 0)
            {
                string newName = name;
                int count = 0;

                var scope = scopeNames.Peek();

                while (ContainsNameInAnyScope(newName))
                {
                    newName = newName + count;
                    count++;
                }

                scope.scopeNames.Add(newName);
            }
            else
            {
                NewScope();
                AddLocalNameInScope(name);
            }
        }

        public bool ContainsNameInAnyScope(string name)
        {
            return scopeNames.Any(scope => scope.scopeNames.Contains(name));
        }


        public ControlGenerationData() { }

        public class GeneratorScope
        {
            public string id = "";

            public List<string> scopeNames;

            public GeneratorScope()
            {

            }

            public GeneratorScope(string id)
            {
                this.id = id;
            }

            public GeneratorScope(string id, List<string> scopeNames)
            {
                this.id = id;
                this.scopeNames = scopeNames;
            }
        }
    }

}