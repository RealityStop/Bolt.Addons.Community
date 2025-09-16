using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [MatchHandler]
    public class StateMatchHandler : BaseMatchHandler
    {
        public override string Name => "State";

        public override Type SupportedType => typeof(State);

        public override bool CanHandle(IGraphElement element)
        {
            return element is State;
        }

        public override MatchObject HandleMatch(IGraphElement element, string pattern, NodeFinderWindow.SearchMode searchMode)
        {
            if (element is State state)
            {
                var displayName = SearchUtility.GetElementDisplayName(state);

                if (SearchUtility.SearchMatches(pattern, displayName, searchMode))
                {
                    var matchRecord = new MatchObject(state, displayName);
                    return matchRecord;
                }
            }
            return null;
        }
    }
}