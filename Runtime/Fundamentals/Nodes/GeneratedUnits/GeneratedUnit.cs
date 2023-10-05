using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitCategory("Community/Code")]
public abstract class GeneratedUnit : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput Exit;

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Enter;

    public string NameSpace;

    protected override void Definition()
    {
        Enter = ControlInput(nameof(Enter), Logic);
        Exit = ControlOutput(nameof(Exit));
        Succession(Enter, Exit);
    }

    public virtual ControlOutput Logic(Flow flow)
    {
        Debug.LogWarning("This node is only for the code generators to understand what to do it does not work in a normal graph");
        return Exit;
    }

    public virtual string GeneratorLogic(int indent)
    {
        return $"/* Create logic Generator */";
    }

    public virtual string GeneratorOutput(ValueOutput output)
    {
        return $"/* Create output Generator */";
    }

    public virtual string GenerateValue(ValueInput input)
    {
        if (input.hasValidConnection)
        {
            return "/* Generated Units do not support connected ports */";
        }
        else if (input.hasDefaultValue)
        {
            return this.defaultValues[input.key].ToString();
        }
        else
        {
            return $"/* {input.key} Requires Input */";
        }
    }
}

