using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
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