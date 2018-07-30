using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using Lasm.BoltAddons.Helpers;
using UnityEngine;
using Bolt.Addons.Community.Fundamentals.Units.Documenting;

namespace Lasm.BoltAddons.Helpers.Editor
{
    [Widget(typeof(CommentUnit))]
    public sealed class CommentUnitWidget : UnitWidget<CommentUnit>
    {

        public CommentUnitWidget(FlowCanvas canvas, CommentUnit unit) : base(canvas, unit)
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