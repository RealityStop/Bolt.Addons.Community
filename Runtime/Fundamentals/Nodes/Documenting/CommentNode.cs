using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Lasm.Bolt.Comments.CommentUnit")]
    [RenamedFrom("Lasm.BoltAddons.Helpers.CommentUnit")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Documenting.CommentUnit")]
    [TypeIcon(typeof(CommentNode))]
    [UnitTitle("Comment")]
    [UnitShortTitle("")]
    [UnitCategory("Community\\Documentation")]
    [Serializable]
    public class CommentNode : Unit
    {
        // Global
        public static CommentPalette style;

        // Per Instance
        public bool
            refresh = true;     // Unused - for optimisation

        [Inspectable][Serialize]
        public (int palette, int row, int col)
            paletteSelection = (0, 0, 0);

        [Inspectable]
        public bool
            customPalette = false,
            fontColorize = false,
            lockedToPalette = false,
            fontBold = false,
            fontItalic = false,
            autoWidth = true,
            hasOutline = false,
            hasTitle = false,
            alignCentre = false;

        [Inspectable]
        public int
            fontSize = 13,
            maxWidth = 500;

        [Inspectable, UnitHeaderInspectable, InspectorTextArea()]
        public string
            title,
            comment;

        [Inspectable]
        public Color
            color = new Color(0.1961f, 0.3333f, 0.7176f, 1f),
            fontColor = new Color(1f, 1f, 1f, 1f);

        protected override void Definition() { }

        public override bool isControlRoot { get { return true; } }
    }
}
