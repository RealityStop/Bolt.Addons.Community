using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
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