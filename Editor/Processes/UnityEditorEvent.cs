using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class UnityEditorEvent : DeserializedRoutine
    {
        public static UnityEditorEvent instance { get; private set; }
        public static event Action<Event> onCurrentEvent = (e)=> { };

        public override void Run()
        {
            instance = this;
            FieldInfo fieldInfo = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
            EditorApplication.CallbackFunction callback = fieldInfo.GetValue(null) as EditorApplication.CallbackFunction;
            callback += UpdateActiveEvent;
            fieldInfo.SetValue(null, callback);
        }

        public void UpdateActiveEvent()
        {
            onCurrentEvent(Event.current);
        }
    }
}