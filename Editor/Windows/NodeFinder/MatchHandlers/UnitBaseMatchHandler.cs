using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    public abstract class UnitBaseMatchHandler : BaseMatchHandler
    {

        public override Type SupportedType => typeof(Unit);

        public override bool CanHandle(IGraphElement element)
        {
            return element is Unit;
        }

        public override MatchObject HandleMatch(IGraphElement element, string pattern, NodeFinderWindow.SearchMode searchMode)
        {
            if (element is Unit unit)
            {
                var displayName = SearchUtility.GetSearchName(unit);

                if (SearchUtility.SearchMatches(pattern, displayName, searchMode, unit))
                {
                    var matchRecord = new MatchObject(unit, displayName);
                    return matchRecord;
                }
            }
            return null;
        }
    }
}