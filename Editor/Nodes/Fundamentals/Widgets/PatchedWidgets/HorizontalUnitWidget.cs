#if NEW_UNIT_UI && !ENABLE_VERTICAL_FLOW
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
namespace Unity.VisualScripting.Community
{
    public class UnitWidget<TUnit> : NodeWidget<FlowCanvas, TUnit>, IUnitWidget where TUnit : class, IUnit
    {
        public UnitWidget(FlowCanvas canvas, TUnit unit) : base(canvas, unit)
        {
            unit.onPortsChanged += CacheDefinition;
            unit.onPortsChanged += SubWidgetsChanged;
        }

        public override void Dispose()
        {
            base.Dispose();

            unit.onPortsChanged -= CacheDefinition;
            unit.onPortsChanged -= SubWidgetsChanged;
        }

        public override IEnumerable<IWidget> subWidgets => unit.ports.Select(port => canvas.Widget(port));

        #region Model

        protected TUnit unit => element;

        IUnit IUnitWidget.unit => unit;

        protected IUnitDebugData unitDebugData => GetDebugData<IUnitDebugData>();

        private UnitDescription description;

        private UnitAnalysis analysis => unit.Analysis<UnitAnalysis>(context);

        protected readonly List<IUnitPortWidget> ports = new List<IUnitPortWidget>();

        protected readonly List<IUnitPortWidget> inputs = new List<IUnitPortWidget>();

        protected readonly List<IUnitPortWidget> outputs = new List<IUnitPortWidget>();

        private readonly List<string> settingNames = new List<string>();

        protected readonly List<Metadata> settings = new List<Metadata>();


        protected override void CacheItemFirstTime()
        {
            base.CacheItemFirstTime();
            CacheDefinition();
        }

        protected virtual void CacheDefinition()
        {
            inputs.Clear();
            outputs.Clear();
            ports.Clear();
            inputs.AddRange(unit.inputs.Select(port => canvas.Widget<IUnitPortWidget>(port)));
            outputs.AddRange(unit.outputs.Select(port => canvas.Widget<IUnitPortWidget>(port)));
            ports.AddRange(inputs);
            ports.AddRange(outputs);

            Reposition();
        }

        protected override void CacheDescription()
        {
            description = unit.Description<UnitDescription>();

            titleContent.text = description.shortTitle;
            titleContent.tooltip = description.summary;
            surtitleContent.text = description.surtitle;
            subtitleContent.text = description.subtitle;

            Reposition();
        }

        protected override void CacheMetadata()
        {
            settingNames.Clear();

            settingNames.AddRange(metadata.valueType
                .GetMembers()
                .Where(mi => mi.HasAttribute<UnitHeaderInspectableAttribute>())
                .OrderBy(mi => mi.GetAttributes<Attribute>().OfType<IInspectableAttribute>().FirstOrDefault()?.order ?? int.MaxValue)
                .ThenBy(mi => mi.MetadataToken)
                .Select(mi => mi.Name));

            settings.Clear();

            foreach (var name in settingNames)
            {
                settings.Add(metadata[name]);
            }

            lock (settingLabelsContents)
            {
                settingLabelsContents.Clear();

                foreach (var setting in settings)
                {
                    var settingLabel = setting.GetAttribute<UnitHeaderInspectableAttribute>().label;

                    GUIContent settingContent;

                    if (string.IsNullOrEmpty(settingLabel))
                    {
                        settingContent = null;
                    }
                    else
                    {
                        settingContent = new GUIContent(settingLabel);
                    }

                    settingLabelsContents.Add(setting, settingContent);
                }
            }

            Reposition();
        }

        public virtual Inspector GetPortInspector(IUnitPort port, Metadata metadata)
        {
            return metadata.Inspector();
        }

        #endregion


        #region Lifecycle

        public override bool foregroundRequiresInput => showSettings || unit.valueInputs.Any(vip => vip.hasDefaultValue);

        protected virtual IEnumerable<IGraphItem> SnapTargets
        {
            get
            {
                foreach (var e in graph.elements)
                {
                    if (e != element && !(e is IUnitConnection))
                    {
                        yield return e;
                    }
                }
            }
        }

        private List<Rect> _cachedSnapTargets;

