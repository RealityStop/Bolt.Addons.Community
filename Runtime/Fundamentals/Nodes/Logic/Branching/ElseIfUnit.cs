using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community{
[UnitCategory("Community/Control")]
[UnitTitle("Else If")]
[TypeIcon(typeof(If))]
public class ElseIfUnit : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Enter;

    [Inspectable]
    public int amount = 1;

    [DoNotSerialize]
    public ControlOutput If;

    [DoNotSerialize]
    public ControlOutput Else;

    [DoNotSerialize]
    public ValueInput condition;

    [DoNotSerialize]
    [PortLabelHidden]
    public List<ControlOutput> elseIfs;

    [DoNotSerialize]
    [PortLabelHidden]
    public List<ValueInput> elseIfConditions;

    protected override void Definition()
    {
        amount = Mathf.Clamp(amount, 1, 10);
        elseIfs = new();
        elseIfConditions = new();

        Enter = ControlInput(nameof(Enter), ElseIf);
        If = ControlOutput(nameof(If));
        Succession(Enter, If);
        condition = ValueInput<bool>(nameof(condition));

        for (int i = 0; i < amount; i++)
        {
            var controlOutput = ControlOutput("elseif" + i);
            elseIfs.Add(controlOutput);
            Succession(Enter, controlOutput);
            var valueInput = ValueInput<bool>(i.ToString());
            elseIfConditions.Add(valueInput);
            relations.Add(new UnitRelation(valueInput, controlOutput));
        }

        Else = ControlOutput(nameof(Else));
        Succession(Enter, Else);
    }
    public ControlOutput ElseIf(Flow flow) 
    {
        var mainCondition = flow.GetValue<bool>(condition);
        if(mainCondition)
        {
            return If;
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                bool condition = flow.GetValue<bool>(elseIfConditions[i]);
                if(condition)
                {
                    return elseIfs[i];
                }else
                {
                    continue;
                }
            }
        }
        return Else;
    }
}}