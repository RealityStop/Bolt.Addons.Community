using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public interface IGraphProvider
    {
        string Name { get; }
        bool IsEnabled { get; }

        /// <summary>
        /// Order to display.
        /// </summary>
        int Order { get; }

        void SetEnabled(bool value);
        void ToggleProvider();
        IEnumerable<(GraphReference, IGraphElement)> GetElements();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class GraphProviderAttribute : Attribute { }
}