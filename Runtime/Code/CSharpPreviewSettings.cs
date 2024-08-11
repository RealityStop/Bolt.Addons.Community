using System;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class CSharpPreviewSettings : ScriptableObject
    {

        public Color VariableColor = new Color(38, 204, 204, 255);

        public Color StringColor = new Color(204, 136, 51, 255);

        public Color NumericColor = new Color(221, 255, 187, 255);

        public Color ConstructColor = new Color(68, 138, 255, 255);

        public Color TypeColor = new Color(51, 238, 170, 255);

        public Color EnumColor = new Color(255, 255, 187, 255);

        public Color InterfaceColor = new Color(221, 255, 187, 255);
        [HideInInspector]
        public bool isInitalized = false;
        public bool ShowSubgraphComment = true;

        public void Initalize()
        {
            VariableColor = new Color(38, 204, 204, 255);

            StringColor = new Color(204, 136, 51, 255);

            NumericColor = new Color(221, 255, 187, 255);

            ConstructColor = new Color(68, 138, 255, 255);

            TypeColor = new Color(51, 238, 170, 255);

            EnumColor = new Color(255, 255, 187, 255);

            InterfaceColor = new Color(221, 255, 187, 255);
            isInitalized = true;
        }
    }
}