        public override void HandleInput()
        {
            if (AllowRectSnapping && isDragging && e.ctrlOrCmd)
            {
                if (_cachedSnapTargets == null)
                {
                    _cachedSnapTargets = new List<Rect>();
                    foreach (var target in SnapTargets)
                    {
                        _cachedSnapTargets.Add(SnapTarget(target));
                    }
                }

                var snapResult = RectUtility.CheckSnap(outerPosition, _cachedSnapTargets, threshold: 15f);
                snapLines.Clear();

                if (snapResult.snapped)
                {
                    var pos = BoltCore.Configuration.snapToGrid ? GraphGUI.SnapToGrid(snapResult.snapPosition) : snapResult.snapPosition;
                    _position = OuterToEdgePosition(new Rect(pos.x, pos.y, _position.width, _position.height));

                    if (snapResult.hasVerticalLine) snapLines.Add(snapResult.verticalLine);
                    if (snapResult.hasHorizontalLine) snapLines.Add(snapResult.horizontalLine);

                    Reposition();
                }
            }
            else
            {
                if (_cachedSnapTargets != null)
                {
                    _cachedSnapTargets = null;
                    snapLines.Clear();
                }
            }

            if (canvas.isCreatingConnection)
            {
                if (e.IsMouseDown(MouseButton.Left))
                {
                    var source = canvas.connectionSource;
                    var destination = source.CompatiblePort(unit);

                    if (destination != null)
                    {
                        UndoUtility.RecordEditedObject("Connect Nodes");
                        source.ValidlyConnectTo(destination);
                        canvas.connectionSource = null;
                        canvas.Widget(source.unit).Reposition();
                        canvas.Widget(destination.unit).Reposition();
                        GUI.changed = true;
                    }

                    e.Use();
                }
                else if (e.IsMouseDown(MouseButton.Right))
                {
                    canvas.CancelConnection();
                    e.Use();
                }
            }

            base.HandleInput();
        }

        private Rect SnapTarget(IGraphItem e)
        {
            if (e is Unit unit)
            {
                return canvas.Widget<INodeWidget>(unit).outerPosition;
            }
            return canvas.Widget(e).position;
        }
        #endregion


        #region Contents

        protected readonly GUIContent titleContent = new GUIContent();

        protected readonly GUIContent surtitleContent = new GUIContent();

        protected readonly GUIContent subtitleContent = new GUIContent();

        protected readonly Dictionary<Metadata, GUIContent> settingLabelsContents = new Dictionary<Metadata, GUIContent>();

        #endregion


        #region Positioning

        protected override bool snapToGrid => BoltCore.Configuration.snapToGrid;

        protected virtual Color? PortsbackgroundColor => null;

        public override IEnumerable<IWidget> positionDependers => ports.Cast<IWidget>();

        protected Rect _position;

        public override Rect position
        {
            get { return _position; }
            set { unit.position = value.position; }
        }

        public Rect titlePosition { get; protected set; }

        public Rect surtitlePosition { get; protected set; }

        public Rect subtitlePosition { get; protected set; }

        public Rect iconPosition { get; protected set; }

        public List<Rect> iconsPositions { get; protected set; } = new List<Rect>();

        public Dictionary<Metadata, Rect> settingsPositions { get; } = new Dictionary<Metadata, Rect>();

        public Rect headerAddonPosition { get; protected set; }

        public Rect portsBackgroundPosition { get; protected set; }

