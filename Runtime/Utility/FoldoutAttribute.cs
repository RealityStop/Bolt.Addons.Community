using System;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public string header;
        public bool isOpen;
        public FoldoutAttribute(string header)
        {
            this.header = header;
        }
    }
    
    public class FoldoutEndAttribute : PropertyAttribute
    {
    } 
}