using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
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