        public override void CachePosition()
        {
            List<Metadata> _settings = settings.ToList();
            float inputsWidth = 0f;
            for (int i = 0; i < inputs.Count; i++)
            {
                inputsWidth = Mathf.Max(inputsWidth, inputs[i].GetInnerWidth());
            }

            float outputsWidth = 0f;
            for (int i = 0; i < outputs.Count; i++)
            {
                outputsWidth = Mathf.Max(outputsWidth, outputs[i].GetInnerWidth());
            }

            float portsWidth = inputsWidth + Styles.spaceBetweenInputsAndOutputs + outputsWidth;

            settingsPositions.Clear();
            float settingsWidth = 0f;

            if (showSettings)
            {
                for (int i = 0; i < _settings.Count; i++)
                {
                    var setting = _settings[i];
                    float settingWidth = 0f;
                    var labelContent = settingLabelsContents[setting];

                    if (labelContent != null)
                    {
                        settingWidth += Styles.settingLabel.CalcSize(labelContent).x;
                    }

                    settingWidth += setting.Inspector().GetAdaptiveWidth();
                    settingWidth = Mathf.Min(settingWidth, Styles.maxSettingsWidth);

                    settingsPositions.Add(setting, new Rect(0, 0, settingWidth, 0));
                    settingsWidth = Mathf.Max(settingsWidth, settingWidth);
                }
            }

            float headerAddonWidth = showHeaderAddon ? GetHeaderAddonWidth() : 0f;
            float headerTextWidth = Styles.title.CalcSize(titleContent).x;

            if (showSurtitle)
                headerTextWidth = Mathf.Max(headerTextWidth, Styles.surtitle.CalcSize(surtitleContent).x);

            if (showSubtitle)
                headerTextWidth = Mathf.Max(headerTextWidth, Styles.subtitle.CalcSize(subtitleContent).x);

            float iconsWidth = 0f;
            if (showIcons && description.icons.Length > 0)
            {
                int iconsColumns = Mathf.CeilToInt((float)description.icons.Length / Styles.iconsPerColumn);
                iconsWidth = (iconsColumns * Styles.iconsSize) + ((iconsColumns - 1) * Styles.iconsSpacing);
            }

            float headerWidth = Mathf.Max(headerTextWidth + iconsWidth, Mathf.Max(settingsWidth, headerAddonWidth))
                            + Styles.iconSize + Styles.spaceAfterIcon;

            float innerWidth = Mathf.Max(portsWidth, headerWidth);
            float edgeWidth = InnerToEdgePosition(new Rect(0, 0, innerWidth, 0)).width;

            Vector2 edgeOrigin = unit.position;
            float edgeX = edgeOrigin.x;
            float edgeY = edgeOrigin.y;

            Vector2 innerOrigin = EdgeToInnerPosition(new Rect(edgeOrigin, Vector2.zero)).position;
            float innerX = innerOrigin.x;
            float innerY = innerOrigin.y;

            iconPosition = new Rect(innerX, innerY, Styles.iconSize, Styles.iconSize);
            float headerTextX = iconPosition.xMax + Styles.spaceAfterIcon;

            float y = innerY;
            float headerHeight = 0f;

            if (showSurtitle)
            {
                float h = Styles.surtitle.CalcHeight(surtitleContent, headerTextWidth);
                surtitlePosition = new Rect(headerTextX, y, headerTextWidth, h);
                float step = h + Styles.spaceAfterSurtitle;
                headerHeight += step;
                y += step;
            }

            if (showTitle)
            {
                float h = Styles.title.CalcHeight(titleContent, headerTextWidth);
                titlePosition = new Rect(headerTextX, y, headerTextWidth, h);
                headerHeight += h;
                y += h;
            }

            if (showSubtitle)
            {
                headerHeight += Styles.spaceBeforeSubtitle;
                y += Styles.spaceBeforeSubtitle;
                float h = Styles.subtitle.CalcHeight(subtitleContent, headerTextWidth);
                subtitlePosition = new Rect(headerTextX, y, headerTextWidth, h);
                headerHeight += h;
                y += h;
            }

            iconsPositions.Clear();
            if (showIcons)
            {
                int row = 0, col = 0;
                for (int i = 0; i < description.icons.Length; i++)
                {
                    iconsPositions.Add(new Rect(
                        innerX + innerWidth - ((col + 1) * Styles.iconsSize) - (col * Styles.iconsSpacing),
                        innerY + (row * (Styles.iconsSize + Styles.iconsSpacing)),
                        Styles.iconsSize, Styles.iconsSize));

                    if (++row % Styles.iconsPerColumn == 0) { col++; row = 0; }
                }
            }

            if (showSettings && _settings.Count > 0)
            {
                headerHeight += Styles.spaceBeforeSettings;
                float settingsTotalHeight = 0f;

                for (int i = 0; i < _settings.Count; i++)
                {
                    var setting = _settings[i];
                    float sWidth = settingsPositions[setting].width;
                    using (LudiqGUIUtility.currentInspectorWidth.Override(sWidth))
                    {
                        float sHeight = LudiqGUI.GetInspectorHeight(null, setting, sWidth, settingLabelsContents[setting] ?? GUIContent.none);
                        settingsPositions[setting] = new Rect(headerTextX, y, sWidth, sHeight);

                        float step = sHeight + Styles.spaceBetweenSettings;
                        settingsTotalHeight += step;
                        y += step;
                    }
                }

                settingsTotalHeight -= Styles.spaceBetweenSettings;
                y -= Styles.spaceBetweenSettings;
                headerHeight += settingsTotalHeight + Styles.spaceAfterSettings;
                y += Styles.spaceAfterSettings;
            }

            if (showHeaderAddon)
            {
                float h = GetHeaderAddonHeight(headerAddonWidth);
                headerAddonPosition = new Rect(headerTextX, y, headerAddonWidth, h);
                headerHeight += h;
                y += h;
            }

            if (headerHeight < Styles.iconSize)
            {
                float centeringOffset = (Styles.iconSize - headerHeight) * 0.5f;
                if (showTitle) titlePosition = new Rect(titlePosition.x, titlePosition.y + centeringOffset, titlePosition.width, titlePosition.height);
                if (showSubtitle) subtitlePosition = new Rect(subtitlePosition.x, subtitlePosition.y + centeringOffset, subtitlePosition.width, subtitlePosition.height);
                if (showHeaderAddon) headerAddonPosition = new Rect(headerAddonPosition.x, headerAddonPosition.y + centeringOffset, headerAddonPosition.width, headerAddonPosition.height);

                if (showSettings)
                {
                    for (int i = 0; i < _settings.Count; i++)
                    {
                        var rect = settingsPositions[_settings[i]];
                        rect.y += centeringOffset;
                        settingsPositions[_settings[i]] = rect;
                    }
                }
                headerHeight = Styles.iconSize;
            }

            y = innerY + headerHeight;
            float innerHeight = headerHeight;

            if (showPorts)
            {
                innerHeight += Styles.spaceBeforePorts;
                y += Styles.spaceBeforePorts;

                float portsBackgroundY = y;
                float portsPaddingTop = Styles.portsBackground.padding.top;
                y += portsPaddingTop;
                innerHeight += portsPaddingTop;

                float portStartY = y;
                float inH = 0f, outH = 0f;

                for (int i = 0; i < inputs.Count; i++)
                {
                    inputs[i].y = y;
                    float h = inputs[i].GetHeight();
                    inH += h + Styles.spaceBetweenPorts;
                    y += h + Styles.spaceBetweenPorts;
                }
                if (inputs.Count > 0) inH -= Styles.spaceBetweenPorts;

                y = portStartY;
                for (int i = 0; i < outputs.Count; i++)
                {
                    outputs[i].y = y;
                    float h = outputs[i].GetHeight();
                    outH += h + Styles.spaceBetweenPorts;
                    y += h + Styles.spaceBetweenPorts;
                }
                if (outputs.Count > 0) outH -= Styles.spaceBetweenPorts;

                float maxPortsH = Mathf.Max(inH, outH);
                innerHeight += maxPortsH + Styles.portsBackground.padding.bottom;

                portsBackgroundPosition = new Rect(edgeX, portsBackgroundY, edgeWidth, maxPortsH + portsPaddingTop + Styles.portsBackground.padding.bottom);
            }

            float finalEdgeHeight = InnerToEdgePosition(new Rect(0, 0, 0, innerHeight)).height;
            _position = new Rect(edgeX, edgeY, edgeWidth, finalEdgeHeight);
        }

