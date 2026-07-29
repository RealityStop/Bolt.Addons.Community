using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{

    [Descriptor(typeof(AssetFieldUnit))]
    public class AssetFieldUnitDescriptor : UnitDescriptor<AssetFieldUnit>
    {
        public AssetFieldUnitDescriptor(AssetFieldUnit target) : base(target)
        {
            if (target.field)
                target.field.OnChanged += target.Describe;
        }

        protected override string DefinedSurtitle()
        {
            if (target.field)
                return target.field.parentAsset.title;
            else
                return base.DefinedSurtitle();
        }

        protected override EditorTexture DefinedIcon()
        {
            if (target.field)
                return target.field.type.Icon();
            else
                return BoltCore.Icons.errorState;
        }

        protected override string DefinedTitle()
        {
            if (target.field)
                return target.field.parentAsset.title + "." + target.field.FieldName;
            else
                return "No Field Assigned";
        }

        protected override string DefinedShortTitle()
        {
            if (target.field)
                return target.field.FieldName;
            else
                return base.DefinedShortTitle();
        }
    }

}