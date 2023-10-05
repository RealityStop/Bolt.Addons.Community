using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Descriptor(typeof(Shortcut))]
public class ShortcutDescriptor : UnitDescriptor<Shortcut>
{
    public ShortcutDescriptor(Shortcut target) : base(target)
    {
    }

    protected override string DefinedSubtitle()
    {
        var subtitle = string.Empty;

        if (target.targetPos != null)
        {
            if (target.targetPos is not SubgraphUnit)
            {
                if (target.targetPos is MemberUnit member)
                {
                    subtitle = "Target : " + member.member.ToPseudoDeclarer();
                }
                else if (target.targetPos is Unit)
                {
                    subtitle = "Target : " + BoltFlowNameUtility.UnitTitle(target.targetPos.GetType(), false, false);
                }
                else 
                {
                    subtitle = "Target : " + target.targetPos.GetType().DisplayName();
                }
            }
            else if (target.targetPos is SubgraphUnit subgraph && !string.IsNullOrEmpty(subgraph.nest.graph.title))
            {
                if (subgraph.nest.graph.title.Length > 0)
                {
                    subtitle = "Target : " + subgraph.nest.graph.title;
                }
            }
            else if (target.targetPos is SubgraphUnit subgraphUnit && (string.IsNullOrEmpty(subgraphUnit.nest.graph.title) || subgraphUnit.nest.graph.title?.Length == 0))
            {
                subtitle = "Target : " + "UnnamedSubgraph";
            }
        }
        else
        {
            subtitle = "No Target";
        }
        return subtitle;
    }
}