        protected virtual float GetHeaderAddonWidth()
        {
            return 0;
        }

        protected virtual float GetHeaderAddonHeight(float width)
        {
            return 0;
        }

        #endregion


        #region Drawing

        protected virtual NodeColorMix baseColor => NodeColor.Gray;

        protected override NodeColorMix color
        {
            get
            {
                if (unitDebugData.runtimeException != null)
                {
                    return NodeColor.Red;
                }

                var color = baseColor;

                if (analysis.warnings.Count > 0)
                {
                    var mostSevereWarning = Warning.MostSevereLevel(analysis.warnings);

                    switch (mostSevereWarning)
                    {
                        case WarningLevel.Error:
                            color = NodeColor.Red;
                            break;

                        case WarningLevel.Severe:
                            color = NodeColor.Orange;
                            break;

                        case WarningLevel.Caution:
                            color = NodeColor.Yellow;

                            break;
                    }
                }

                if (EditorApplication.isPaused)
                {
                    if (EditorTimeBinding.frame == unitDebugData.lastInvokeFrame)
                    {
                        return NodeColor.Blue;
                    }
                }
                else
                {
                    var mix = color;
                    mix.blue = Mathf.Lerp(1, 0, (EditorTimeBinding.time - unitDebugData.lastInvokeTime) / Styles.invokeFadeDuration);

                    return mix;
                }

                return color;
            }
        }

