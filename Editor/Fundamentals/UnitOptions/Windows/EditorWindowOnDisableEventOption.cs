using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
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