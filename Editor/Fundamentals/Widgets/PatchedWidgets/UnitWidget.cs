#if NEW_UNIT_UI
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if ENABLE_VERTICAL_FLOW
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

        protected IEnumerable<Metadata> settings
        {
            get
            {
                foreach (var settingName in settingNames)
                {
                    yield return metadata[settingName];
                }
            }
        }

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

        public override void HandleInput()
        {
            if (isDragging && e.ctrlOrCmd)
            {
                List<Rect> otherRects = graph.elements.Where(e => e != element && !(e is IUnitConnection))
                    .Select(e => e is Unit unit ? canvas.Widget<INodeWidget>(unit).outerPosition : canvas.Widget(e).position)
                    .ToList();

                var snapResult = RectUtility.CheckSnap(outerPosition, otherRects, threshold: 15f);

                if (snapResult.snapped)
                {
                    var pos = GraphGUI.SnapToGrid(snapResult.snapPosition);
                    _position = OuterToEdgePosition(new Rect(pos.x, pos.y, _position.width, _position.height));
                    snapLines = snapResult.snapLines;
                    Reposition();
                }
                else
                {
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

        #endregion


        #region Contents

        protected readonly GUIContent titleContent = new GUIContent();

        protected readonly GUIContent surtitleContent = new GUIContent();

        protected readonly GUIContent subtitleContent = new GUIContent();

        protected readonly Dictionary<Metadata, GUIContent> settingLabelsContents = new Dictionary<Metadata, GUIContent>();

        #endregion


        #region Positioning

        protected override bool snapToGrid => BoltCore.Configuration.snapToGrid;

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
            const float compactY = 0.5f;
            const float compactX = 0.8f;

            // TODO: Make invalid control ports vertical.
            var valueInputs = inputs
                .Where(p =>
                    p is ValueInputWidget ||
                    p is InvalidInputWidget)
                .Cast<IUnitPortWidget>();

            var valueOutputs = outputs
                .Where(p =>
                    p is ValueOutputWidget ||
                    p is InvalidOutputWidget)
                .Cast<IUnitPortWidget>();

            var controlInputs = inputs.OfType<ControlInputWidget>().ToList();
            var controlOutputs = outputs.OfType<ControlOutputWidget>().ToList();

            var valueInputsWidth = valueInputs.Any() ? valueInputs.Max(p => p.GetInnerWidth()) : 0f;
            var outputsWidth = valueOutputs.Any() ? valueOutputs.Max(p => p.GetInnerWidth()) : 0f;

            var portsWidth = valueInputsWidth + Styles.spaceBetweenInputsAndOutputs + outputsWidth;

            const float spaceBetweenControlPorts = 10;

            portsWidth = Mathf.Max(portsWidth, (controlInputs.Any() ? controlInputs.Max(p => p.GetInnerWidth()) + spaceBetweenControlPorts : 0f) * controlInputs.Count);
            portsWidth = Mathf.Max(portsWidth, (controlOutputs.Any() ? controlOutputs.Max(p => p.GetInnerWidth()) + spaceBetweenControlPorts : 0f) * controlOutputs.Count);

            settingsPositions.Clear();
            var settingsWidth = 0f;

            if (showSettings)
            {
                foreach (var setting in settings)
                {
                    var settingLabelContent = settingLabelsContents[setting];
                    var settingWidth = 0f;

                    if (settingLabelContent != null)
                        settingWidth += Styles.settingLabel.CalcSize(settingLabelContent).x;

                    settingWidth += setting.Inspector().GetAdaptiveWidth();
                    settingWidth = Mathf.Min(settingWidth, Styles.maxSettingsWidth);

                    settingsPositions.Add(setting, new Rect(0, 0, settingWidth, 0));
                    settingsWidth = Mathf.Max(settingsWidth, settingWidth);
                }
            }

            var headerAddonWidth = showHeaderAddon ? GetHeaderAddonWidth() : 0f;
            var titleWidth = Styles.title.CalcSize(titleContent).x;
            var headerTextWidth = titleWidth;

            if (showSurtitle)
                headerTextWidth = Mathf.Max(headerTextWidth, Styles.surtitle.CalcSize(surtitleContent).x);
            if (showSubtitle)
                headerTextWidth = Mathf.Max(headerTextWidth, Styles.subtitle.CalcSize(subtitleContent).x);

            var iconsWidth = 0f;
            if (showIcons)
            {
                var iconsColumns = Mathf.Ceil((float)description.icons.Length / Styles.iconsPerColumn);
                iconsWidth = iconsColumns * Styles.iconsSize + ((iconsColumns - 1) * Styles.iconsSpacing * compactX);
            }

            var headerWidth = Mathf.Max(headerTextWidth + iconsWidth,
                Mathf.Max(settingsWidth, headerAddonWidth)) + Styles.iconSize + (Styles.spaceAfterIcon * compactX);

            var innerWidth = Mathf.Max(portsWidth, headerWidth);
            var edgeWidth = InnerToEdgePosition(new Rect(0, 0, innerWidth, 0)).width;

            var edgeOrigin = unit.position;
            var edgeX = edgeOrigin.x;
            var edgeY = edgeOrigin.y;

            var innerOrigin = EdgeToInnerPosition(new Rect(edgeOrigin, Vector2.zero)).position;
            var innerX = innerOrigin.x;
            var innerY = innerOrigin.y;

            var y = innerY;
            var headerHeight = 0f;

#if NEW_UNIT_STYLE
            var controlInputsHeight = controlInputs.Any(c => c.showLabel) ? controlInputs.Max(p => p.GetHeight()) : 0f;
#else
            var controlInputsHeight = controlInputs.Any(c => c.showLabel) ? controlInputs.Max(p => p.GetHeight()) + 4 : 0f;
#endif
            headerHeight += controlInputsHeight;
            y += controlInputsHeight;

            iconPosition = new Rect(innerX, y, Styles.iconSize, Styles.iconSize);
            var headerTextX = iconPosition.xMax + Styles.spaceAfterIcon * compactX;

            if (showSurtitle)
            {
                var h = Styles.surtitle.CalcHeight(surtitleContent, headerTextWidth);
                surtitlePosition = new Rect(headerTextX, y, headerTextWidth, h);
                headerHeight += h + Styles.spaceAfterSurtitle * compactY;
                y += h + Styles.spaceAfterSurtitle * compactY;
            }

            if (showTitle)
            {
                var h = Styles.title.CalcHeight(titleContent, headerTextWidth);
                titlePosition = new Rect(headerTextX, y, headerTextWidth, h);
                headerHeight += h;
                y += h;
            }

            if (showSubtitle)
            {
                headerHeight += Styles.spaceBeforeSubtitle * compactY;
                y += Styles.spaceBeforeSubtitle * compactY;

                var h = Styles.subtitle.CalcHeight(subtitleContent, headerTextWidth);
                subtitlePosition = new Rect(headerTextX, y, headerTextWidth, h);
                headerHeight += h;
                y += h;
            }

            iconsPositions.Clear();

            if (showIcons)
            {
                var iconRow = 0;
                var iconCol = 0;

                for (int i = 0; i < description.icons.Length; i++)
                {
                    var iconPosition = new Rect
                        (
                        innerX + innerWidth - ((iconCol + 1) * Styles.iconsSize) - ((iconCol) * Styles.iconsSpacing),
                        innerY + (iconRow * (Styles.iconsSize + Styles.iconsSpacing)),
                        Styles.iconsSize,
                        Styles.iconsSize
                        );

                    iconsPositions.Add(iconPosition);

                    iconRow++;

                    if (iconRow % Styles.iconsPerColumn == 0)
                    {
                        iconCol++;
                        iconRow = 0;
                    }
                }
            }

            if (showSettings)
            {
                headerHeight += Styles.spaceBeforeSettings * compactY;
                y += Styles.spaceBeforeSettings * compactY;
                var last = settings.Last();
                foreach (var setting in settings)
                {
                    var settingWidth = settingsPositions[setting].width;
                    using (LudiqGUIUtility.currentInspectorWidth.Override(settingWidth))
                    {
                        var settingHeight = LudiqGUI.GetInspectorHeight(null, setting, settingWidth, settingLabelsContents[setting] ?? GUIContent.none);
                        settingsPositions[setting] = new Rect(headerTextX, y, settingWidth, settingHeight);
                        if (setting != last)
                        {
                            y += settingHeight + Styles.spaceBetweenSettings * compactY;
                            headerHeight += settingHeight + Styles.spaceBetweenSettings * compactY;
                        }
                        else
                        {

                            y += settingHeight * compactY;
                            headerHeight += settingHeight * compactY;
                        }
                    }
                }
            }

            if (showHeaderAddon)
            {
                var addonHeight = GetHeaderAddonHeight(headerAddonWidth);
                headerAddonPosition = new Rect(headerTextX, y, headerAddonWidth, addonHeight);
                y += addonHeight;
                headerHeight += addonHeight;
            }

            headerHeight = Mathf.Max(headerHeight, Styles.iconSize * 0.7f);

            y = innerY + headerHeight + Styles.spaceBeforePorts;
            var innerHeight = headerHeight;

            var controlOutputsHeight = 0f;
            if (showPorts)
            {
                bool hasValuePorts = ports.Any(p => p.port is ValueInput or ValueOutput or InvalidInput or InvalidOutput);

                if (hasValuePorts)
                {
                    innerHeight += Styles.spaceBeforePorts * compactY;
                    y += Styles.spaceBeforePorts * compactY;
                }

                float portsBackgroundY = y;
                float portsBackgroundHeight = hasValuePorts ? Styles.portsBackground.padding.top * compactY : 0f;
                y += portsBackgroundHeight;

                var portStartY = y;

                float inputsHeight = 0f;
                foreach (var input in inputs)
                {
                    if (input is ControlInputWidget) continue;
                    float h = input.GetHeight();
                    input.y = y;
                    y += h + Styles.spaceBetweenPorts * compactY;
                    inputsHeight += h + Styles.spaceBetweenPorts * compactY;
                }

                float outputsHeight = 0f;
                foreach (var output in outputs)
                {
                    if (output is ControlOutputWidget) continue;
                    float h = output.GetHeight();
                    output.y = portStartY + outputsHeight;
                    outputsHeight += h + Styles.spaceBetweenPorts * compactY;
                }

                float portsHeight = Mathf.Max(inputsHeight, outputsHeight);
                if (hasValuePorts)
                {
                    portsBackgroundHeight += portsHeight + Styles.portsBackground.padding.bottom * compactY;
                    innerHeight += portsHeight + Styles.portsBackground.padding.bottom * compactY;
                }

                if (controlInputs.Count > 0)
                {
                    int portCount = controlInputs.Count;

                    float controlY = edgeY - Styles.spaceBeforePorts - Styles.spaceAfterControlInputs;

                    float totalSlotSpace = EdgeToOuterPosition(new Rect(edgeX, portsBackgroundY, edgeWidth, portsBackgroundHeight)).width;
                    float slotWidth = totalSlotSpace / portCount;

                    for (int i = 0; i < portCount; i++)
                    {
                        var widget = controlInputs[i];
                        float slotCenter = edgeX + (slotWidth * (i + 0.5f)) - Styles.spaceBeforePorts;
                        widget.x = slotCenter;
                        widget.y = controlY;
                    }
                }

                if (controlOutputs.Count > 0)
                {
                    int portCount = controlOutputs.Count;

                    float maxHeight = 0f;
                    foreach (var widget in controlOutputs)
                    {
                        maxHeight = Mathf.Max(maxHeight, widget.GetHeight());
                    }

                    controlOutputsHeight = maxHeight + 3;

                    float controlY = innerY + innerHeight + Styles.spaceBeforePorts + controlOutputsHeight + Styles.spaceBeforeControlOutputs;

                    float totalSlotSpace = EdgeToOuterPosition(new Rect(edgeX, portsBackgroundY, edgeWidth, portsBackgroundHeight)).width;
                    float slotWidth = totalSlotSpace / portCount;

                    for (int i = 0; i < portCount; i++)
                    {
                        var widget = controlOutputs[i];
                        float slotCenter = edgeX + (slotWidth * (i + 0.5f)) - Styles.spaceBeforePorts;
                        widget.x = slotCenter;
                        widget.y = controlY;
                    }
                }
                portsBackgroundPosition = new Rect(edgeX, portsBackgroundY, edgeWidth, portsBackgroundHeight);
            }

            var edgeHeight = InnerToEdgePosition(new Rect(0, 0, 0, innerHeight)).height;
            _position = new Rect(edgeX, edgeY, edgeWidth, edgeHeight + controlOutputsHeight);
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
        private List<RectUtility.SnapResult.Line> snapLines = new List<RectUtility.SnapResult.Line>();

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
        public override void DrawOverlay()
        {
            base.DrawOverlay();
            if (isDragging && e.ctrlOrCmd)
                DrawSnapLines();
        }
        public override void DrawForeground()
        {
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

        protected void DrawPortsBackground()
        {
            if (canvas.showRelations)
            {
                foreach (var relation in unit.relations)
                {
                    var start = ports.Single(pw => pw.port == relation.source).handlePosition.center;
                    var end = ports.Single(pw => pw.port == relation.destination).handlePosition.center;

                    var startTangent = start;
                    var endTangent = end;

                    float distance = Vector2.Distance(start, end);
                    float offset = Mathf.Min(distance * 0.5f, 50f);

                    if (relation.source is ControlOutput && relation.destination is ControlInput)
                    {
                        startTangent += new Vector2(0, offset);
                        endTangent += new Vector2(0, -offset);
                    }
                    else if (relation.source is ControlInput && relation.destination is ValueInput)
                    {
                        startTangent += new Vector2(0, offset);
                        endTangent += new Vector2(-offset, 0);
                    }
                    else if (relation.source is ValueOutput && relation.destination is ValueInput)
                    {
                        startTangent += new Vector2(offset, 0);
                        endTangent += new Vector2(-offset, 0);
                    }
                    else if (relation.source is ValueInput && relation.destination is ValueOutput)
                    {
                        startTangent += new Vector2(-offset, 0);
                        endTangent += new Vector2(offset, 0);
                    }
                    else
                    {
                        startTangent += new Vector2(0, offset);
                        endTangent += new Vector2(0, -offset);
                    }

                    Handles.DrawBezier(
                        start,
                        end,
                        startTangent,
                        endTangent,
                        CommunityStyles.backgroundColor,
                        null,
                        3
                    );
                }
            }
            else
            {
                if (e.IsRepaint && ports.Count(p => p.port is ValueInput or ValueOutput) > 0)
                {
#if NEW_UNIT_STYLE
                    var isSpecial = color.orange == new NodeColorMix(NodeColor.Orange).orange || color.red == new NodeColorMix(NodeColor.Red).red;

                    var previous = GUI.backgroundColor;

                    if (isSpecial)
                    {
                        GUI.backgroundColor = color.ToColor();
                    }

                    Styles.portsBackground.Draw(portsBackgroundPosition, false, false, false, false);

                    if (isSpecial)
                        GUI.backgroundColor = previous;
#else
                    Styles.portsBackground.Draw(portsBackgroundPosition, false, false, false, false);
#endif
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

        private void ConvertToEmbed()
        {
            NodeSelection.Convert(GraphSource.Embed);
        }

        private void ConvertToMacro()
        {
            NodeSelection.Convert(GraphSource.Macro);
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

            public static readonly float spaceBeforePorts = 8;
#if NEW_UNIT_STYLE
            public static readonly float spaceBeforeControlOutputs = 10;

            public static readonly float spaceAfterControlInputs = 23;
#else
            public static readonly float spaceBeforeControlOutputs = 5;
            public static readonly float spaceAfterControlInputs = 17;
#endif
            public static readonly float spaceBetweenInputsAndOutputs = 8;

            public static readonly float spaceBeforeSettings = 2;

            public static readonly float spaceBetweenSettings = 3;

            public static readonly float spaceBetweenPorts = 3;

            public static readonly float spaceAfterSettings = 0;

            public static readonly float maxSettingsWidth = 150;

            public static readonly GUIStyle portsBackground;

            public static readonly float iconSize = 24f;

            public static readonly float iconsSize = IconSize.Small;

            public static readonly float iconsSpacing = 3;

            public static readonly int iconsPerColumn = 2;

            public static readonly float spaceAfterIcon = 3;

            public static readonly float spaceAfterSurtitle = 1;

            public static readonly float spaceBeforeSubtitle = 0;

            public static readonly float invokeFadeDuration = 0.5f;
        }
    }

    internal class UnitWidgetHelper
    {
        internal static void ReplaceUnit(IUnit unit, GraphReference reference, IGraphContext context, GraphSelection selection, EventWrapper eventWrapper)
        {
            var oldUnit = unit;
            var unitPosition = oldUnit.position;
            var preservation = UnitPreservation.Preserve(oldUnit);

            var options = new UnitOptionTree(new GUIContent("Node"));
            options.filter = UnitOptionFilter.Any;
            options.filter.NoConnection = false;
            options.reference = reference;

            var activatorPosition = new Rect(eventWrapper.mousePosition, new Vector2(200, 1));

            LudiqGUI.FuzzyDropdown
            (
                activatorPosition,
                options,
                null,
                delegate (object _option)
                {
                    var option = (IUnitOption)_option;

                    context.BeginEdit();
                    UndoUtility.RecordEditedObject("Replace Node");
                    var graph = oldUnit.graph;
                    oldUnit.graph.units.Remove(oldUnit);
                    var newUnit = option.InstantiateUnit();
                    newUnit.guid = Guid.NewGuid();
                    newUnit.position = unitPosition;
                    graph.units.Add(newUnit);
                    preservation.RestoreTo(newUnit);
                    option.PreconfigureUnit(newUnit);
                    selection.Select(newUnit);
                    GUI.changed = true;
                    context.EndEdit();
                }
            );
        }
    }
}
#else
namespace Unity.VisualScripting.Community
{
    public class UnitWidget<TUnit> : VisualScripting.UnitWidget<TUnit>, IUnitWidget where TUnit : class, IUnit
    {
        public UnitWidget(FlowCanvas canvas, TUnit unit) : base(canvas, unit)
        {
        }

        private List<RectUtility.SnapResult.Line> snapLines = new List<RectUtility.SnapResult.Line>();

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

        public override void HandleInput()
        {
            if (isDragging && e.ctrlOrCmd)
            {
                List<Rect> otherRects = graph.elements.Where(e => e != element && !(e is IUnitConnection))
                    .Select(e => canvas.Widget(e).position)
                    .ToList();

                var snapResult = RectUtility.CheckSnap(_position, otherRects, threshold: 10f);

                if (snapResult.snapped)
                {
                    _position.position = snapResult.snapPosition;
                    snapLines = snapResult.snapLines;
                    Reposition();
                }
                else
                {
                    snapLines.Clear();
                }
            }
            base.HandleInput();
        }

        public override void DrawForeground()
        {
            if (isDragging && e.ctrlOrCmd)
                DrawSnapLines();

            base.DrawForeground();
        }

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

        private void ConvertToEmbed()
        {
            NodeSelection.Convert(GraphSource.Embed);
        }

        private void ConvertToMacro()
        {
            NodeSelection.Convert(GraphSource.Macro);
        }

        internal class UnitWidgetHelper
        {
            internal static void ReplaceUnit(IUnit unit, GraphReference reference, IGraphContext context, GraphSelection selection, EventWrapper eventWrapper)
            {
                var oldUnit = unit;
                var unitPosition = oldUnit.position;
                var preservation = UnitPreservation.Preserve(oldUnit);

                var options = new UnitOptionTree(new GUIContent("Node"));
                options.filter = UnitOptionFilter.Any;
                options.filter.NoConnection = false;
                options.reference = reference;

                var activatorPosition = new Rect(eventWrapper.mousePosition, new Vector2(200, 1));

                LudiqGUI.FuzzyDropdown
                (
                    activatorPosition,
                    options,
                    null,
                    delegate (object _option)
                    {
                        var option = (IUnitOption)_option;

                        context.BeginEdit();
                        UndoUtility.RecordEditedObject("Replace Node");
                        var graph = oldUnit.graph;
                        oldUnit.graph.units.Remove(oldUnit);
                        var newUnit = option.InstantiateUnit();
                        newUnit.guid = Guid.NewGuid();
                        newUnit.position = unitPosition;
                        graph.units.Add(newUnit);
                        preservation.RestoreTo(newUnit);
                        option.PreconfigureUnit(newUnit);
                        selection.Select(newUnit);
                        GUI.changed = true;
                        context.EndEdit();
                    }
                );
            }
        }
    }
}
#endif

#endif