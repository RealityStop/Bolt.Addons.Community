using System;

namespace Unity.VisualScripting.Community
{
    public interface IMatchHandler
    {
        string Name { get; }
        bool IsEnabled { get; }
        Type SupportedType { get; }
        void SetEnabled(bool enabled);
        bool CanHandle(IGraphElement element);
        MatchObject HandleMatch(IGraphElement element, string query, NodeFinderWindow.SearchMode searchMode);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MatchHandlerAttribute : Attribute { }
}