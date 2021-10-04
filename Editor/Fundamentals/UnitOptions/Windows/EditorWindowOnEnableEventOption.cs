using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(EditorWindowOnEnable))]
    public sealed class EditorWindowOnEnableEventOption : EditorWindowEventOption<EditorWindowOnEnable>
    {
        [Obsolete]
        public EditorWindowOnEnableEventOption()
        {
        }

        public EditorWindowOnEnableEventOption(EditorWindowOnEnable unit) : base(unit)
        {
        }
    }
}