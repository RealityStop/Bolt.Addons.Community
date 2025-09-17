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

                if (SearchUtility.SearchMatches(pattern, displayName, searchMode, out var subMatches, unit))
                {
                    var matchRecord = new MatchObject(unit, displayName);
                    matchRecord.SubMatches = CreateChildrenRecursive(subMatches);
                    return matchRecord;
                }
            }
            return null;
        }

        private List<MatchObject> CreateChildrenRecursive(List<SearchUtility.MatchNode> matches)
        {
            var list = ListPool<MatchObject>.New();
            foreach (var match in matches)
            {
                if (match.Unit == null) continue;
                var matchObject = new MatchObject(match.Unit, SearchUtility.GetSearchName(match.Unit));
                matchObject.SubMatches = CreateChildrenRecursive(match.Children);
                list.Add(matchObject);
            }
            return list;
        }
    }
}