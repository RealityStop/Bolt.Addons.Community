using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseMatchHandler : IMatchHandler
    {
        public abstract string Name { get; }
        private bool _isEnabled = true;
        public virtual bool IsEnabled => _isEnabled;

        public abstract Type SupportedType { get; }

        public virtual void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }
        public abstract bool CanHandle(IGraphElement element);
        public abstract MatchObject HandleMatch(IGraphElement element, string query, NodeFinderWindow.SearchMode searchMode);
    }
}