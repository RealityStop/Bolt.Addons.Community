using System;

namespace Unity.VisualScripting.Community
{
    [MatchHandler]
    public class ErrorMatchHandler : UnitBaseMatchHandler
    {
        // Using the ErrorMatchHandler to avoid conflicts with UnitMatchHandler.
        // Which should be fine since this is handled manually
        public override Type SupportedType => typeof(ErrorMatchHandler);
        public override string Name => "Error";
        public GraphPointer graphPointer;
        public override bool CanHandle(IGraphElement element)
        {
            return graphPointer != null && element is Unit unit && IsErrorUnit(unit);
        }
        private bool IsErrorUnit(Unit unit)
        {
            if (unit.GetException(graphPointer) != null)
                return true;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (unit is MissingType)
                return true;
#endif
            return false;
        }
        public override MatchObject HandleMatch(IGraphElement element, string pattern, NodeFinderWindow.SearchMode searchMode)
        {
            if (element is Unit unit)
            {
                if (IsErrorUnit(unit))
                {
                    var name = GetUnitFullName(unit);
                    var matchRecord = new MatchObject(unit, name) { IsErrorUnit = true };
                    return matchRecord;
                }
            }
            return null;
        }

        private string GetUnitFullName(Unit unit)
        {
            var typeName = SearchUtility.GetSearchName(unit);
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (unit is MissingType missingType)
            {
                typeName = string.IsNullOrEmpty(missingType.formerType) ? "Missing Type" : "Missing Type : " + missingType.formerType;
            }
#endif
            if (unit.GetException(graphPointer) != null)
            {
                typeName += " " + unit.GetException(graphPointer).Message;
            }
            return typeName;
        }
    }
}