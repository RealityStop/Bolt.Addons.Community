using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Widget(typeof(Shortcut))]
public class ShortcutWidget : UnitWidget<Shortcut>
{
    public ShortcutWidget(FlowCanvas canvas, Shortcut unit) : base(canvas, unit)
    {
    }

    protected override void OnDoubleClick()
    {
        base.OnDoubleClick();
        Move();
    }

    protected override IEnumerable<DropdownOption> contextOptions
    {
        get
        {
            var main = base.contextOptions.ToList();
            if (selection.Count == 2 && selection.Contains(unit))
            {
                main.Add(new DropdownOption((Action)Set, "Set Target"));
            }

            return main;
        }
    }

    private void Set()
    {
        var _selection = selection.First(_unit => _unit != unit);
        if (unit.targetPos is Unit)
        {
            unit.targetPos = (Unit)selection.First(_unit => _unit != unit);
        }
        else 
        {
            unit.targetPos = selection.First(_unit => _unit != unit);
        }
        unit.Describe();
    }

    private void Move()
    {
        if (unit.targetPos != null)
        {
            if (unit.targetPos is SubgraphUnit subgraph)
            {
                if (unit.OpenSubgraph)
                {
                    window.reference = reference.ChildReference(subgraph, false);
                }
                else
                {
                    if (unit.graph.units.Contains(unit.targetPos))
                    {
                        canvas.TweenViewport((unit.targetPos as Unit).position, canvas.zoom, .2f);
                    }
                    else
                    {
                        Debug.LogWarning("Can only move to a Unit in this graph");
                    }
                }
            }
            else if (unit.targetPos is Unit)
            {
                if (unit.graph.units.Contains(unit.targetPos))
                {
                    canvas.TweenViewport((unit.targetPos as Unit).position, canvas.zoom, .2f);
                }
                else
                {
                    Debug.LogWarning("Can only move to a Unit in this graph");
                }
            }
            else 
            {
                if (unit.graph.sticky.Contains(unit.targetPos))
                {
                    canvas.TweenViewport((unit.targetPos as StickyNote).position.position, canvas.zoom, .2f);
                }
                else if (unit.graph.groups.Contains(unit.targetPos))
                {
                    canvas.TweenViewport((unit.targetPos as GraphGroup).position.position, canvas.zoom, .2f);
                }
            }
        }
    }
}
