using System;
using System.Collections.Generic;
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

        public List<IGraphElement> connectedElements = new List<IGraphElement>();

        public Rect
    wholeRect,
    borderRect,
    textRect;

        [Inspectable]
        [Serialize]
        public (int palette, int row, int col)
            paletteSelection = (0, 0, 0);

        [Inspectable]
        public bool
            curvedLine = true,
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

        public void UpdateFrom(CommentNode other)
        {
            if (other == null || other == this) return;

            refresh = other.refresh;
            connectedElements = new List<IGraphElement>(other.connectedElements);

            wholeRect = other.wholeRect;
            borderRect = other.borderRect;
            textRect = other.textRect;

            paletteSelection = other.paletteSelection;

            curvedLine = other.curvedLine;
            customPalette = other.customPalette;
            fontColorize = other.fontColorize;
            lockedToPalette = other.lockedToPalette;
            fontBold = other.fontBold;
            fontItalic = other.fontItalic;
            autoWidth = other.autoWidth;
            hasOutline = other.hasOutline;
            hasTitle = other.hasTitle;
            alignCentre = other.alignCentre;

            fontSize = other.fontSize;
            maxWidth = other.maxWidth;

            title = other.title;
            comment = other.comment;

            color = other.color;
            fontColor = other.fontColor;
        }
    }
}
