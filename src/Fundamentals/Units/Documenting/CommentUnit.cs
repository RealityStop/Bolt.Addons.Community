using System;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.Documenting
{
    [RenamedFrom("Lasm.Bolt.Comments.CommentUnit")]
    [RenamedFrom("Lasm.BoltAddons.Helpers.CommentUnit")]
    [TypeIcon(typeof(CommentUnit))]
    [UnitTitle("Comment")]
    [UnitShortTitle("")]
    [UnitCategory("Community\\Documentation")]
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
