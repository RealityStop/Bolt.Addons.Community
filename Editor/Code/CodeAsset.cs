using UnityEngine;
using Unity.VisualScripting;
using System;
using Bolt.Addons.Libraries.CSharp;

namespace Bolt.Addons.Community.Code.Editor
{
    [Serializable]
    public abstract class CodeAsset : ScriptableObject
    {
        [Inspectable]
        [InspectorWide]
        public string title;
        [Inspectable]
        [InspectorWide]
        public string category;
        public bool optionsOpened;
        public bool preview;
        public string lastCompiledName;

        public string GetFullTypeName()
        {
            return category + (string.IsNullOrEmpty(category) ? string.Empty : ".") + title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
        }
    }
}
