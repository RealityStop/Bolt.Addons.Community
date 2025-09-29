using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseGraphProvider : IGraphProvider
    {
        protected NodeFinderWindow window;
        public BaseGraphProvider(NodeFinderWindow window)
        {
            this.window = window;
        }
        public abstract string Name { get; }
        private bool _isEnabled = true;
        public virtual bool IsEnabled => _isEnabled;

        public abstract int Order { get; }

        public abstract IEnumerable<(GraphReference, IGraphElement)> GetElements();

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        public void ToggleProvider()
        {
            SetEnabled(!IsEnabled);
        }
    }
}