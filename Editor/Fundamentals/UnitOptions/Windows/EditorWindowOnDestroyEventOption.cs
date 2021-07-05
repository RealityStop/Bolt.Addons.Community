using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
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