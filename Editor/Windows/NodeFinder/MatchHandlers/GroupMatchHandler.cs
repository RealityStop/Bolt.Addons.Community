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
                var name = GetGroupFullName(group);
                if (SearchUtility.SearchMatches(pattern, name, searchMode, out _))
                {
                    var matchRecord = new MatchObject(group, name);
                    return matchRecord;
                }
            }
            return null;
        }

        public static string GetGroupFullName(GraphGroup group)
        {
            if (!string.IsNullOrEmpty(group.label) && !string.IsNullOrEmpty(group.comment))
            {
                return group.label + "." + group.comment;
            }
            else if (!string.IsNullOrEmpty(group.label))
            {
                return group.label;
            }
            else if (!string.IsNullOrEmpty(group.comment))
            {
                return group.comment;
            }
            return "Unnamed Graph Group";
        }
    }
}