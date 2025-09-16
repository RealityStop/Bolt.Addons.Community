#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [MatchHandler]
    public class StickyNoteMatchHandler : BaseMatchHandler
    {
        public override string Name => "StickyNote";

        public override Type SupportedType => typeof(StickyNote);

        public override bool CanHandle(IGraphElement element)
        {
            return element is StickyNote;
        }

        public override MatchObject HandleMatch(IGraphElement element, string pattern, NodeFinderWindow.SearchMode searchMode)
        {
            if (element is StickyNote note)
            {
                var name = GetStickyNoteFullName(note);
                if (SearchUtility.SearchMatches(pattern, name, searchMode))
                {
                    var matchRecord = new MatchObject(note, name);
                    return matchRecord;
                }
            }
            return null;
        }

        private string GetStickyNoteFullName(StickyNote note)
        {
            if (!string.IsNullOrEmpty(note.title) && !string.IsNullOrEmpty(note.body))
            {
                return note.title + "." + note.body;
            }
            else if (!string.IsNullOrEmpty(note.title))
            {
                return note.title;
            }
            else if (!string.IsNullOrEmpty(note.body))
            {
                return note.body;
            }
            return "Empty StickyNote";
        }

    }
}
#endif