using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;

[NodeGenerator(typeof(Timer))]
public class TimerGenerator : NodeGenerator<Timer>
{
    public TimerGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        // I cannot think of a way to implement this.
        var output = CodeBuilder.Indent(indent) + "/* Timer unit not supported */\n";
        if (Unit.started.hasValidConnection)
        {
            output += CodeBuilder.Indent(indent) + "//Connected To Timer Started".CommentHighlight() + "\n";
            output += GetNextUnit(Unit.started, data, indent);
        }

        if (Unit.tick.hasValidConnection)
        {
            output += CodeBuilder.Indent(indent) + "//Connected To Timer Tick".CommentHighlight() + "\n";
            output += GetNextUnit(Unit.tick, data, indent);
        }

        if (Unit.completed.hasValidConnection)
        {
            output += CodeBuilder.Indent(indent) + "//Connected To Timer Completed".CommentHighlight() + "\n";
            output += GetNextUnit(Unit.completed, data, indent);
        }
        return output;
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        return "/* Timer unit not supported */";
    }
}