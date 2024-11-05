using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[Descriptor(typeof(ValueGraphSnippet))]
public class ValueGraphSnippetDescriptor : MacroDescriptor<ValueGraphSnippet, MacroDescription>
{
    public ValueGraphSnippetDescriptor(ValueGraphSnippet target) : base(target)
    {
    }
}
