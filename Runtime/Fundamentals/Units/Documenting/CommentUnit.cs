using System;

using Bolt;
using Unity.VisualScripting;
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
        public Color color = new Color(0.1961f, 0.3333f, 0.7176f, 1f);

        [Inspectable]
        [UnitHeaderInspectable]
        [InspectorTextArea()]
        public string comment;

        [Serialize]
        private int _width = 120;
        [Inspectable]
        [InspectorRange(120, 400)]
        public int width { get => _width; set => _width = Mathf.Clamp(value, 120, 400); }

        protected override void Definition()
        {
        }

        public override bool isControlRoot { get { return true; } }
    }
        
}
