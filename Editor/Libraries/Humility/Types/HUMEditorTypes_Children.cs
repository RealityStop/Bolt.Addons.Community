using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditorTypes_Children
    {
        /// <summary>
        /// Starts the process of getting a window of some kind.
        /// </summary>
        public static HUMEditorTypes.Data.Window Window(this HUMEditorTypes.Data.Get get, EditorWindow window)
        {
            return new HUMEditorTypes.Data.Window(get, window);
        }

        /// <summary>
        /// Finds a field within an editor window via reflection.
        /// </summary>
        public static FieldInfo Field(this HUMEditorTypes.Data.Window window, string name)
        {
            return window.window.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets the value of a field via reflection.
        /// </summary>
        public static object FieldValue(this HUMEditorTypes.Data.Window window, string name)
        {
            return window.window.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(window.window);
        }

        /// <summary>
        /// Gets te value of a field via reflection.
        /// </summary>
        public static T FieldValue<T>(this HUMEditorTypes.Data.Window window, string name)
        {
            return (T)window.window.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(window.window);
        }

        /// <summary>
        /// Gets a members info on any object via reflection.
        /// </summary>
        public static MemberInfo[] Member(this object obj, string name)
        {
            return obj.GetType().GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets a field on any object via reflection.
        /// </summary>
        public static FieldInfo Field(this object obj, string name)
        {
            return obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets a fields value on any object via reflection.
        /// </summary>
        public static object FieldValue(this object obj, string name)
        {
            return obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
        }

        /// <summary>
        /// Gets a fields value on any object via reflection.
        /// </summary>
        public static T FieldValue<T>(this object obj, string name)
        {
            return (T)obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
        }

        /// <summary>
        /// Gets all Attributes and Fields with an attribute within this objects members.
        /// </summary>
        public static List<(FieldInfo field, Attribute attr)> FieldsWithAttribute(this object obj, Type attribute)
        {
            var fields = obj.GetType().GetFields();
            var validFields = new List<(FieldInfo field, Attribute attr)>();

            for (int i = 0; i < fields.Length; i++)
            {
                var customAttribute = fields[i].GetCustomAttribute(attribute);
                if (customAttribute != null)
                {
                    validFields.Add((fields[i], customAttribute));
                }
            }
            return validFields;
        }

        /// <summary>
        /// Gets all Attributes and Fields with an attribute within this objects members.
        /// </summary>
        public static List<FieldInfo> FieldsOfType(this object obj, Type fieldType, bool inherits = true)
        {
            var fields = obj.GetType().GetFields();
            var validFields = new List<FieldInfo>();

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType == fieldType || fields[i].FieldType.Inherits(fieldType) && inherits)
                {
                    validFields.Add(fields[i]);
                }
            }
            return validFields;
        } 
        /// <summary>
        /// Gets all Attributes and Fields with an attribute within this objects members.
        /// </summary>
        public static List<FieldInfo> FieldsOfType<T>(this object obj, bool inherits = true)
        {
            var fields = obj.GetType().GetFields();
            var validFields = new List<FieldInfo>();

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType == typeof(T) || fields[i].FieldType.Inherits<T>() && inherits)
                {
                    validFields.Add(fields[i]);
                }
            }
            return validFields;
        }

        /// <summary>
        /// Gets all Attributes and Fields with an attribute within this objects members.
        /// </summary>
        public static List<(FieldInfo field, TAttribute attr)> FieldsWithAttribute<TAttribute>(this object obj) where TAttribute : Attribute
        {
            var fields = obj.GetType().GetFields();
            var validFields = new List<(FieldInfo field, TAttribute attr)>();

            for (int i = 0; i < fields.Length; i++)
            {
                var customAttribute = fields[i].GetCustomAttribute<TAttribute>();
                if (customAttribute != null)
                {
                    validFields.Add((fields[i], customAttribute));
                }
            }
            return validFields;
        }

        /// <summary>
        /// Gets an attribute on a field within an object via reflection.
        /// </summary>
        public static object FieldAttribute(this object obj, string name, Type attributeType)
        {
            return obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetCustomAttribute(attributeType);
        }

        /// <summary>
        /// Gets an attribute on a field within an object via reflection.
        /// </summary>
        public static T FieldAttribute<T>(this object obj, string name) where T : Attribute
        {
            return (T)obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetCustomAttribute<T>();
        }

        /// <summary>
        /// Gets an attribute on a field within an object via reflection.
        /// </summary>
        public static T ClassAttribute<T>(this object obj) where T : Attribute
        {
            return (T)obj.GetType().GetCustomAttribute<T>();
        }

        /// <summary>
        /// Gets a field on a host view window via reflection.
        /// </summary>
        public static FieldInfo Field(this HUMEditorTypes.Data.GetHostView hostView, string name)
        {
            return hostView.window.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets a field on a host view window via reflection.
        /// </summary>
        public static object FieldValue(this HUMEditorTypes.Data.GetHostView hostView, string name)
        {
            return hostView.window.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(hostView.window);
        }

        /// <summary>
        /// Gets a field on a host view window via reflection.
        /// </summary>
        public static T FieldValue<T>(this HUMEditorTypes.Data.GetHostView window, string name)
        {
            return (T)window.window.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(window.window);
        }

        /// <summary>
        /// Gets a property on a window via reflection.
        /// </summary>
        public static PropertyInfo Property(this HUMEditorTypes.Data.Window window, string name)
        {
            return window.window.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets a property on a window via reflection.
        /// </summary>
        public static object PropertyValue(this HUMEditorTypes.Data.Window window, string name)
        {
            return window.window.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(window.window);
        }

        /// <summary>
        /// Gets a properties value on a window via reflection.
        /// </summary>
        public static T PropertyValue<T>(this HUMEditorTypes.Data.Window window, string name)
        {
            return (T)window.window.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(window.window);
        }

        /// <summary>
        /// Gets a property on any object via reflection.
        /// </summary>
        public static PropertyInfo Property(this object obj, string name)
        {
            return obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets a properties value on any object via reflection.
        /// </summary>
        public static object PropertyValue(this object obj, string name)
        {
            return obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
        }

        /// <summary>
        /// Gets a properties value on any object via reflection.
        /// </summary>
        public static T PropertyValue<T>(this object obj, string name)
        {
            return (T)obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
        }

        /// <summary>
        /// Gets a property on a host view window via reflection.
        /// </summary>
        public static PropertyInfo Property(this HUMEditorTypes.Data.GetHostView hostView, string name)
        {
            return hostView.window.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets a properties value on a host view window via reflection.
        /// </summary>
        public static object PropertyValue(this HUMEditorTypes.Data.GetHostView hostView, string name)
        {
            return hostView.window.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(hostView.window);
        }

        /// <summary>
        /// Gets a properties value on a host view window via reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostView"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T PropertyValue<T>(this HUMEditorTypes.Data.GetHostView hostView, string name)
        {
            return (T)hostView.window.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(hostView.window);
        }

        /// <summary>
        /// Invokes a method of name on any object via reflection.
        /// </summary>
        public static T Invoke<T>(this object obj, string name, bool onParent = false, params object[] parameters)
        {
            return (T)obj.Invoke(name, onParent, parameters);
        }

        public static object Invoke(this object obj, string name, bool onParent = false, params object[] parameters)
        {
            return obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(obj, parameters);
        }

        /// <summary>
        /// Gets the host view of this window.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static object HostView(this HUMEditorTypes.Data.Window window)
        {
            return FieldValue(window, "m_Parent");
        }

        /// <summary>
        /// Starts the process of getting a host view from a window and doing something with it.
        /// </summary>
        public static HUMEditorTypes.Data.GetHostView HostView(this HUMEditorTypes.Data.Get get, EditorWindow window)
        {
            return new HUMEditorTypes.Data.GetHostView(get, window);
        }

        /// <summary>
        /// The inner view of the window. For instance, inside tab window, its below the tab area.
        /// </summary>
        public static object ActualView(this HUMEditorTypes.Data.GetHostView hostView)
        {
            return PropertyValue(hostView, "m_ActualView");
        }

        /// <summary>
        /// Starts the process of getting the main window and doing something.
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        public static HUMEditorTypes.Data.GetMain Main(this HUMEditorTypes.Data.Get get)
        {
            return new HUMEditorTypes.Data.GetMain(get);
        }

        /// <summary>
        /// Starts the process of getting the main window and doing something.
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        public static HUMEditorTypes.Data.GetGUIView GUIView(this HUMEditorTypes.Data.Get get)
        {
            return new HUMEditorTypes.Data.GetGUIView(get);
        }

        private static object _mainToolbar;
        private static object _guiView;

        private static object GetMainToolbar()
        {
            var type = Assembly.GetAssembly(typeof(EditorWindow)).GetType("UnityEditor.MainView", true, false);
            var instance = Resources.FindObjectsOfTypeAll(type)[0];
            var children = (IEnumerable)type.GetProperty("children").GetValue(instance);
            var childrenEnumerator = children.GetEnumerator();
            childrenEnumerator.MoveNext();
            return childrenEnumerator.Current;
        }

        public static object GUIView(this HUMEditorTypes.Data.GetGUIView main)
        {
            if (_guiView == null) _guiView = GetGUIView();
            return _guiView;
        }

        public static Rect Position(this HUMEditorTypes.Data.GetGUIView main)
        {
            return GetGUIView().PropertyValue<Rect>("windowPosition");
        }

        private static object GetGUIView()
        {
            var type = Assembly.GetAssembly(typeof(EditorWindow)).GetType("UnityEditor.GUIView", true, false);
            var instance = Resources.FindObjectsOfTypeAll(type)[0];
            return instance;
        }

        public static object Toolbar(this HUMEditorTypes.Data.GetMain main)
        {
            if (_mainToolbar == null) _mainToolbar = GetMainToolbar();
            return _mainToolbar;
        }

        public static Rect Position(this HUMEditorTypes.Data.GetMain main)
        {
            var type = Assembly.GetAssembly(typeof(EditorWindow)).GetType("UnityEditor.Toolbar", true, false);
            var instance = Resources.FindObjectsOfTypeAll(type)[0];
            return instance.Invoke<Rect>("GetToolbarPosition");
        }

        private static object _mainStatusBar;

        private static object GetMainStatusBar()
        {
            var type = Assembly.GetAssembly(typeof(EditorWindow)).GetType("UnityEditor.MainView", true, false);
            var instance = Resources.FindObjectsOfTypeAll(type)[0];
            var children = (IEnumerable)type.GetProperty("children").GetValue(instance);
            var childrenEnumerator = children.GetEnumerator();
            childrenEnumerator.MoveNext();
            childrenEnumerator.MoveNext();
            childrenEnumerator.MoveNext();
            return childrenEnumerator.Current;
        }

        /// <summary>
        /// Gets an editor type via reflection.
        /// </summary>
        public static Type EditorType(this HUMEditorTypes.Data.Get get, string typeFullName)
        {
            return Assembly.GetAssembly(typeof(EditorWindow)).GetType(typeFullName, true, false);
        }

        public static object StatusBar(this HUMEditorTypes.Data.GetMain main)
        {
            if (_mainStatusBar == null) _mainStatusBar = GetMainStatusBar();
            return _mainStatusBar;
        }

        private static object _mainDockArea;

        private static object GetMainDockArea()
        {
            var type = Assembly.GetAssembly(typeof(EditorWindow)).GetType("UnityEditor.MainView", true, false);
            var instance = Resources.FindObjectsOfTypeAll(type)[0];
            var children = (IEnumerable)type.GetProperty("children").GetValue(instance);
            var childrenEnumerator = children.GetEnumerator();
            childrenEnumerator.MoveNext();
            childrenEnumerator.MoveNext();
            return childrenEnumerator.Current;
        }

        public static object DockArea(this HUMEditorTypes.Data.GetMain main)
        {
            if (_mainDockArea == null) _mainDockArea = GetMainDockArea();
            return _mainDockArea;
        }
    }
}
