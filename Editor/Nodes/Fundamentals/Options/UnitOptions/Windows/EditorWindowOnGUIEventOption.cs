using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(EditorWindowOnGUI))]
    public sealed class EditorWindowOnGUIEventOption : EditorWindowEventOption<EditorWindowOnGUI>
    {
        [Obsolete]
        public EditorWindowOnGUIEventOption()
        {
        }

        public EditorWindowOnGUIEventOption(EditorWindowOnGUI unit) : base(unit)
        {
        }
    }
}