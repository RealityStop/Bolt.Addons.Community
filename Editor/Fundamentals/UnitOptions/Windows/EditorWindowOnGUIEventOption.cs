using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
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