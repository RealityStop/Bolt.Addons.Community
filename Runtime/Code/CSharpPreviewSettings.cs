using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class CSharpPreviewSettings : ScriptableObject
    {
        public Color VariableColor = new Color(0.149f, 0.8f, 0.8f, 1f);
        public Color StringColor = new Color(0.8f, 0.533f, 0.2f, 1f);
        public Color NumericColor = new Color(0.867f, 1f, 0.733f, 1f);
        public Color ConstructColor = new Color(0.267f, 0.541f, 1f, 1f);
        public Color TypeColor = new Color(0.2f, 0.933f, 0.667f, 1f);
        public Color EnumColor = new Color(1f, 1f, 0.733f, 1f);
        public Color InterfaceColor = new Color(0.867f, 1f, 0.733f, 1f);
        public float zoomValue = 1f;

        [HideInInspector]
        public List<CompilerInfo> pendingInfo = new List<CompilerInfo>();

        [HideInInspector]
        public bool isInitalized = false;
        public bool showSubgraphComment = true;
        public bool showRecommendations = true;
        public bool showTooltips = true;
        public int recursionDepth = 10;

        public static bool ShouldShowSubgraphComment = true;
        public static bool ShouldShowRecommendations = true;
        public static bool ShouldGenerateTooltips = true;
        public static int RecursionDepth = 10;

        public void Initalize()
        {
            VariableColor = new Color(0.149f, 0.8f, 0.8f, 1f);
            StringColor = new Color(0.8f, 0.533f, 0.2f, 1f);
            NumericColor = new Color(0.867f, 1f, 0.733f, 1f);
            ConstructColor = new Color(0.267f, 0.541f, 1f, 1f);
            TypeColor = new Color(0.2f, 0.933f, 0.667f, 1f);
            EnumColor = new Color(1f, 1f, 0.733f, 1f);
            InterfaceColor = new Color(0.867f, 1f, 0.733f, 1f);

            isInitalized = true;
        }

        [Serializable]
        public class CompilerInfo
        {
            public UnityEngine.Object @object;
            public string relativePath;
            public string compilerTypeName;

            [NonSerialized] public object compiler;

            public void RestoreCompiler()
            {
                if (compiler == null && !string.IsNullOrEmpty(compilerTypeName))
                {
                    var type = Type.GetType(compilerTypeName);
                    if (type != null)
                    {
                        compiler = Activator.CreateInstance(type);
                    }
                }
            }
        }
    }
}