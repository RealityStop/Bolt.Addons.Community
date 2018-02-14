using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.UnitTools.Events
{
    [TypeIcon(typeof(Event))]
    [Inspectable]
    public class MethodEventManager : MonoBehaviour
    {
        [Inspectable]
        public AotDictionary methodEventListeners = new AotDictionary();

        public void Initialize(out bool complete)
        {
            if (gameObject)
            complete = true;
        }

        public void Clear()
        {
            methodEventListeners.Clear();
        }

        public MethodEventListener FindMethodEventListener(string @name, bool createListener)
        {
            if (!methodEventListeners.Contains(name) && createListener)
            {
                methodEventListeners.Add(name, new MethodEventListener(name));
                return ((MethodEventListener)methodEventListeners[name]);
            }
            else
            {
                return (MethodEventListener)methodEventListeners[name];
            }
        }

        public void RemoveMethodEventManager()
        {
            Destroy(this);
        }


    }
}
