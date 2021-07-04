using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
{
    public abstract class EditorWindowEventOption<TUnit> : UnitOption<TUnit> where TUnit : Unit
    {
        [Obsolete]
        public EditorWindowEventOption()
        {
        }

        public EditorWindowEventOption(TUnit unit) : base(unit)
        {
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory("Events/Community/Editor/Window");
        }
    }
}