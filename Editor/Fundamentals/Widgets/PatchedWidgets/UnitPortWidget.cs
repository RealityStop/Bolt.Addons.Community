#if ENABLE_VERTICAL_FLOW
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Analytics;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class UnitPortWidget<TPort> : Widget<FlowCanvas, TPort>, IUnitPortWidget where TPort : class, IUnitPort
    {
        protected UnitPortWidget(FlowCanvas canvas, TPort port) : base(canvas, port) { }


        #region Model

        public TPort port => item;

        public IUnit unit => port.unit;

        // Usually very efficient, but cached because it's used so often
        private IUnitWidget _unitWidget;

        public IUnitWidget unitWidget
        {
            get
            {
                if (_unitWidget == null)
                {
                    _unitWidget = canvas.Widget<IUnitWidget>(unit);
                }

                return _unitWidget;
            }
        }

        IUnitPort IUnitPortWidget.port => port;

        protected UnitPortDescription description { get; private set; }

        public Metadata inspectorMetadata { get; private set; }

        protected Inspector inspector { get; private set; }

        public override Metadata FetchMetadata()
        {
            return description.getMetadata(unitWidget.metadata);
        }

        public virtual Metadata FetchInspectorMetadata()
        {
            return null;
        }

        protected override void CacheDescription()
        {
            description = port.Description<UnitPortDescription>();

            labelContent.text = description.label;

            Reposition();
        }

        protected override void CacheMetadata()
        {
            base.CacheMetadata();

            inspectorMetadata = FetchInspectorMetadata();

            if (inspectorMetadata != null)
            {
                inspector = unitWidget.GetPortInspector(port, inspectorMetadata);
            }
            else
            {
                inspector = null;
            }
        }

        #endregion


        #region Lifecycle

        public override bool foregroundRequiresInput => showInspector;

        public bool wouldDisconnect { get; private set; }

        public bool willDisconnect => wouldDisconnect && isMouseOver;

        protected virtual bool canStartConnection => true;

        static Type type = typeof(GraphWindow).Assembly.GetTypes().FirstOrDefault(t => t.Name == "HotkeyUsageAnalytics");
        static MethodInfo method = type.GetMethod("HotkeyUsed", BindingFlags.NonPublic | BindingFlags.Static);
        static Enum @enum = (Enum)type.GetNestedType("Hotkey", BindingFlags.NonPublic).GetField("RmbRemoveConnections", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        public override void HandleInput()
        {
            if (!canvas.isCreatingConnection)
            {
                if (e.IsMouseDown(MouseButton.Left))
                {
                    if (canStartConnection)
                    {
                        StartConnection();
                    }

                    e.Use();
                }
                else if (e.IsMouseDown(MouseButton.Right))
                {
                    wouldDisconnect = true;
                }
                else if (e.IsMouseUp(MouseButton.Right))
                {
                    if (isMouseOver)
                    {
                        method.Invoke(null, new object[] { @enum });

                        // HotkeyUsageAnalytics.HotkeyUsed(HotkeyUsageAnalytics.Hotkey.RmbRemoveConnections);

                        RemoveConnections();
                    }

                    wouldDisconnect = false;
                    e.Use();
                }
            }
            else
            {
                var source = canvas.connectionSource;
                var isSource = source == port;

                if (!isSource && e.IsMouseDown(MouseButton.Left))
                {
                    var destination = port;
                    FinishConnection(source, destination);
                    e.Use();
                }
                else if (isSource && e.IsMouseUp(MouseButton.Left))
                {
                    IUnitPort destination = null;

                    var hovered = canvas.hoveredWidget;

                    if (hovered is IUnitPortWidget)
                    {
                        destination = ((IUnitPortWidget)hovered).port;
                    }
                    else if (hovered is IUnitWidget)
                    {
                        destination = source.CompatiblePort(((IUnitWidget)hovered).unit);
                    }

                    if (destination != null)
                    {
                        if (destination != source)
                        {
                            FinishConnection(source, destination);
                        }
                    }
                    else
                    {
                        if (canvas.isMouseOverBackground)
                        {
                            canvas.NewUnitContextual();
                        }
                        else if (!canvas.isMouseOver)
                        {
                            canvas.CancelConnection();
                        }
                    }

                    e.Use();
                }
                else if (isSource && e.IsMouseDrag(MouseButton.Left))
                {
                    e.Use();
                }
                else if (isSource && e.IsMouseDown(MouseButton.Right))
                {
                    canvas.CancelConnection();
                    e.Use();
                }
            }
        }

        private void StartConnection()
        {
            canvas.connectionSource = port;
            window.Focus();
        }

        private void RemoveConnections()
        {
            UndoUtility.RecordEditedObject("Disconnect Port");

            foreach (var connectedPort in port.connectedPorts)
            {
                canvas.Widget(connectedPort.unit).Reposition();
            }

            unitWidget.Reposition();

            port.Disconnect();

            e.Use();

            GUI.changed = true;
        }

        private void FinishConnection(IUnitPort source, IUnitPort destination)
        {
            if (source.CanValidlyConnectTo(destination))
            {
                UndoUtility.RecordEditedObject("Connect Nodes");
                source.ValidlyConnectTo(destination);
                canvas.connectionSource = null;
                canvas.Widget(source.unit).Reposition();
                canvas.Widget(destination.unit).Reposition();
                GUI.changed = true;
            }
            else
            {
                Debug.LogWarningFormat
                    (
                        "Cannot connect this {0} to this {1}.\n",
                        source.GetType().HumanName().ToLower(),
                        destination.GetType().HumanName().ToLower()
                    );
            }
        }

        #endregion


        #region Contents

        private readonly GUIContent labelContent = new GUIContent();

        #endregion


        #region Positioning

        public override IEnumerable<IWidget> positionDependencies => ((IWidget)unitWidget).Yield();

        public override IEnumerable<IWidget> positionDependers => port.connections.Select(connection => (IWidget)canvas.Widget(connection));

        protected abstract Edge edge { get; }

        public float y { get; set; }
        public float x { get; set; }

        protected Rect _position;

        public override Rect position
        {
            get { return _position; }
            set { }
        }

        public Rect handlePosition { get; protected set; }

        public Rect labelPosition { get; protected set; }

        public Rect iconPosition { get; protected set; }

        public Rect inspectorPosition { get; protected set; }

        public Rect identifierPosition { get; protected set; }

        public Rect surroundPosition { get; protected set; }

        public override Rect hotArea
        {
            get
            {
                if (canvas.isCreatingConnection)
                {
                    if (canvas.connectionSource == port || canvas.connectionSource.CanValidlyConnectTo(port))
                    {
                        return Styles.easierGrabOffset.Add(identifierPosition);
                    }

                    return Rect.zero;
                }

                return Styles.easierGrabOffset.Add(handlePosition);
            }
        }

        public override void CachePosition()
        {
            var outside = edge.Normal().x;
            var inside = -outside;
            var flip = inside < 0;

            var handlePosition = new Rect(
                x,
                y + (EditorGUIUtility.singleLineHeight - Styles.handleSize.y + Styles.spaceBetweenEdgeAndHandle) / 2,
                Styles.handleSize.x,
                Styles.handleSize.y
            );

            if (flip) handlePosition.x -= handlePosition.width;

            this.handlePosition = handlePosition;
            _position = handlePosition;
            identifierPosition = handlePosition;

            x += Styles.spaceAfterEdge * inside;

            // Align icon, label, inspector relative to handlePosition.center.x
            if (showIcon)
            {
                var iconPosition = new Rect(
                    handlePosition.center.x - Styles.iconSize / 2,
                    unitWidget.position.GetEdgeCenter(edge).y - Styles.spaceAfterEdge - 3,
                    Styles.iconSize,
                    Styles.iconSize
                ).PixelPerfect();
                if (flip) iconPosition.x -= iconPosition.width;

                _position = _position.Encompass(iconPosition);
                identifierPosition = identifierPosition.Encompass(iconPosition);
                this.iconPosition = iconPosition;
            }

            if (showLabel)
            {
                var labelPosition = new Rect(
                    handlePosition.center.x - GetLabelWidth() / 2f,
                    unitWidget.position.GetEdgeCenter(edge).y - Styles.spaceAfterEdge - 5,
                    GetLabelWidth(),
                    GetLabelHeight()
                );
                if (flip) labelPosition.x -= labelPosition.width;

                _position = _position.Encompass(labelPosition);
                identifierPosition = identifierPosition.Encompass(labelPosition);
                this.labelPosition = labelPosition;
            }

            surroundPosition = Styles.surroundPadding.Add(identifierPosition);
        }
        public float GetInnerWidth()
        {
            var width = 0f;

            if (showIcon)
            {
                width += Styles.iconSize;
            }

            if (showIcon && showLabel)
            {
                width += Styles.spaceBetweenIconAndLabel;
            }

            if (showIcon && !showLabel && showInspector)
            {
                width += Styles.spaceBetweenIconAndInspector;
            }

            if (showLabel)
            {
                width += GetLabelWidth();
            }

            if (showLabel && showInspector)
            {
                width += Styles.spaceBetweenLabelAndInspector;
            }

            if (showInspector)
            {
                width += GetInspectorWidth();
            }

            return width;
        }

        private float GetInspectorWidth()
        {
            var width = inspector.GetAdaptiveWidth();

            width = Mathf.Min(width, Styles.maxInspectorWidth);

            if (!showLabel)
            {
                width = Mathf.Max(width, Styles.labellessInspectorMinWidth);
            }

            return width;
        }

        protected float GetLabelWidth()
        {
            return Mathf.Min(Styles.label.CalcSize(labelContent).x, Styles.maxLabelWidth);
        }

        public float GetHeight()
        {
            var height = 0f;

            if (showIcon)
            {
                height = Mathf.Max(height, Styles.iconSize);
            }

            if (showLabel)
            {
                height = Mathf.Max(height, GetLabelHeight());
            }

            if (showInspector)
            {
                height = Mathf.Max(height, GetInspectorHeight());
            }

            return height;
        }

        protected float GetLabelHeight()
        {
            return Styles.label.CalcSize(labelContent).y;
        }

        private float GetInspectorHeight()
        {
            var width = GetInspectorWidth();

            using (LudiqGUIUtility.currentInspectorWidth.Override(width))
            {
                return inspector.GetCachedHeight(width, GUIContent.none, null);
            }
        }

        public override float zIndex
        {
            get { return unitWidget.zIndex + 0.5f; }
            set { }
        }

        #endregion


        #region Drawing

        public override bool canClip => base.canClip && canvas.connectionSource != port;

        protected virtual bool showInspector => false;

        protected bool showIcon => false;

        public virtual bool showLabel => description.showLabel;

        public virtual Color color => Color.white;

        protected abstract Texture handleTextureConnected { get; }

        protected abstract Texture handleTextureUnconnected { get; }

        protected virtual bool colorIfActive => true;

        protected override bool dim
        {
            get
            {
                var dim = BoltCore.Configuration.dimInactiveNodes && !unit.Analysis<UnitAnalysis>(context).isEntered;

                if (unitWidget.isMouseOver || unitWidget.isSelected)
                {
                    dim = false;
                }

                if (BoltCore.Configuration.dimIncompatibleNodes && canvas.isCreatingConnection)
                {
                    dim = canvas.connectionSource != port && !canvas.connectionSource.CanValidlyConnectTo(port);
                }

                return dim;
            }
        }

        public override void DrawBackground() { }

        public override void DrawForeground()
        {
            if (BoltCore.Configuration.developerMode && BoltCore.Configuration.debug)
            {
                EditorGUI.DrawRect(clippingPosition, new Color(0, 0, 0, 0.1f));
            }

            BeginDim();

            DrawHandle();

            if (showIcon)
            {
                DrawIcon();
            }

            if (showLabel)
            {
                DrawLabel();
            }

            if (showInspector && graph.zoom >= FlowCanvas.inspectorZoomThreshold)
            {
                DrawInspector();
            }

            EndDim();
        }

        public override void DrawOverlay()
        {
            base.DrawOverlay();

            var surroundFromPort = canvas.isCreatingConnection &&
                isMouseOver &&
                canvas.connectionSource.CanValidlyConnectTo(port);

            var surroundFromUnit = canvas.isCreatingConnection &&
                unitWidget.isMouseOver &&
                canvas.connectionSource.CompatiblePort(unit) == port;

            if (surroundFromPort || surroundFromUnit)
            {
                DrawSurround();
            }

            if (canvas.connectionSource == port)
            {
                DrawConnectionSource();
            }
        }

        private void GetConnectionsNoAlloc(HashSet<IUnitConnection> connections)
        {
            connections.Clear();

            var graph = unit.graph;

            // Unit might have been removed from graph, but still drawn this frame.
            if (graph == null)
            {
                return;
            }

            var controlInput = port as ControlInput;
            var controlOutput = port as ControlOutput;
            var valueInput = port as ValueInput;
            var valueOutput = port as ValueOutput;
            var input = port as IUnitInputPort;
            var output = port as IUnitOutputPort;

            if (controlInput != null)
            {
                foreach (var connection in graph.controlConnections.WithDestinationNoAlloc(controlInput))
                {
                    connections.Add(connection);
                }
            }

            if (controlOutput != null)
            {
                foreach (var connection in graph.controlConnections.WithSourceNoAlloc(controlOutput))
                {
                    connections.Add(connection);
                }
            }

            if (valueInput != null)
            {
                foreach (var connection in graph.valueConnections.WithDestinationNoAlloc(valueInput))
                {
                    connections.Add(connection);
                }
            }

            if (valueOutput != null)
            {
                foreach (var connection in graph.valueConnections.WithSourceNoAlloc(valueOutput))
                {
                    connections.Add(connection);
                }
            }

            if (input != null)
            {
                foreach (var connection in graph.invalidConnections.WithDestinationNoAlloc(input))
                {
                    connections.Add(connection);
                }
            }

            if (output != null)
            {
                foreach (var connection in graph.invalidConnections.WithSourceNoAlloc(output))
                {
                    connections.Add(connection);
                }
            }
        }

        private void DrawHandle()
        {
            // Trying to be very speed / memory efficient in this method

            if (!e.IsRepaint)
            {
                return;
            }

            var color = Color.white;

            var highlight = false;

            var invalid = false;

            var willDisconnect = false;

            var connections = HashSetPool<IUnitConnection>.New();

            GetConnectionsNoAlloc(connections);

            var isConnected = connections.Count > 0;

            if (isConnected)
            {
                foreach (var connection in connections)
                {
                    if (connection is InvalidConnection)
                    {
                        invalid = true;
                    }

                    var sourceWidget = canvas.Widget<IUnitPortWidget>(connection.source);
                    var destinationWidget = canvas.Widget<IUnitPortWidget>(connection.destination);

                    if (sourceWidget.isMouseOver || destinationWidget.isMouseOver)
                    {
                        highlight = true;
                    }

                    if (sourceWidget.willDisconnect || destinationWidget.willDisconnect)
                    {
                        willDisconnect = true;
                    }
                }
            }

            if (isMouseOver)
            {
                highlight = true;
            }

            if (willDisconnect)
            {
                color = UnitConnectionStyles.disconnectColor;
            }
            else if (highlight)
            {
                color = UnitConnectionStyles.highlightColor;
            }
            else if (invalid)
            {
                color = UnitConnectionStyles.invalidColor;
            }
            else if (canvas.isCreatingConnection && (canvas.connectionSource == port || canvas.connectionSource.CanValidlyConnectTo(port)))
            {
                color = this.color;
            }
            else if (isConnected)
            {
                Color? resolvedColor = null;

                foreach (var connection in connections)
                {
                    var connectionColor = canvas.Widget<IUnitConnectionWidget>(connection).color;

                    if (resolvedColor == null)
                    {
                        resolvedColor = connectionColor;
                    }
                    else if (resolvedColor != connectionColor)
                    {
                        resolvedColor = this.color;

                        break;
                    }
                }

                color = resolvedColor.Value;
            }

            if (colorIfActive)
            {
                foreach (var connection in connections)
                {
                    var connectionEditorData = reference.GetElementDebugData<IUnitConnectionDebugData>(connection);

                    if (EditorApplication.isPaused)
                    {
                        if (EditorTimeBinding.frame == connectionEditorData.lastInvokeFrame)
                        {
                            color = UnitConnectionStyles.activeColor;

                            break;
                        }
                    }
                    else
                    {
                        color = Color.Lerp(UnitConnectionStyles.activeColor, color, (EditorTimeBinding.time - connectionEditorData.lastInvokeTime) / UnitWidget<IUnit>.Styles.invokeFadeDuration);
                    }
                }
            }

            var handlePosition = this.handlePosition;

            if (highlight)
            {
                var widthExpansion = handlePosition.width * (Styles.highlightScaling - 1);
                var heightExpansion = handlePosition.height * (Styles.highlightScaling - 1);
                handlePosition.width += widthExpansion;
                handlePosition.height += heightExpansion;
                handlePosition.x -= widthExpansion / 2;
                handlePosition.y -= heightExpansion / 2;
            }

            if (highlight ||
                isConnected ||
                canvas.connectionSource == port ||
                canvas.isCreatingConnection && canvas.connectionSource.CanValidlyConnectTo(port))
            {
                using (LudiqGUI.color.Override(color.WithAlphaMultiplied(LudiqGUI.color.value.a * 0.85f))) // Full color is a bit hard on the eyes
                {
                    if (handleTextureConnected != null)
                    {
                        GUI.DrawTexture(handlePosition, handleTextureConnected);
                    }
                }
            }
            else
            {
                if (handleTextureUnconnected != null)
                {
                    GUI.DrawTexture(handlePosition, handleTextureUnconnected);
                }
            }

            HashSetPool<IUnitConnection>.Free(connections);
        }

        private void DrawIcon()
        {
            if (description != null && description.icon[Styles.iconSize])
            {
                GUI.DrawTexture(iconPosition, description.icon?[Styles.iconSize]);
            }
        }

        private void DrawLabel()
        {
            GUI.Label(labelPosition, description.label, Styles.label);
        }

        private void DrawInspector()
        {
            EditorGUI.BeginChangeCheck();

            using (LudiqGUIUtility.currentInspectorWidth.Override(inspectorPosition.width))
            using (Inspector.adaptiveWidth.Override(true))
            {
                inspector.Draw(inspectorPosition, GUIContent.none);
            }

            if (EditorGUI.EndChangeCheck())
            {
                unitWidget.Reposition();
            }
        }

        private void DrawConnectionSource()
        {
            var start = handlePosition.GetEdgeCenter(edge);

            if (window.IsFocused())
            {
                canvas.connectionEnd = mousePosition;
            }

            float minBend = 20f;

            GraphGUI.DrawConnection
                (
                    color,
                    start,
                    canvas.connectionEnd,
                    edge,
                    null,
                    handleTextureConnected,
                    Styles.handleSize,
                    UnitConnectionStyles.relativeBend,
                    minBend
                );
        }

        private void DrawSurround()
        {
            if (e.controlType == EventType.Repaint)
            {
                Styles.surround.Draw(surroundPosition, false, false, false, false);
            }
        }

        #endregion


        public static class Styles
        {
            private static byte[] t;
            private static Texture2D tx;
            static Styles()
            {
                label = new GUIStyle(EditorStyles.label);
                label.wordWrap = false;
                label.imagePosition = ImagePosition.TextOnly;
                label.padding = new RectOffset(0, 0, 0, 0);

                TextureResolution[] textureResolution = { 2 };

                surround = new GUIStyle
                {
                    normal =
                    {
                        background = BoltCore.Resources.LoadTexture($"Surround.png", textureResolution, CreateTextureOptions.Scalable).Single()
                    }
                };
            }

            public const float highlightScaling = 1f;

            public static readonly Vector2 handleSize = new Vector2(12, 12);

            public static readonly float spaceBetweenEdgeAndHandle = 5;

            public static readonly float spaceAfterEdge = 5;

            public static readonly float spaceBetweenIconAndLabel = 5;

            public static readonly float spaceBetweenIconAndInspector = 5;

            public static readonly float spaceBetweenLabelAndInspector = 5;

            public static readonly float labellessInspectorMinWidth = 75;

            public static readonly float maxInspectorWidth = 200;

            public static readonly float maxLabelWidth = 150;

            public static readonly int iconSize = IconSize.Small;

            public static readonly GUIStyle label;

            public static readonly GUIStyle surround;

            public static readonly RectOffset easierGrabOffset = new RectOffset(5, 5, 4, 4);

            public static readonly RectOffset surroundPadding = new RectOffset(3, 3, 2, 2);
        }
    }

    public abstract class UnitOutputPortWidget<TPort> : UnitPortWidget<TPort> where TPort : class, IUnitOutputPort
    {
        protected UnitOutputPortWidget(FlowCanvas canvas, TPort port) : base(canvas, port) { }

        protected override Edge edge => Edge.Right;
    }

    public abstract class UnitInputPortWidget<TPort> : UnitPortWidget<TPort> where TPort : class, IUnitInputPort
    {
        protected UnitInputPortWidget(FlowCanvas canvas, TPort port) : base(canvas, port) { }

        protected override Edge edge => Edge.Left;
    }
}
#endif