using System;
using System.Linq;
using Unity.VisualScripting;

[UnitCategory("Community/Utility")]
[UnitTitle("Shortcut")]
[TypeIcon(typeof(Flow))]
public class Shortcut : Unit
{
    public object targetPos;

    public bool OpenSubgraph;

    protected override void Definition()
    {
        isControlRoot = true;
    }
}