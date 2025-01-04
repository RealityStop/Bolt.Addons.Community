﻿using UnityEngine;
using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [RenamedFrom("Bolt.Addons.Community.Code.CodeAsset")]
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
        public List<string> lastCompiledNames;

        #if UNITY_EDITOR
        public bool shouldRefresh;
        #endif

        public string GetFullTypeName()
        {
            return category + (string.IsNullOrEmpty(category) ? string.Empty : ".") + title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
        }
    }
}
