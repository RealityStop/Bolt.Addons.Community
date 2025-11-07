using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [MatchHandler]
    public class GroupMatchHandler : BaseMatchHandler
    {

        public override Type SupportedType => typeof(GraphGroup);

        public override string Name => "Group";
        public override bool CanHandle(IGraphElement element)
        {
            return element is GraphGroup;
        }

        public override MatchObject HandleMatch(IGraphElement element, string pattern, NodeFinderWindow.SearchMode searchMode)
        {
            if (element is GraphGroup group)
            {
                var name = SearchUtility.GetSearchName(group);
                if (SearchUtility.SearchMatches(pattern, name, searchMode, out _))
                {
                    var matchRecord = new MatchObject(group, name);
                    return matchRecord;
                }
            }
            return null;
        }
    }
}