using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Widget(typeof(CommentUnit))]
    public sealed class CommentUnitWidget : UnitWidget<CommentUnit>
    {

        public CommentUnitWidget(CommentUnit unit) : base(unit)
        {
        }

        protected override NodeColorMix baseColor
        {
            get
            {
                return new NodeColorMix
                {
                    red = unit.color.r,
                    green = unit.color.g,
                    blue = unit.color.b,
                    yellow = 0f,
                    gray = 0f,
                    orange = 0f,
                    teal = 0f
                    
                };
            }
        }
    }
}