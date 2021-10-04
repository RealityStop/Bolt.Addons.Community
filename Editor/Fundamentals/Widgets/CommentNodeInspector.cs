using UnityEngine;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(CommentNode)), CanEditMultipleObjects]
    public class CommentNodeInspector : Inspector
    {
        // Constants
        const float
            xIndentA = 10f,
            xIndentB = 20f,
            xIndentC = 96f,
            xFieldWidth = 76f,
            xFieldOffset = -122f,
            xFieldDivision = 20f / 9f;

        // Statics
        static float
            yCurrent = 0f,
            xCurrent = 0f,
            wCurrent = 0f,
            hCurrent = 0f,
            xWidth,
            xFieldRatio,
            tempFloat,
#pragma warning disable 0414
            guiScale = 1f;      // Unused at this stage
#pragma warning restore 0414

        static bool
            copyACTIVE,
            copyAll,
            copyCOLORS,
            copyColor,
            copyFontColor,
            copyLOCK,
            copyLockToPalette,
            copyLockToFontColor,
            copyFONT,
            copyFontSize,
            copyFontStyle,
            copyMaxWidth,
            copyTEXT,
            copyTitle,
            copyContent,

            customAddColor,
            scaleGUI;           // Unused at this stage


        static readonly GUIStyle
            sectionGUI = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold | FontStyle.Italic },
            inspectorGUI = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold },
            titleGUI = new GUIStyle(GUI.skin.textField) { fontSize = 9, fontStyle = FontStyle.Bold, wordWrap = true },
            commentGUI = new GUIStyle(GUI.skin.textField) { fontSize = 11, wordWrap = true },
            buttonGUI = new GUIStyle(GUI.skin.button) { fontSize = 9 };

        static CommentNode
            copyUnit,
            unit;

        // For palette calculations
        const int
            columns = 9,
            rows = 6;

        const float
            steps = 1f / columns;

        static readonly float[]
            offset = { 0f, 1f / 3f, 2f / 3f };

        static Color[]
            baseColors = new Color[columns];

        public static Color[,,] colorPalette = new Color[2, 6, 9];  // Main, Custom
        public static Color[,,] fontPalette = new Color[3, 6, 9];  // Main, Colorized, Custom

        static CommentPalette style => CommentNode.style;
        public static bool initialised = false;

        ///////////////////////////   Methods   /////////////////////////////////////////////

        public override void Initialize()
        {
            base.Initialize();
        }

        // Update the palette colours on change
        public static void UpdatePalette()
        {
            for (int i = 0; i < columns; i++)
            {
                baseColors[i].r = 1f - Mathf.Clamp(Mathf.Abs(((i * steps + style.colorOffset - offset[0]) % 1f) - 0.5f) * style.colorSpread, 0f, 1f);
                baseColors[i].g = 1f - Mathf.Clamp(Mathf.Abs(((i * steps + style.colorOffset - offset[1]) % 1f) - 0.5f) * style.colorSpread, 0f, 1f);
                baseColors[i].b = 1f - Mathf.Clamp(Mathf.Abs(((i * steps + style.colorOffset - offset[2]) % 1f) - 0.5f) * style.colorSpread, 0f, 1f);
                baseColors[i].a = 1f;
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (!initialised) colorPalette[1, row, col] = fontPalette[2, row, col] = Color.black; // Probably not required if Custom palette already set to blank on SO
                    colorPalette[0, row, col] =
                        Color.Lerp(
                            (style.greyScale ?
                                new Color(baseColors[col].grayscale, baseColors[col].grayscale, baseColors[col].grayscale)
                                : baseColors[col]),
                            Color.Lerp(
                                Color.black,
                                Color.white,
                                (float)row / (rows - 1)),
                            Mathf.Abs((row - ((rows - 1) / 2f)) * style.colorHeight / ((rows - 1) / 2f)))
                        * 3f;

                    fontPalette[0, row, col] = row < (rows - 1) / 2f ? Color.white * 3f : Color.black * 3f;

                    if (fontPalette[0, row, col].maxColorComponent > 0.5f)
                    {
                        Vector3 tempColor = new Vector3(colorPalette[0, row, col].r, colorPalette[0, row, col].g, colorPalette[0, row, col].b);
                        Vector3 tempColor2 = tempColor / colorPalette[0, row, col].maxColorComponent * 0.4f + Vector3.one;
                        Vector3 tempColor3 = tempColor2 / Mathf.Max(tempColor2.x, tempColor2.y, tempColor2.z);
                        fontPalette[1, row, col] = new Color(tempColor3.x, tempColor3.y, tempColor3.z, 1f);
                    }
                    else
                    {
                        Vector3 tempColor = new Vector3(colorPalette[0, row, col].r, colorPalette[0, row, col].g, colorPalette[0, row, col].b);
                        Vector3 tempColor2 = tempColor / colorPalette[0, row, col].maxColorComponent * 0.1f;
                        Vector3 tempColor3 = tempColor2 * (1f / (Mathf.Max(tempColor2.x, tempColor2.y, tempColor2.z) + 1f));
                        fontPalette[1, row, col] = new Color(tempColor3.x, tempColor3.y, tempColor3.z, 1f);
                    }
                }
            }
        }

        // GUI drawing helper
        Rect GUIRect(float? xMargin = null, float? yMargin = null, float x = 0, float right = 0, float y = 0, float down = 0, float? w = null, float? h = null)
        {
            xCurrent = xMargin ?? xCurrent + right;
            yCurrent = yMargin ?? yCurrent + down;
            wCurrent = w ?? wCurrent;
            hCurrent = h ?? hCurrent;

            return new Rect(x + xCurrent, y + yCurrent, wCurrent, hCurrent);
        }

        // Reset GUI colours to default
        void ResetGUI() => GUI.backgroundColor = GUI.contentColor = Color.white;

        // Custom green/red toggle button
        void ToggleButtonColor(bool isTrue, bool alwaysVisible = false, bool red = false)
        {
            if (isTrue)
            {
                GUI.contentColor = red ? new Color(1f, 0.7f, 0.7f, 1f) : new Color(0.7f, 1f, 0.7f, 1f);
                GUI.backgroundColor = red ? new Color(0.8f, 0.5f, 0.5f, 1f) : new Color(0.5f, 0.8f, 0.5f, 1f);
            }
            else
            {
                GUI.contentColor = alwaysVisible ? new Color(1f, 1f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);
                GUI.backgroundColor = Color.white;
            }
        }


        // Required Methods
        public CommentNodeInspector(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            return 800;
        }


        ///////////////////////////   OnGUI   /////////////////////////////////////////////
        protected override void OnGUI(Rect position, GUIContent label)
        {
            // Start block
#if UNITY_2021_1_OR_NEWER
            BeginBlock(metadata, position);
#else
            BeginBlock(metadata, position, GUIContent.none);
#endif
            if (!initialised) UpdatePalette();  // Probably not required with Scriptable **************

            xWidth = position.width;
            xFieldRatio = (xWidth < 336 ? ((335 - xWidth) / xFieldDivision) + xFieldDivision / 10f : 0) + ((xWidth - 335) / xFieldDivision);
            unit = (CommentNode)metadata.value;

            // Initially copy settings to selected unit if Copy is Active
            if (copyACTIVE && copyUnit != null)
            {
                if (copyColor) unit.color = copyUnit.color;
                if (copyFontColor) unit.fontColor = copyUnit.fontColor;
                if (copyFontSize) unit.fontSize = copyUnit.fontSize;
                if (copyFontStyle) { unit.fontBold = copyUnit.fontBold; unit.fontItalic = copyUnit.fontItalic; unit.hasOutline = copyUnit.hasOutline; }
                if (copyLockToFontColor) unit.fontColorize = copyUnit.fontColorize;
                if (copyLockToPalette) { unit.lockedToPalette = copyUnit.lockedToPalette; unit.paletteSelection = copyUnit.paletteSelection; unit.customPalette = copyUnit.customPalette; }
                if (copyMaxWidth) { unit.maxWidth = copyUnit.maxWidth; unit.autoWidth = copyUnit.autoWidth; }
                if (copyTitle) { unit.title = copyUnit.title; unit.hasTitle = copyUnit.hasTitle; }
                if (copyContent) unit.comment = copyUnit.comment;
            }


            ///////////////////////////   Section - Color Palette   /////////////////////////////////////////////

            // Header
            GUI.contentColor = new Color(1, 1, 0.6f);
            GUI.Label(GUIRect(xMargin: xIndentA, yMargin: position.y, w: xIndentC, h: 20), "Color Palette", sectionGUI);

            // View custom palette?
            ToggleButtonColor(unit.customPalette, alwaysVisible: true);
            if (GUI.Button(GUIRect(xMargin: xIndentC, w: 60, h: 18), "Custom", buttonGUI)) unit.customPalette = !unit.customPalette;
            if (unit.lockedToPalette) unit.paletteSelection.palette = unit.customPalette ? 1 : 0;

            // Copy current unit colors to a custom color
            ToggleButtonColor(customAddColor, red: true);
            if (unit.customPalette) if (GUI.Button(GUIRect(right: 65), "Add Color", buttonGUI)) { unit.customPalette = true; customAddColor = !customAddColor; }

            // Separator
            EditorGUI.DrawRect(GUIRect(xMargin: xIndentA, down: 20, w: xWidth, h: 1), new Color(0.5f, 0.5f, 0.5f, 1));

            // Palette initial start point
            GUIRect(xMargin: xIndentB, down: 10, w: xWidth / columns - 1f, h: 30);

            // Draw palette
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    GUI.backgroundColor = colorPalette[unit.customPalette ? 1 : 0, row, col];
                    GUI.contentColor = fontPalette[(unit.customPalette ? 2 : unit.fontColorize ? 1 : 0), row, col];
                    if (GUI.Button(GUIRect(x: (xWidth - xIndentA) / columns * col), "Aa"))
                    {
                        // If adding custom color, set selected custom palette color to current color
                        if (customAddColor)
                        {
                            customAddColor = unit.fontColorize = false;

                            colorPalette[1, row, col] = unit.color * 3f;
                            fontPalette[2, row, col] = unit.fontColor;
                        }
                        // Else set current unit color to selected palette color
                        else
                        {
                            unit.color = GUI.backgroundColor / 3f;
                            unit.fontColor = GUI.contentColor;
                        }
                        unit.paletteSelection = (unit.customPalette ? 1 : 0, row, col);
                    }

                    // If lockToPalette draw selection box
                    if (row == unit.paletteSelection.row && col == unit.paletteSelection.col && unit.lockedToPalette)
                        GUI.DrawTexture(GUIRect(x: (xWidth - xIndentA) / columns * col, w: xWidth / columns - 1f), Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, false, 0, fontPalette[1, row, col], 3f, 6f);
                }
                GUIRect(down: 30);
            }
            ResetGUI();

            // Lock to palette?
            ToggleButtonColor(unit.lockedToPalette);
            if (GUI.Button(GUIRect(x: (xWidth - xIndentA) / columns * 0, down: 10, w: xWidth / columns * 2f - 4, h: 18), "Lock Palette", buttonGUI)) { unit.lockedToPalette = !unit.lockedToPalette; }
            ResetGUI();

            // + Draw current color picker
            var tempColor = unit.color;
            ToggleButtonColor(customAddColor, red: true);
            unit.color = EditorGUI.ColorField(GUIRect(x: (xWidth - xIndentA) / columns * 2), new GUIContent(""), unit.color, true, false, false);
            ResetGUI();
            if (unit.customPalette && unit.lockedToPalette && tempColor != unit.color) colorPalette[1, unit.paletteSelection.row, unit.paletteSelection.col] = (unit.color * 3f).WithAlpha(3f);

            // + Draw current font color
            tempColor = unit.fontColor;
            GUI.Label(GUIRect(x: (xWidth - xIndentA) / columns * 5f - 25, w: 23), "Aa", commentGUI);
            ToggleButtonColor(customAddColor, red: true);
            unit.fontColor = EditorGUI.ColorField(GUIRect(x: (xWidth - xIndentA) / columns * 5, w: xWidth / columns * 2f - 4f), new GUIContent(""), unit.fontColor, true, false, false);
            ResetGUI();
            if (unit.customPalette && unit.lockedToPalette && tempColor != unit.fontColor) fontPalette[2, unit.paletteSelection.row, unit.paletteSelection.col] = unit.fontColor;//

            // + Colorize font?
            ToggleButtonColor(unit.fontColorize);
            if (unit.lockedToPalette && !unit.customPalette)
                if (GUI.Button(GUIRect(x: (xWidth - xIndentA) / columns * 7, w: xWidth / columns * 2f - 2), "Colorize", buttonGUI)) { unit.fontColorize = !unit.fontColorize; UpdatePalette(); }
            ResetGUI();

            ///////////////////////////   Section - Settings   /////////////////////////////////////////////

            // Header
            GUI.contentColor = new Color(1, 1, 0.6f);
            GUI.Label(GUIRect(xMargin: xIndentA, down: 33, w: xIndentC, h: 20), "Settings", sectionGUI);
            EditorGUI.DrawRect(GUIRect(down: 20, w: xWidth, h: 1), new Color(0.5f, 0.5f, 0.5f, 1));
            ResetGUI();

            // Color Spread
            tempFloat = style.colorSpread;
            GUI.Label(GUIRect(xMargin: 0, x: xIndentB, down: 10, w: xIndentC, h: 18), "Color Spread", inspectorGUI);
            style.colorSpread = Mathf.Clamp(EditorGUI.FloatField(GUIRect(x: xIndentC + xFieldOffset - xFieldRatio, w: xIndentC + xFieldWidth + xFieldRatio - 5), " ", style.colorSpread * 10f, titleGUI) / 10f, 0f, 3f);
            if (style.colorSpread != tempFloat) UpdatePalette();

            // + Toggle greyscale?
            ToggleButtonColor(style.greyScale);
            if (GUI.Button(GUIRect(right: xIndentC + 50, w: 40), "Grey", buttonGUI)) { style.greyScale = !style.greyScale; UpdatePalette(); }
            ResetGUI();

            // + 3 palette presets
            if (GUI.Button(GUIRect(right: 42), "Mars", buttonGUI)) { style.colorSpread = 1.5f; style.colorOffset = 0f; UpdatePalette(); }
            if (GUI.Button(GUIRect(right: 42), "Pastel", buttonGUI)) { style.colorSpread = 1.5f; style.colorOffset = 0.4f; UpdatePalette(); }
            if (GUI.Button(GUIRect(right: 42), "Vivid", buttonGUI)) { style.colorSpread = 2.5f; style.colorOffset = 0.4f; UpdatePalette(); }

            // Color Contrast
            tempFloat = style.colorHeight;
            GUI.Label(GUIRect(xMargin: 0, x: xIndentB, down: 22, w: xIndentC), "Color Offset", inspectorGUI);
            style.colorHeight = Mathf.Clamp(EditorGUI.FloatField(GUIRect(x: xIndentC + xFieldOffset - xFieldRatio, w: xIndentC + xFieldWidth + xFieldRatio - 5), " ", style.colorHeight * 10f, titleGUI) / 10f, 0.6f, 9.8f);
            if (style.colorHeight != tempFloat) UpdatePalette();

            // Color Offset
            tempFloat = style.colorOffset;
            GUI.Label(GUIRect(xMargin: 0, x: xIndentB, down: 22, w: xIndentC), "Color Height", inspectorGUI);
            style.colorOffset = Mathf.Clamp(EditorGUI.FloatField(GUIRect(x: xIndentC + xFieldOffset - xFieldRatio, w: xIndentC + xFieldWidth + xFieldRatio - 5), " ", style.colorOffset * 10f, titleGUI) / 10f, 0f, 1.5f);
            if (style.colorOffset != tempFloat) UpdatePalette();

            // Font Size + Bold? Italic? Outline? Centre?
            GUI.Label(GUIRect(x: xIndentB, down: 22, w: xIndentC), "Font Size", inspectorGUI);
            unit.fontSize = EditorGUI.IntField(GUIRect(right: xIndentC, x: xFieldOffset - xFieldRatio, w: xIndentC + xFieldWidth + xFieldRatio - 5), " ", unit.fontSize, titleGUI);

            ToggleButtonColor(unit.fontBold);
            if (GUI.Button(GUIRect(right: 50, w: 40), "Bold", buttonGUI)) unit.fontBold = !unit.fontBold;

            ToggleButtonColor(unit.fontItalic);
            if (GUI.Button(GUIRect(right: 42), "Italic", buttonGUI)) unit.fontItalic = !unit.fontItalic;

            ToggleButtonColor(unit.hasOutline);
            if (GUI.Button(GUIRect(right: 42), "Outline", buttonGUI)) unit.hasOutline = !unit.hasOutline;

            ToggleButtonColor(unit.alignCentre);
            if (GUI.Button(GUIRect(right: 42), "Centre", buttonGUI)) unit.alignCentre = !unit.alignCentre;
            ResetGUI();

            // Max Width + Auto?
            GUI.Label(GUIRect(xMargin: 0, x: xIndentB, down: 22, w: xIndentC), "Max Width", inspectorGUI);
            unit.maxWidth = EditorGUI.IntField(GUIRect(x: xIndentC + xFieldOffset - xFieldRatio, w: xIndentC + xFieldWidth + xFieldRatio - 5), " ", unit.maxWidth, titleGUI);
            ToggleButtonColor(unit.autoWidth);
            if (GUI.Button(GUIRect(x: xIndentC + 50, w: 40), "Auto", buttonGUI)) unit.autoWidth = !unit.autoWidth;
            ResetGUI();

            // Comment Title
            GUI.Label(GUIRect(x: xIndentB, down: 33), "Title", inspectorGUI);
            unit.hasTitle = GUI.Toggle(GUIRect(x: xIndentC, w: 20), unit.hasTitle, "");
            if (unit.hasTitle) unit.title = GUI.TextField(GUIRect(x: xIndentC + 20, w: xWidth - xIndentC - 10, h: 16), unit.title, titleGUI);

            // Comment Contents
            var textHeight = commentGUI.CalcHeight(new GUIContent(unit.comment), xWidth - xIndentC + 10);
            GUI.Label(GUIRect(x: xIndentB, down: 22, w: xWidth, h: 18), "Comment", inspectorGUI);
            unit.comment = GUI.TextArea(GUIRect(x: xIndentC, w: xWidth - xIndentC + 10, h: textHeight + 2), unit.comment, commentGUI);


            ///////////////////////////   Section - Copy   /////////////////////////////////////////////

            // Header
            GUI.contentColor = new Color(1, 1, 0.6f);
            GUI.Label(GUIRect(right: xIndentA, down: textHeight + 2 - 18 + 33, w: xWidth, h: 20), "Copy Painter", sectionGUI);
            EditorGUI.DrawRect(GUIRect(down: 20, h: 1), new Color(0.5f, 0.5f, 0.5f, 1));
            ResetGUI();

            // Toggle Copy ACTIVE + show active at the top of inspector
            ToggleButtonColor(copyACTIVE, red: true);
            if (GUI.Button(GUIRect(xMargin: 0, x: xIndentB, down: 10, w: xIndentC - xIndentB - 10, h: 18), "ACTIVE", buttonGUI)) copyACTIVE = !copyACTIVE;
            if (copyACTIVE) if (GUI.Button(GUIRect(x: xIndentA + xWidth - 100, y: position.y - yCurrent, w: 100), "COPY ACTIVE", buttonGUI)) copyACTIVE = !copyACTIVE;
            ResetGUI();

            // If Copy is ACTIVE show other buttons
            if (copyACTIVE)
            {
                // All
                ToggleButtonColor(copyAll, alwaysVisible: true);
                if (GUI.Button(GUIRect(right: xIndentC, w: 50), "All", buttonGUI))
                {
                    copyAll = !copyAll;
                    copyCOLORS = copyLOCK = copyFONT = copyColor = copyFontColor = copyLockToPalette = copyLockToFontColor = copyFontSize = copyFontStyle = copyMaxWidth = copyTitle = copyContent = copyAll;
                }

                // Colors
                ToggleButtonColor(copyCOLORS, alwaysVisible: true);
                if (GUI.Button(GUIRect(down: 22), "Colors", buttonGUI))
                {
                    copyCOLORS = !copyCOLORS;
                    copyColor = copyFontColor = copyCOLORS;
                }

                ToggleButtonColor(copyColor);
                if (GUI.Button(GUIRect(right: 55), "Box", buttonGUI))
                    copyColor = !copyColor;

                ToggleButtonColor(copyFontColor);
                if (GUI.Button(GUIRect(right: 55), "Font", buttonGUI))
                    copyFontColor = !copyFontColor;

                // Locks
                ToggleButtonColor(copyLOCK, alwaysVisible: true);
                if (GUI.Button(GUIRect(xMargin: xIndentC, down: 22), "Locks", buttonGUI))
                {
                    copyLOCK = !copyLOCK;
                    copyLockToPalette = copyLockToFontColor = copyLOCK;
                }

                ToggleButtonColor(copyLockToPalette);
                if (GUI.Button(GUIRect(right: 55), "Palette", buttonGUI))
                    copyLockToPalette = !copyLockToPalette;

                ToggleButtonColor(copyLockToFontColor);
                if (GUI.Button(GUIRect(right: 55), "Colorize", buttonGUI))
                    copyLockToFontColor = !copyLockToFontColor;

                // Font
                ToggleButtonColor(copyFONT, alwaysVisible: true);
                if (GUI.Button(GUIRect(xMargin: xIndentC, down: 22), "Font", buttonGUI))
                {
                    copyFONT = !copyFONT;
                    copyFontSize = copyFontStyle = copyMaxWidth = copyFONT;
                }

                ToggleButtonColor(copyFontSize);
                if (GUI.Button(GUIRect(right: 55), "Size", buttonGUI))
                    copyFontSize = !copyFontSize;

                ToggleButtonColor(copyFontStyle);
                if (GUI.Button(GUIRect(right: 55), "Style", buttonGUI))
                    copyFontStyle = !copyFontStyle;

                ToggleButtonColor(copyMaxWidth);
                if (GUI.Button(GUIRect(right: 55), "Width", buttonGUI))
                    copyMaxWidth = !copyMaxWidth;

                // Text
                ToggleButtonColor(copyTEXT, alwaysVisible: true);
                if (GUI.Button(GUIRect(xMargin: xIndentC, down: 22), "Text", buttonGUI))
                    copyTEXT = !copyTEXT;

                ToggleButtonColor(copyTitle);
                if (GUI.Button(GUIRect(right: 55), "Title", buttonGUI))
                    copyTitle = !copyTitle;

                ToggleButtonColor(copyContent);
                if (GUI.Button(GUIRect(right: 55), "Content", buttonGUI))
                    copyContent = !copyContent;

                copyCOLORS = copyColor && copyFontColor;
                copyLOCK = copyLockToPalette && copyLockToFontColor;
                copyFONT = copyFontSize && copyFontStyle && copyMaxWidth;
                copyTEXT = copyTitle && copyContent;
                copyAll = copyCOLORS && copyLOCK && copyFONT && copyTEXT;
            }
            ResetGUI();

            // Copy the current unit to copyUnit, to use during copy
            copyUnit = unit;

            // EndBlock, record Undo, and write back unit details to the actual unit
            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = unit;
            }
        }
    }
}
