using System;
using Unity.VisualScripting;
using UnityEngine;
using Bolt.Addons.Libraries.Humility;

namespace Bolt.Addons.Community.Fundamentals.Units.Documenting
{
    [RenamedFrom("Lasm.Bolt.Comments.CommentUnit")]
    [RenamedFrom("Lasm.BoltAddons.Helpers.CommentUnit")]
    [TypeIcon(typeof(CommentUnit))]
    [UnitTitle("Comment")]
    [UnitShortTitle("")]
    [UnitCategory("Community\\Documentation")]
    [Serializable]
    public class CommentUnit : Unit
    {
        // Global
        public static CommentPalette style;

        // Per Instance
        public bool
            refresh = true;     // Unused - for optimisation

        [Inspectable]
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
