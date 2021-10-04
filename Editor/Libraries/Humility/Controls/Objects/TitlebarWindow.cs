using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    /// <summary>
    /// A custom floating/dockable window with a custom title bar graphics and status buttons intended to be like a desktop window. Close, Minimize, and Maximimize.
    /// </summary>
    public abstract class TitlebarWindow : CustomWindow
    {
        protected virtual Texture2D headerIcon => null;

        /// <summary>
        /// The container size of the button area for minimizing, maximizing, and closing the window.
        /// </summary>
        protected virtual Vector2 WindowStatusContainerSize => GetStatusContainerSize();

        /// <summary>
        /// The color of the title bars background.
        /// </summary>
        protected virtual Color TitlebarBackgroundColor => backgroundColor.Darken(0.075f);

        /// <summary>
        /// The border color for the title bar.
        /// </summary>
        protected virtual Color TitlebarBorderColor => borderColor;

        /// <summary>
        /// Returns true if the mouse is currently held down.
        /// </summary>
        protected bool mouseDown { get; private set; }

        /// <summary>
        /// The position we last clicked down within the title bar.
        /// </summary>
        private Vector2 clickPosition;

        /// <summary>
        /// Return true to show the close button. Will effect the status container.
        /// </summary>
        protected virtual bool showCloseButton => true;

        /// <summary>
        /// Return true to show the minimize button. Will effect the status container.
        /// </summary>
        protected virtual bool showMinimizeButton => true;

        /// <summary>
        /// Return true to show the maximize button. Will effect the status container.
        /// </summary>
        protected virtual bool showMaximizeButton => true;

        /// <summary>
        /// Return true if the window can be maximized.
        /// </summary>
        protected virtual bool canMaximize => true;

        /// <summary>
        /// Return true if the window can be minimized.
        /// </summary>
        protected virtual bool canMinimize => true;

        /// <summary>
        /// Return true if the window can be closed.
        /// </summary>
        protected virtual bool canClose => true;

        /// <summary>
        /// Return true if the window can be resized horizontally fromt left and right handles.
        /// </summary>
        protected virtual bool canResizeHorizontal => true;

        /// <summary>
        /// Return true if the window can be resized vertically from the top and bottom handles.
        /// </summary>
        protected virtual bool canResizeVertical => true;

        /// <summary>
        /// Returns true if the window can be moved.
        /// </summary>
        protected virtual bool canMove => true;

        private bool isMaximized;
        private bool isResizingLeft;
        private bool isResizingRight;
        private bool isResizingTop;
        private bool isResizingBottom;

        protected bool isResizing => isResizingLeft || isResizingRight || isResizingBottom || isResizingTop;

        private bool isMoving;

        /// <summary>
        /// The size of the window status buttons.
        /// </summary>
        protected virtual int buttonSize => 24;
        

        /// <summary>
        /// Creates the entire window, adding Visual Elements for the title bar and body.
        /// </summary>
        protected override VisualElement OnCreateWindow()
        {
            var parent = new VisualElement();
            parent.style.flexDirection = FlexDirection.Column;

            var titlebar = CreateTitlebar();
            parent.Add(titlebar);

            var body = OnCreateBody();
            parent.Add(body);

            return parent;
        }

        /// <summary>
        /// Creates the Visual Element for the body below the title bar.
        /// </summary>
        /// <returns></returns>
        protected virtual VisualElement OnCreateBody()
        {
            return null;
        }

        /// <summary>
        /// Creates the visual elements tree for the title bar.
        /// </summary>
        private VisualElement CreateTitlebar()
        {
            var background = new BorderedRectangle(TitlebarBackgroundColor, TitlebarBorderColor, 0);
            background.style.height = 34;

            var paddedContainer = new VisualElement();
            paddedContainer.Set().Padding(4);
            paddedContainer.StretchToParentSize();
            paddedContainer.style.flexDirection = FlexDirection.Row;

            RegisterMove(paddedContainer);

            var leftContainer = HUMRetained.Container();
            leftContainer.style.position = Position.Relative;
            leftContainer.style.flexDirection = FlexDirection.Row;
            leftContainer.style.flexGrow = 1;

            var icon = new Image();
            icon.image = headerIcon;
            icon.style.flexDirection = FlexDirection.Row;
            icon.style.position = Position.Relative;
            icon.style.height = 34; 
            icon.style.width = 34;
            icon.Set().Padding(4);

            var headerTitle = label;
            headerTitle.style.flexDirection = FlexDirection.Row;
            headerTitle.style.position = Position.Relative;
            headerTitle.Set().Padding(4);
            if (headerIcon == null) headerTitle.style.paddingLeft = 10;
            background.style.height = 40;
            background.Add(paddedContainer);

            if (headerIcon != null) leftContainer.Add(icon);
            leftContainer.Add(headerTitle);

            leftContainer.AlignItems(Align.Center);
            leftContainer.Justification(Justify.FlexStart);
            leftContainer.style.unityTextAlign = TextAnchor.MiddleCenter;

            var rightContainer = WindowStatus();

            paddedContainer.Add(leftContainer);
            paddedContainer.Add(rightContainer);

            paddedContainer.AlignItems(Align.Center);
            paddedContainer.Justification(Justify.FlexEnd);
            return background;
        }

        /// <summary>
        /// Creates the visual element tree for the window status buttons container.
        /// </summary>
        protected VisualElement WindowStatus()
        {
            var container = new VisualElement();
            container.PositionStyle(Position.Relative);
            container.Flex(FlexDirection.Row);
            container.Justification(Justify.FlexEnd);

            if (showMinimizeButton)
            {
                container.Add(OnCreateMinimizeButton());
            }

            if (showMaximizeButton)
            {
                container.Add(OnCreateMaximizeButton());
            }

            if (showCloseButton)
            {
                container.Add(OnCreateCloseButton());
            }

            return container;
        }

        /// <summary>
        /// Creates the minimize button Visual Element.
        /// </summary>
        protected virtual Button OnCreateMinimizeButton()
        {
            var minBackground = new Button();
            minBackground.style.width = 22;
            minBackground.style.height = 22;
            minBackground.text = "-";
            minBackground.style.fontSize = 20;
            minBackground.style.unityTextAlign = TextAnchor.MiddleCenter;
            minBackground.SetEnabled(canMinimize);

            return minBackground;
        }

        /// <summary>
        /// Created the maximize button Visual Element.
        /// </summary>
        /// <returns></returns>
        protected virtual Button OnCreateMaximizeButton()
        {
            var maxBackground = new Button();
            maxBackground.style.width = 22;
            maxBackground.style.height = 22;
            maxBackground.AlignItems(Align.Center);
            maxBackground.style.justifyContent = Justify.Center;

            var rectangle = new Rectangle(Color.clear);
            rectangle.style.width = 10;
            rectangle.style.height = 10;
            rectangle.Set().Border(1);
            rectangle.Set().BorderColor(Color.white);
            
            maxBackground.clicked += () =>
            {
                TryMaximizeUnmaximize();
            };

            maxBackground.SetEnabled(canMaximize);
            maxBackground.Add(rectangle);

            return maxBackground;
        }

        /// <summary>
        /// Creates the close button Visual Element.
        /// </summary>
        /// <returns></returns>
        protected virtual Button OnCreateCloseButton()
        {
            var closeBackground = new Button();
            closeBackground.style.width = 22;
            closeBackground.style.height = 22;
            closeBackground.text = "X";
            closeBackground.clicked += Close;
            closeBackground.AlignSelf(Align.Center);
            return closeBackground;
        }

        /// <summary>
        /// Attempts to maximize the window if 'isMaximized' is false. 
        /// If 'isMaximized' is true, it will attempt to Unmaximize the window.
        /// Returns true if 'canMaximized' is true and we were successful.
        /// </summary>
        protected bool TryMaximizeUnmaximize()
        {
            if (canMaximize)
            {
                if (!isMaximized)
                {
                    Maximize();
                }
                else
                {
                    Unmaximize();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        protected void Maximize()
        {
            isMaximized = true;
            windowPosition = HUMEditor.MaximizedWindowSize;
        }

        /// <summary>
        /// Unmaximizes the window.
        /// </summary>
        protected void Unmaximize()
        {
            isMaximized = false;
            windowPosition = normalSizePosition;
        }

        /// <summary>
        /// Returns the size for the container that the status buttons appear in.
        /// </summary>
        protected Vector2 GetStatusContainerSize()
        {
            var container = new Vector2(0, buttonSize);
            if (showCloseButton) container.x += buttonSize;
            if (showMaximizeButton) container.x += buttonSize;
            if (showMinimizeButton) container.x += buttonSize;
            return container;
        }

        internal override sealed void OnGUI()
        {
            e = Event.current;
            UpdateMouseDown();
            Resize();
            Move();
            OnWindowGUI();
            RecordMouseData();
            ResizeWindow();
            SetNormalWindowSize();
            Repaint();
        }

        /// <summary>
        /// Runs the handles for resizing the window and changes the sizes accordingly.
        /// </summary>
        protected void Resize()
        {
            if (!isMaximized && !isMoving)
            {
                var min = new Vector2(250, 250);
                var max = HUMEditor.MaximizedWindowSize.size;
                if (canResizeHorizontal)
                {
                    windowPosition = e.Mouse().Left().Resizer(windowPosition, min, max, 10, mouseDown, ref isResizingLeft);
                    windowPosition = e.Mouse().Right().Resizer(windowPosition, min, max, 10, mouseDown, ref isResizingRight);
                    if (isResizingRight || isResizingLeft) Repaint();
                }

                if (canResizeVertical)
                {
                    windowPosition = e.Mouse().Top().Resizer(windowPosition, min, max, 20, mouseDown, ref isResizingTop);
                    windowPosition = e.Mouse().Bottom().Resizer(windowPosition, min, max, 10, mouseDown, ref isResizingBottom);
                    if (isResizingTop || isResizingLeft) Repaint();
                }
            }
        }

        /// <summary>
        /// Resizes the window to the new 'windowPosition'
        /// </summary>
        protected void ResizeWindow()
        {
            position = windowPosition;
        }

        /// <summary>
        /// Sets the normal size to the current window size if we are not maximized.
        /// </summary>
        protected void SetNormalWindowSize()
        {
            if (!isMaximized) normalSizePosition = windowPosition;
        }

        /// <summary>
        /// Moves the window when dragging.
        /// </summary>
        protected void Move()
        {
            if (!isMaximized && !isResizing && canMove)
            {
                if (e.type == EventType.MouseDrag && isMoving)
                {
                    var clickDifference = e.mousePosition.Subtract(clickPosition);
                    windowPosition = new Rect(windowPosition.x + clickDifference.x, windowPosition.y + clickDifference.y, windowPosition.width, windowPosition.height);
                    e.Use();
                }
            }
        }

        public void RegisterMove(VisualElement titlebar)
        {
            titlebar.RegisterCallback<MouseDownEvent>((e) =>
            {
                clickPosition = e.originalMousePosition;

                if (!isResizing)
                {
                    isMoving = true;

                    if (e.clickCount == 2)
                    {
                        TryMaximizeUnmaximize();
                    }
                }

                e.StopImmediatePropagation();
            });

            titlebar.RegisterCallback<MouseUpEvent>((e) =>
            {
                isMoving = false;
                e.StopImmediatePropagation();
            });
        }

        protected void UpdateMouseDown()
        {
            if (e.type == EventType.MouseDown)
            {
                mouseDown = true;
            }

            if (e.type == EventType.MouseUp)
            {
                mouseDown = false;
            }
        }

        internal void RecordMouseData()
        {
            HUMEvents.mouseDelta = e.mousePosition.Subtract(HUMEvents.mousePosition);
            HUMEvents.mousePosition = e.mousePosition;
        }
    }
}
