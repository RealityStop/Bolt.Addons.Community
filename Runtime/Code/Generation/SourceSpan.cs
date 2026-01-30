using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public readonly struct SourceSpan
    {
        public readonly int Start;
        public readonly int End;
    
        public int Length => End - Start;
    
        public SourceSpan(int start, int end)
        {
            Start = start;
            End = end;
        }
    
        public bool Contains(int index)
            => index >= Start && index < End;
    } 
}