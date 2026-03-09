using System;
using UnityEditor;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditorTypes
    {
        public partial class Data
        {
            public struct Get { }

            public struct Field
            {
                public Window window;

                public Field(Window window)
                {
                    this.window = window;
                }
            }

            public struct Window
            {
                public Get get;
                public EditorWindow window;

                public Window(Get get, EditorWindow window)
                {
                    this.get = get;
                    this.window = window;
                }
            }

            public struct GetMain
            {
                public Get get;

                public GetMain(Get get)
                {
                    this.get = get;
                }
            }

            public struct GetGUIView
            {
                public Get get;

                public GetGUIView(Get get)
                {
                    this.get = get;
                }
            }

            public struct GetHostView
            {
                public EditorWindow window;
                public Get get;

                public GetHostView(Get get, EditorWindow window)
                {
                    this.get = get;
                    this.window = window;
                }
            }
        }
    }
}
