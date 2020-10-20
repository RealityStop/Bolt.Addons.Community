using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;
using Bolt.Addons.Community.Fundamentals.Units.Documenting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
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
                if (unit.color.maxColorComponent == 0)
                {
                    return new NodeColorMix() { red = 0.6578709f, green = 1f };
                }

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