        protected override NodeShape shape => NodeShape.Square;

        protected virtual bool showTitle => !string.IsNullOrEmpty(description.shortTitle);

        protected virtual bool showSurtitle => !string.IsNullOrEmpty(description.surtitle);

        protected virtual bool showSubtitle => !string.IsNullOrEmpty(description.subtitle);

        protected virtual bool showIcons => description.icons.Length > 0;

        protected virtual bool showSettings => settingNames.Count > 0;

        protected virtual bool showHeaderAddon => false;

        protected virtual bool showPorts => ports.Count > 0;

        protected override bool dim
        {
            get
            {
                var dim = BoltCore.Configuration.dimInactiveNodes && !analysis.isEntered;

                if (isMouseOver || isSelected)
                {
                    dim = false;
                }

                if (BoltCore.Configuration.dimIncompatibleNodes && canvas.isCreatingConnection)
                {
                    dim = !unit.ports.Any(p => canvas.connectionSource == p || canvas.connectionSource.CanValidlyConnectTo(p));
                }

                return dim;
            }
        }

        protected virtual bool AllowRectSnapping => true;
        private List<RectUtility.SnapLine> snapLines = new List<RectUtility.SnapLine>();

        protected void DrawSnapLines()
        {
            if (snapLines == null || snapLines.Count == 0)
                return;

            Handles.color = new Color32(64, 113, 156, 255);
            foreach (var line in snapLines)
            {
                Handles.DrawLine(line.start, line.end);
            }
        }

        private void ConvertToEmbed()
        {
            NodeSelection.Convert(GraphSource.Embed);
        }

        private void ConvertToMacro()
        {
            NodeSelection.Convert(GraphSource.Macro);
        }

        public override void DrawForeground()
        {
            if (AllowRectSnapping && isDragging && e.ctrlOrCmd)
                DrawSnapLines();

            BeginDim();

            base.DrawForeground();

            DrawIcon();

            if (showSurtitle)
            {
                DrawSurtitle();
            }

            if (showTitle)
            {
                DrawTitle();
            }

            if (showSubtitle)
            {
                DrawSubtitle();
            }

            if (showIcons)
            {
                DrawIcons();
            }

            if (showSettings)
            {
                DrawSettings();
            }

            if (showHeaderAddon)
            {
                DrawHeaderAddon();
            }

            if (showPorts)
            {
                DrawPortsBackground();
            }

            EndDim();
        }

        protected void DrawIcon()
        {
            var icon = description.icon ?? BoltFlow.Icons.unit;

            if (icon != null && icon[(int)iconPosition.width])
            {
                GUI.DrawTexture(iconPosition, icon[(int)iconPosition.width]);
            }
        }

        protected void DrawTitle()
        {
            GUI.Label(titlePosition, titleContent, invertForeground ? Styles.titleInverted : Styles.title);
        }

        protected void DrawSurtitle()
        {
            GUI.Label(surtitlePosition, surtitleContent, invertForeground ? Styles.surtitleInverted : Styles.surtitle);
        }

        protected void DrawSubtitle()
        {
            GUI.Label(subtitlePosition, subtitleContent, invertForeground ? Styles.subtitleInverted : Styles.subtitle);
        }

        protected void DrawIcons()
        {
            for (int i = 0; i < description.icons.Length; i++)
            {
                var icon = description.icons[i];
                var position = iconsPositions[i];

                GUI.DrawTexture(position, icon?[(int)position.width]);
            }
        }

