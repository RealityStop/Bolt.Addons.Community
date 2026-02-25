using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(EditorWindowOnLostFocus))]
    public sealed class EditorWindowOnLostFocusEventOption : EditorWindowEventOption<EditorWindowOnLostFocus>
    {
        [Obsolete]
        public EditorWindowOnLostFocusEventOption()
        {
        }

        public EditorWindowOnLostFocusEventOption(EditorWindowOnLostFocus unit) : base(unit)
        {
        }
    }
}