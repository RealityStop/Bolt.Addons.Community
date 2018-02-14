using System;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Helpers
{

    [TypeIcon(typeof(CommentUnit))]
    [UnitTitle("Comment")]
    [UnitShortTitle("")]
    [UnitCategory("Tools/Graphs")]
    public class CommentUnit : Unit
    {
        [Inspectable]
        public Color color;

        [Inspectable]
        [UnitHeaderInspectable]
        [InspectorTextArea()]
        public string comment;

        protected override void Definition()
        {
        }

        public override bool isControlRoot { get { return true; } }
    }
        
}