        private void DrawSettings()
        {
            if (graph.zoom < FlowCanvas.inspectorZoomThreshold)
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(!e.IsRepaint && isMouseThrough && !isMouseOver);

            EditorGUI.BeginChangeCheck();

            foreach (var setting in settings)
            {
                DrawSetting(setting);
            }

            if (EditorGUI.EndChangeCheck())
            {
                unit.Define();
                Reposition();
            }

            EditorGUI.EndDisabledGroup();
        }

        protected void DrawSetting(Metadata setting)
        {
            var settingPosition = settingsPositions[setting];

            using (LudiqGUIUtility.currentInspectorWidth.Override(settingPosition.width))
            using (Inspector.expandTooltip.Override(false))
            {
                var label = settingLabelsContents[setting];

                if (label == null)
                {
                    LudiqGUI.Inspector(setting, settingPosition, GUIContent.none);
                }
                else
                {
                    using (Inspector.defaultLabelStyle.Override(Styles.settingLabel))
                    using (LudiqGUIUtility.labelWidth.Override(Styles.settingLabel.CalcSize(label).x))
                    {
                        LudiqGUI.Inspector(setting, settingPosition, label);
                    }
                }
            }
        }

        protected virtual void DrawHeaderAddon() { }

        private IUnitPortWidget Single(IUnitPort targetPort)
        {
            IUnitPortWidget widget = null;
            for (int i = 0; i < ports.Count; i++)
            {
                if (ports[i].port == targetPort)
                {
                    widget = ports[i];
                    break;
                }
            }
            return widget;
        }

        protected void DrawPortsBackground()
        {
            if (canvas.showRelations)
            {
                foreach (var relation in unit.relations)
                {
                    var sourcePort = relation.source;
                    var destinationPort = relation.destination;
                    IUnitPortWidget sourceWidget = Single(sourcePort);
                    IUnitPortWidget destinationWidget = Single(destinationPort);
                    var start = sourceWidget.handlePosition.center;
                    var end = destinationWidget.handlePosition.center;

                    var startTangent = start;
                    var endTangent = end;

                    if (relation.source is IUnitInputPort &&
                        relation.destination is IUnitInputPort)
                    {
                        startTangent -= new Vector2(20, 0);
                        endTangent -= new Vector2(32, 0);
                    }
                    else
                    {
                        startTangent += new Vector2(innerPosition.width / 2, 0);
                        endTangent += new Vector2(-innerPosition.width / 2, 0);
                    }

                    Handles.DrawBezier
                        (
                            start,
                            end,
                            startTangent,
                            endTangent,
                            ColorPalette.unityBackgroundMid,
                            null,
                            3
                        );
                }
            }
            else
            {
                if (e.IsRepaint)
                {
                    Styles.portsBackground.Draw(portsBackgroundPosition, false, false, false, false);
                }
            }
        }

        #endregion

        #region Selecting

        public override bool canSelect => true;

        #endregion


        #region Dragging

        public override bool canDrag => true;

        public override void ExpandDragGroup(HashSet<IGraphElement> dragGroup)
        {
            if (BoltCore.Configuration.carryChildren)
            {
                foreach (var output in unit.outputs)
                {
                    foreach (var connection in output.connections)
                    {
                        if (dragGroup.Contains(connection.destination.unit))
                        {
                            continue;
                        }

                        dragGroup.Add(connection.destination.unit);

                        canvas.Widget(connection.destination.unit).ExpandDragGroup(dragGroup);
                    }
                }
            }
        }

        #endregion


        #region Deleting

        public override bool canDelete => true;

        #endregion


        #region Clipboard

        public override void ExpandCopyGroup(HashSet<IGraphElement> copyGroup)
        {
            copyGroup.UnionWith(unit.connections.Cast<IGraphElement>());
        }

        #endregion


        #region Context

        protected override IEnumerable<DropdownOption> contextOptions
        {
            get
            {
                yield return new DropdownOption((Action)ReplaceUnit, "Replace...");

                foreach (var baseOption in base.contextOptions)
                {
                    yield return baseOption;
                }

                if (selection.Count > 0)
                {
                    yield return new DropdownOption((Action)ConvertToEmbed, "Selection/To Embed Subgraph");
                    yield return new DropdownOption((Action)ConvertToMacro, "Selection/To Macro Subgraph");
                }
            }
        }

