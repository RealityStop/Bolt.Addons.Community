using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Unit))]
    public class NodeGenerator : Decorator<NodeGenerator, NodeGeneratorAttribute, Unit>
    {
        public Unit unit;

        public bool hasNamespace;

        public string NameSpace = "";

        public string UniqueID = "";

        #region Subgraphs
        public List<ControlOutput> connectedGraphOutputs = new List<ControlOutput>();
        public List<ValueInput> connectedValueInputs = new List<ValueInput>();
        #endregion

        public NodeGenerator(Unit unit) { this.unit = unit; }

        public virtual string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return GetNextValueUnit(input);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(true, true, true, "", false);
            }
            else
            {
                return $"/* \"{input.key} Requires Input\" */";
            }
        }

        public virtual string GenerateValue(ValueOutput output) { return $"/* Port '{output.key}' of '{output.unit.GetType().Name}' Missing Generator. */"; }

        public virtual string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return CodeUtility.MakeSelectable(unit, CodeBuilder.Indent(indent) + $"/* Port '{input.key}' of '{input.unit.GetType().Name}' Missing Generator. */");
        }

        public bool ShouldCast(ValueInput input, bool ignoreInputType = false)
        {
            if (input.hasValidConnection)
            {
                if(!ignoreInputType)
                {
                    return input.connection.source.type != input.type && input.connection.source.type == typeof(object) && input.type != typeof(object);
                }
                else
                {
                    return input.connection.source.type == typeof(object);
                }
            }

            return false;
        }

        public string GetNextUnit(ControlOutput controlOutput, ControlGenerationData data, int indent)
        {
            return controlOutput.hasValidConnection ? (controlOutput.connection.destination.unit as Unit).GenerateControl(controlOutput.connection.destination, data, indent) : string.Empty;
        }
        public string GetNextValueUnit(ValueInput valueInput, bool MakeSelectable = true)
        {
            return valueInput.hasValidConnection ? MakeSelectable ? CodeUtility.MakeSelectable(unit, (valueInput.connection.source.unit as Unit).GenerateValue(valueInput.connection.source)) : (valueInput.connection.source.unit as Unit).GenerateValue(valueInput.connection.source) : string.Empty;
        }
    }

    public class NodeGenerator<TUnit> : NodeGenerator where TUnit : Unit
    {
        public TUnit Unit;

        public NodeGenerator(Unit unit) : base(unit) { this.unit = unit; Unit = (TUnit)unit; }
    }
}

public static class CodeUtility
{
    private const string UniqueFormat = "[CommunityAddonsCodeSelectable({0})]{1}[CommunityAddonsCodeSelectableEnd({0})]";

    public static string HighlightCode(string code, string unitId)
    {
        var pattern = $@"\[CommunityAddonsCodeSelectable\({unitId}\)\](.*?)(\[CommunityAddonsCodeSelectableEnd\({unitId}\)\])";

        var highlightedCode = Regex.Replace(code, pattern, "<b class='highlight'>$1</b>", RegexOptions.Singleline);

        // Removing the end tag after highlighting
        highlightedCode = Regex.Replace(highlightedCode, @"\[CommunityAddonsCodeSelectableEnd\(" + Regex.Escape(unitId) + @"\)\]", "");

        return highlightedCode;
    }

    private static string AppendUniqueString(string code, string unit)
    {
        return string.Format(UniqueFormat, unit, code);
    }

    public static string RemoveAllSelectableTags(string multilineCode)
    {
        var pattern = @"\[CommunityAddonsCodeSelectable([^\]]*)\](.*?)(\[CommunityAddonsCodeSelectableEnd\([^\]]*\)\])";
        var pattern2 = @"\[CommunityAddonsCodeSelectable([^\]]*)\](.*?)";
        var strippedCode = Regex.Replace(multilineCode, pattern, "$2", RegexOptions.Singleline);
        strippedCode = Regex.Replace(strippedCode, pattern2, "");
        return strippedCode;
    }

    // Generates code with selectable tags for a unit
    public static string MakeSelectable(Unit unit, string code)
    {
        return AppendUniqueString(code, unit.ToString());
    }

    public static string RemoveCustomHighlights(string highlightedCode)
    {
        var pattern = @"<b class='highlight'>(.*?)<\/b>";
        var cleanedCode = Regex.Replace(highlightedCode, pattern, "$1", RegexOptions.Singleline);
        return cleanedCode;
    }
}
