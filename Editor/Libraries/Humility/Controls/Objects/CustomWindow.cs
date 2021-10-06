using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public abstract class CustomWindow : EditorWindow
    {
        /// <summary>
        /// Indicated weither the window is finished initializing or not.
        /// </summary>
        protected bool isInitialized;

        /// <summary>
        /// The background Visual Element.
        /// </summary>
        protected Rectangle background;

        /// <summary>
        /// Thickness of the border for this window.
        /// </summary>
        protected virtual int borderThickness => 1;

        /// <summary>
        /// Border color for this window.
        /// </summary>
        protected virtual Color borderColor => Color.black;

        /// <summary>
        /// Background color for this window.
        /// </summary>
        protected virtual Color backgroundColor => HUMEditorColor.DefaultEditorBackground;

        /// <summary>
        /// Set and get this position when working with your window rect. The regular position can only be updated at the beginning of the next frame.
        /// </summary>
        protected Rect windowPosition;

        /// <summary>
        /// The size while the window is not in a maximized state.
        /// </summary>
        protected Rect normalSizePosition;

        protected virtual Label label { get; } = new Label();
        protected virtual string headerTitle => this.GetType().Name;

        /// <summary>
        /// The current event from this window.
        /// </summary>
        protected Event e;

        private void OnEnable()
        {
            InitializeTree();
            isInitialized = true;
        }

        /// <summary>
        /// Invoked when right before the windows VisualElement tree is created.
        /// </summary>
        protected virtual void BeforeTreeCreated()
        {
            
        }

        /// <summary>
        /// Builds and sets all the data pertaining to the windows initialization.
        /// </summary>
        protected void InitializeTree()
        {
            BeforeTreeCreated();

            background = new BorderedRectangle(backgroundColor, borderColor, borderThickness);
            background.style.position = Position.Relative;
            background.AlignItems(Align.Stretch);
            background.StretchToParentSize();
            background.pickingMode = PickingMode.Ignore;

            rootVisualElement.Add(background);

            var window = OnCreateWindow();

            if (window != null) background.Add(window);
        }

        /// <summary>
        /// Your own custom IMGUI method. This is called in OnGUI right after we assign field 'e' the current event, and set the background rect.
        /// </summary>
        protected virtual void OnWindowGUI()
        {

        }

        /// <summary>
        /// The Visual Element for the entire body of the window. Excludes the background.
        /// </summary>
        protected virtual VisualElement OnCreateWindow()
        {
            return null;
        }

        internal virtual void OnGUI()
        {
            e = Event.current;

            OnWindowGUI();

            Repaint();
        }

        public static T GetCustomWindow<T>(bool popup = false) where T : CustomWindow
        {
            if (typeof(T).Inherits<TitlebarWindow>())
            {
                var databaseInstance = Resources.FindObjectsOfTypeAll<T>();
                if (databaseInstance.Length == 0)
                {
                    var instance = CreateInstance<T>();
                    instance.ShowPopup();
                    (instance as TitlebarWindow).position = new Rect((Screen.currentResolution.width / 2) - 400, (Screen.currentResolution.height / 2) - 400, 800, 800);
                    instance.Focus();
                    return instance;
                }

                (databaseInstance[0] as TitlebarWindow).position = new Rect((Screen.currentResolution.width / 2) - 400, (Screen.currentResolution.height / 2) - 400, 800, 800);
                databaseInstance[0].Focus();
                return databaseInstance[0];
            }
            else
            {
                if (popup)
                {
                    var instance = CreateInstance<T>();
                    instance.ShowPopup();
                    return instance;
                }

                GetWindow<T>();
                return GetWindow<T>();
            }
        }
    }
}