        private void ReplaceUnit()
        {
            UnitWidgetHelper.ReplaceUnit(unit, reference, context, selection, e);
        }

        #endregion


        public static class Styles
        {
            static Styles()
            {
                // Disabling word wrap because Unity's CalcSize and CalcHeight
                // are broken w.r.t. pixel-perfection and matrix

                title = new GUIStyle(BoltCore.Styles.nodeLabel);
                title.padding = new RectOffset(0, 5, 0, 2);
                title.margin = new RectOffset(0, 0, 0, 0);
                title.fontSize = 12;
                title.alignment = TextAnchor.MiddleLeft;
                title.wordWrap = false;

                surtitle = new GUIStyle(BoltCore.Styles.nodeLabel);
                surtitle.padding = new RectOffset(0, 5, 0, 0);
                surtitle.margin = new RectOffset(0, 0, 0, 0);
                surtitle.fontSize = 10;
                surtitle.alignment = TextAnchor.MiddleLeft;
                surtitle.wordWrap = false;

                subtitle = new GUIStyle(surtitle);
                subtitle.padding.bottom = 2;

                titleInverted = new GUIStyle(title);
                titleInverted.normal.textColor = ColorPalette.unityBackgroundDark;

                surtitleInverted = new GUIStyle(surtitle);
                surtitleInverted.normal.textColor = ColorPalette.unityBackgroundDark;

                subtitleInverted = new GUIStyle(subtitle);
                subtitleInverted.normal.textColor = ColorPalette.unityBackgroundDark;

#if NEW_UNIT_STYLE
                if (EditorGUIUtility.isProSkin)
                {
                    portsBackground = new GUIStyle
                    {
                        padding = new RectOffset(0, 0, 6, 5),
                        border = new RectOffset(0, 0, 2, 2)
                    };

                    portsBackground.normal.background = CommunityStyles.MakeBorderedTexture(CommunityStyles.backgroundColor, CommunityStyles.backgroundColor.Darken(0.05f));
                }
                else
                {
                    portsBackground = new GUIStyle
                    {
                        normal = { background = CommunityStyles.MakeBorderedTexture(CommunityStyles.backgroundColor, CommunityStyles.backgroundColor.Brighten(0.05f)) },
                        padding = new RectOffset(0, 0, 6, 5)
                    };
                }
#else
                portsBackground = VisualScripting.UnitWidget<Unit>.Styles.portsBackground;
#endif

                settingLabel = new GUIStyle(BoltCore.Styles.nodeLabel);
                settingLabel.padding.left = 0;
                settingLabel.padding.right = 5;
                settingLabel.wordWrap = false;
                settingLabel.clipping = TextClipping.Clip;
            }

            public static readonly GUIStyle title;

            public static readonly GUIStyle surtitle;

            public static readonly GUIStyle subtitle;

            public static readonly GUIStyle titleInverted;

            public static readonly GUIStyle surtitleInverted;

            public static readonly GUIStyle subtitleInverted;

            public static readonly GUIStyle settingLabel;

            public static readonly float spaceAroundLineIcon = 5;

            public static readonly float spaceBeforePorts = 5;

            public static readonly float spaceBetweenInputsAndOutputs = 8;

            public static readonly float spaceBeforeSettings = 2;

            public static readonly float spaceBetweenSettings = 3;

            public static readonly float spaceBetweenPorts = 3;

            public static readonly float spaceAfterSettings = 0;

            public static readonly float maxSettingsWidth = 150;

            public static readonly GUIStyle portsBackground;

            public static readonly float iconSize = IconSize.Medium;

            public static readonly float iconsSize = IconSize.Small;

            public static readonly float iconsSpacing = 3;

            public static readonly int iconsPerColumn = 2;

            public static readonly float spaceAfterIcon = 6;

            public static readonly float spaceAfterSurtitle = 2;

            public static readonly float spaceBeforeSubtitle = 0;

            public static readonly float invokeFadeDuration = 0.5f;
        }
    }
}
#endif