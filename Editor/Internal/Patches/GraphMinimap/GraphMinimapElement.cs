using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    public class GraphMinimapElement : Foldout
    {
        public const float DefaultX = 200f;
        public const float DefaultY = 150f;
        private IGraphContext _context;
        private List<IGraphElementWidget> _widgets = new List<IGraphElementWidget>();
        private IGraphElementWidget _selectedWidget;

        private const float Padding = 50f;
        private Rect _combinedBounds;
        private float _scale;
        private Vector2 _minimapOffset;

        private Vector2 canvasPan;
        private Rect canvasViewport;

        public GraphMinimapElement()
        {
            style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            style.borderLeftColor = style.borderRightColor = style.borderTopColor = style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            style.borderLeftWidth = style.borderRightWidth = style.borderTopWidth = style.borderBottomWidth = 1f;

            var defaultToggle = this.Q<Toggle>(className: Foldout.toggleUssClassName);
            if (defaultToggle != null)
            {
                defaultToggle.style.display = DisplayStyle.None;
            }

            RegisterCallback<PointerDownEvent>(OnPointerDown);

            generateVisualContent += OnGenerateVisualContent;
            // Use the foldout's ability to persist it's Foldout State for the Minimized State 
            viewDataKey = "VisualScripting-Minimap-Expanded";
            Add(new IMGUIContainer(CanvasSnapshotCapture));
        }

        private void CanvasSnapshotCapture()
        {
            if (_context == null) return;

            var canvas = _context.canvas;
            if (canvas == null) return;

            canvasPan = canvas.pan;
            canvasViewport = canvas.viewport;
        }

        public void UpdateMinimap(IGraphContext context, List<IGraphElementWidget> widgets)
        {
            _context = context;

            _widgets.Clear();
            if (widgets != null) _widgets.AddRange(widgets);

            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (_context?.graph == null || _widgets.Count == 0) return;

            var canvas = _context.canvas;
            if (canvas == null) return;

            Rect localRect = contentRect;
            if (localRect.width <= 1 || localRect.height <= 1) return;

            Rect contentBounds = GraphGUI.CalculateArea(_widgets);
            contentBounds.xMin -= Padding;
            contentBounds.yMin -= Padding;
            contentBounds.xMax += Padding;
            contentBounds.yMax += Padding;

            Rect viewportWorld = new Rect(canvasPan - canvasViewport.size * 0.5f, canvasViewport.size);
            _combinedBounds = contentBounds.Encompass(viewportWorld);

            float scaleX = localRect.width / _combinedBounds.width;
            float scaleY = localRect.height / _combinedBounds.height;
            _scale = Mathf.Min(scaleX, scaleY);

            if (_scale <= 0f || float.IsInfinity(_scale)) return;

            _minimapOffset = localRect.center - _combinedBounds.size * (_scale * 0.5f);

            var painter = mgc.painter2D;

            bool contextIsValid = GraphContextProvider.instance.IsValid(_context.reference);

            foreach (var widget in _widgets)
            {
                if (widget == null || !canvas.widgetProvider.IsValid(widget.item) || !contextIsValid) continue;

                Rect drawRect = ToMinimapRect(widget.position);

                painter.fillColor = GetElementColor(widget).WithAlpha(0.2f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(drawRect.xMin, drawRect.yMin));
                painter.LineTo(new Vector2(drawRect.xMax, drawRect.yMin));
                painter.LineTo(new Vector2(drawRect.xMax, drawRect.yMax));
                painter.LineTo(new Vector2(drawRect.xMin, drawRect.yMax));
                painter.ClosePath();
                painter.Fill();

                if (canvas.selection.Contains(widget.element))
                {
                    painter.strokeColor = Color.white;
                    painter.lineWidth = 1f;
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(drawRect.xMin, drawRect.yMin));
                    painter.LineTo(new Vector2(drawRect.xMax, drawRect.yMin));
                    painter.LineTo(new Vector2(drawRect.xMax, drawRect.yMax));
                    painter.LineTo(new Vector2(drawRect.xMin, drawRect.yMax));
                    painter.ClosePath();
                    painter.Stroke();
                }
            }

            Rect viewRect = ToMinimapRect(viewportWorld);
            viewRect.xMin = Mathf.Max(viewRect.xMin, 0);
            viewRect.yMin = Mathf.Max(viewRect.yMin, 0);
            viewRect.xMax = Mathf.Min(viewRect.xMax, localRect.width);
            viewRect.yMax = Mathf.Min(viewRect.yMax, localRect.height);

            painter.fillColor = new Color(1f, 1f, 0f, 0.05f);
            painter.BeginPath();
            painter.MoveTo(new Vector2(viewRect.xMin, viewRect.yMin));
            painter.LineTo(new Vector2(viewRect.xMax, viewRect.yMin));
            painter.LineTo(new Vector2(viewRect.xMax, viewRect.yMax));
            painter.LineTo(new Vector2(viewRect.xMin, viewRect.yMax));
            painter.ClosePath();
            painter.Fill();

            painter.strokeColor = Color.yellow;
            painter.lineWidth = 1.5f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(viewRect.xMin, viewRect.yMin));
            painter.LineTo(new Vector2(viewRect.xMax, viewRect.yMin));
            painter.LineTo(new Vector2(viewRect.xMax, viewRect.yMax));
            painter.LineTo(new Vector2(viewRect.xMin, viewRect.yMax));
            painter.ClosePath();
            painter.Stroke();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_context?.graph == null || _widgets.Count == 0 || evt.button != 0) return;

            var canvas = _context.canvas;
            if (canvas == null) return;

            Vector2 mousePos = evt.localPosition;
            Vector2 mouseWorld = (mousePos - _minimapOffset) / _scale + _combinedBounds.min;

            var selection = canvas.selection;
            bool contextIsValid = GraphContextProvider.instance.IsValid(_context.reference);

            List<IGraphElementWidget> hitWidgets = new List<IGraphElementWidget>();
            IGraphElementWidget closest = null;
            float closestDistSq = float.MaxValue;

            foreach (var widget in _widgets)
            {
                if (widget == null || !canvas.widgetProvider.IsValid(widget.item) || !contextIsValid) continue;

                Rect drawRect = ToMinimapRect(widget.position);

                if (drawRect.Contains(mousePos))
                    hitWidgets.Add(widget);

                float distSq = (widget.position.center - mouseWorld).sqrMagnitude;
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = widget;
                }
            }

            IGraphElementWidget target;
            if (hitWidgets.Count > 0)
            {
                int index = 0;
                if (_selectedWidget != null)
                {
                    int i = hitWidgets.IndexOf(_selectedWidget);
                    if (i >= 0) index = (i + 1) % hitWidgets.Count;
                }
                target = hitWidgets[index];
            }
            else
            {
                target = closest;
            }

            if (target != null)
            {
                _selectedWidget = target;

                GraphUtility.OverrideContextIfNeeded(() =>
                {
                    if (target.canSelect)
                    {
                        if (evt.actionKey)
                            selection.Add(target.element);
                        else
                            selection.Select(target.element);
                    }

                    canvas.ViewElements(target.element.Yield());
                });

                evt.StopPropagation();
            }
        }

        private Rect ToMinimapRect(Rect worldRect)
        {
            return new Rect
            {
                position = (worldRect.position - _combinedBounds.min) * _scale + _minimapOffset,
                size = worldRect.size * _scale
            };
        }

        private static Color GetElementColor(IWidget widget)
        {
            if (widget is IUnitWidget)
            {
                if (widget is CommentNodeWidget comment) return comment.element.color;
                if (widget is ArrowWidget arrowWidget) return arrowWidget.element.Color;
                return Color.gray;
            }
            if (widget is GraphGroupWidget group) return group.element.color;

#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (widget is StickyNoteWidget sticky) return StickyNote.GetStickyColor(sticky.element.colorTheme);
#endif
            return Color.white;
        }
    }
}