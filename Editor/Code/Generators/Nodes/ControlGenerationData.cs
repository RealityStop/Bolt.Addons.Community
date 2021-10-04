using System;
using System.Collections.Generic;

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

            return localName;
        }

        public ControlGenerationData() { }
    }
}