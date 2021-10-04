using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(EditorWindowOnDisable))]
    public sealed class EditorWindowOnDisableEventOption : EditorWindowEventOption<EditorWindowOnDisable>
    {
        [Obsolete]
        public EditorWindowOnDisableEventOption()
        {
        }

        public EditorWindowOnDisableEventOption(EditorWindowOnDisable unit) : base(unit)
        {
        }
    }
}