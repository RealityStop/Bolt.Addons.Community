using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(EditorWindowOnDestroy))]
    public sealed class EditorWindowOnDestroyEventOption : EditorWindowEventOption<EditorWindowOnDestroy>
    {
        [Obsolete]
        public EditorWindowOnDestroyEventOption()
        {
        }

        public EditorWindowOnDestroyEventOption(EditorWindowOnDestroy unit) : base(unit)
        {
        }
    }
}