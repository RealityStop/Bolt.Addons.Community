using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[UnitCategory("Community/Control")]
[UnitTitle("FlowDebugger")]
[TypeIcon(typeof(Unit))]
public class FlowDebugger : Unit
{

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Enter;
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput Exit;
    protected override void Definition()
    {
        Enter = ControlInput(nameof(Enter), Debug);
        Exit = ControlOutput(nameof(Exit));
        Succession(Enter, Exit);
    }

    public ControlOutput Debug(Flow flow)
    {
        try
        {
            flow.Invoke(Exit);
        }
        catch (Exception ex)
        {
            var target = RuntimeGraphUtility.GetFirstUnitOfType<Unit>(this, u => u.GetException(flow.stack) == ex, flow.stack.ToReference());
            Log(flow, ex, target);
            throw;
        }
        return null;
    }

    void Log(Flow flow, Exception ex, Unit target)
    {
        UnityEngine.Object self = flow.stack.self;
        string graphPath = flow.stack.ToString();

        UnityEngine.Debug.LogError(
            "Visual Scripting Exception\n" +
            "Object: " + (self != null ? self.name : "<null>") + "\n" +
            "Unit: [" + target.GetType().As().CSharpName(false, true, false) + $"] {RuntimeGraphUtility.ReadableName(target)}\n" +
            "Graph Path: " + graphPath + "\n" +
            ex,
            self
        );
    }
}