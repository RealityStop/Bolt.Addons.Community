using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public string group;
        public bool startExpanded;

        public FoldoutAttribute(string group, bool startExpanded = false)
        {
            this.group = group;
            this.startExpanded = startExpanded;
        }
    }
}