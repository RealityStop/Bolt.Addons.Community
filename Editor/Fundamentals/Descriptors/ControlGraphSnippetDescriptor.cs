using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[Descriptor(typeof(ControlGraphSnippet))]
public class ControlGraphSnippetDescriptor : MacroDescriptor<ControlGraphSnippet, MacroDescription>
{
    public ControlGraphSnippetDescriptor(ControlGraphSnippet target) : base(target)
    {
    }
}
