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

        public string AddLocalNameInScope(string name)
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
                return newName;
            }
            else
            {
                NewScope();
                return AddLocalNameInScope(name);
            }
        }

        public bool ContainsNameInAnyScope(string name)
        {
            return scopeNames.Any(scope => scope.scopeNames.Contains(name));
        }


        public ControlGenerationData() { }
        public ControlGenerationData(ControlGenerationData data)
        {
            returns = data.returns ?? typeof(Void);
            mustBreak = data.mustBreak;
            hasBroke = data.hasBroke;
            mustReturn = data.mustReturn;
            hasReturned = data.hasReturned;
            localNames = data.localNames;
            scopeNames = data.scopeNames;
        }

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