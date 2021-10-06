using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(EditorWindowOnFocus))]
    public sealed class EditorWindowOnFocusEventOption : EditorWindowEventOption<EditorWindowOnFocus>
    {
        [Obsolete]
        public EditorWindowOnFocusEventOption()
        {
        }

        public EditorWindowOnFocusEventOption(EditorWindowOnFocus unit) : base(unit)
        {
        }
    }
}