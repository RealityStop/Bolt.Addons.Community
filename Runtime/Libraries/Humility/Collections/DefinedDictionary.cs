using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    [Serializable]
    public class DefinedDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        public Dictionary<TKey, TValue> previous = new Dictionary<TKey, TValue>();
        public Dictionary<TKey, TValue> current = new Dictionary<TKey, TValue>();
        [SerializeField]
        private List<string> previousSerializedKeys = new List<string>();
        [SerializeField]
        private List<string> previousSerializedValues = new List<string>();
        [SerializeField]
        private List<string> currentSerializedKeys = new List<string>();
        [SerializeField]
        private List<string> currentSerializedValues = new List<string>();

        public void DebugCurrent()
        {
            var keys = Keys().ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                Debug.Log(current[keys[i]]);
            }
        }

        public void DebugPrevious()
        {
            var keys = Keys().ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                Debug.Log(current[keys[i]]);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!previous.ContainsKey(key)) previous.Add(key, value);
            if (!current.ContainsKey(key)) current.Add(key, value);
        }

        public void Remove(TKey key)
        {
            if (previous.ContainsKey(key)) previous.Remove(key);
            if (current.ContainsKey(key)) current.Remove(key);
        }

        public void Clear()
        {
            current.Clear();
        }

        public IEnumerable<TKey> Keys()
        {
            return current.Keys;
        }

        public IEnumerable<TValue> Values()
        {
            return current.Values;
        }

        public TValue Define(TKey key, ref bool defined, Func<TValue, TValue> onCreate, Action<TValue> exists)
        {
            var item = current.Define(previous, key, onCreate, exists);
            if (!current.ContainsKey(key))
            {
                defined = true;
            }
            Add(key, item);
            return item; 
        }

        public void Undefine(ref bool removed, Action<TValue> onRemoved)
        {
            var removeAmount = 10;
            var _removed = removed;

            for (int i = 0; i < removeAmount; i++)
            {
                if (i > 0 && !removed) break;
                {
                    current.Undefine(previous, (val) =>
                    {
                        _removed = true;
                        onRemoved(val);
                    });
                }
            }
        }

        public void OnBeforeSerialize()
        {
            previousSerializedKeys.Clear();
            currentSerializedKeys.Clear();
            previousSerializedValues.Clear();
            currentSerializedValues.Clear();
            var previousKeys = previous.Keys.ToList();
            var currentKeys = current.Keys.ToList();
            var previousValues = previous.Values.ToList();
            var currentValues = current.Values.ToList();
            for (int i = 0; i < previousKeys.Count; i++)
            {
                previousSerializedKeys.Add(previousKeys.Serialized());
                previousSerializedValues.Add(previousValues.Serialized());
            }
            for (int i = 0; i < currentKeys.Count; i++)
            {
                currentSerializedKeys.Add(currentKeys.Serialized());
                currentSerializedValues.Add(currentValues.Serialized());
            }
        }

        public void OnAfterDeserialize()
        {
            previous.Clear();
            current.Clear();
            for (int i = 0; i < previousSerializedKeys.Count; i++)
            {
                previous.Add((TKey)previousSerializedKeys[i].Deserialized(), (TValue)previousSerializedValues[i].Deserialized());
            }
            for (int i = 0; i < currentSerializedKeys.Count; i++)
            {
                previous.Add((TKey)currentSerializedKeys[i].Deserialized(), (TValue)currentSerializedValues[i].Deserialized());
            }
        }
    }
}