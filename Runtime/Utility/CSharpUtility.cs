using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
<<<<<<< Updated upstream
=======
using static Unity.VisualScripting.Round<float, float>;
using System.Reflection;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

namespace Unity.VisualScripting.Community 
{
    public static class CSharpUtility
    {
        public static SMachine GetScriptMachine(GameObject target, ScriptGraphAsset asset)
        {
            var machines = target.GetComponents<SMachine>();
            foreach (var machine in machines)
            {
                if (machine.nest.macro == asset)
                {
                    return machine;
                }
            }
            return null;
        }

        public static SMachine GetScriptMachine(GameObject target, string name)
        {
            var machines = target.GetComponents<SMachine>();
            foreach (var machine in machines)
            {
                if (machine.nest.graph.title == name)
                {
                    return machine;
                }
            }
            return null;
        }

        public static SMachine[] GetScriptMachines(GameObject target, ScriptGraphAsset asset)
        {
            var machines = target.GetComponents<SMachine>();
            var _machines = new List<SMachine>();
            foreach (var machine in machines)
            {
                if (machine.nest.macro == asset)
                {
                    _machines.Add(machine);
                }
            }

            return _machines.ToArrayPooled();
        }

        public static IList MergeLists(params IList[] lists)
        {
            List<object> mergedList = new();
    
            foreach (System.Collections.IList list in lists)
            {
                foreach (var item in list)
                {
                    mergedList.Add(item);
                }
            }
    
            return mergedList;
        }
    
        public static List<T> MergeLists<T>(params IEnumerable<object>[] lists)
        {
            var mergedList = new List<T>();
    
            foreach (var list in lists)
            {
                foreach (var item in list)
                {
                    if (item is T convertedItem)
                    {
                        mergedList.Add(convertedItem);
                    }
                    else
                    {
                        Debug.LogWarning($"{item} is not {typeof(T).As().CSharpName(false, true, false)}, skipping.");
                    }
                }
            }
    
            return mergedList;
        }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
        public static object ConvertType<T>(this T value, Type type)
        {
            if (value.IsConvertibleTo(type, true))
            {
                return value.ConvertTo(type);
            }
            else return value;
        }
    
        private static readonly HashSet<(GameObject, EventHook, System.Action<CustomEventArgs>)> registeredEvents = new HashSet<(GameObject, EventHook, System.Action<CustomEventArgs>)>();
    
        public static void RegisterCustomEvent(GameObject target, System.Action<CustomEventArgs> action)
        {
            var hook = new EventHook(EventHooks.Custom, target);
            var eventKey = (target, hook, action);
    
            if (!registeredEvents.Contains(eventKey))
=======

        private static readonly HashSet<(GameObject target, EventHook hook, string eventID)> registeredEvents
            = new();

        public static void RegisterCustomEvent(GameObject target, Action<CustomEventArgs> action, string eventID)
        {
            var hook = new EventHook(EventHooks.Custom, target);
=======

        private static readonly HashSet<(GameObject target, EventHook hook, string eventID)> registeredEvents
            = new();

        public static void RegisterCustomEvent(GameObject target, Action<CustomEventArgs> action, string eventID)
        {
            var hook = new EventHook(EventHooks.Custom, target);
>>>>>>> Stashed changes
=======

        private static readonly HashSet<(GameObject target, EventHook hook, string eventID)> registeredEvents
            = new();

        public static void RegisterCustomEvent(GameObject target, Action<CustomEventArgs> action, string eventID)
        {
            var hook = new EventHook(EventHooks.Custom, target);
>>>>>>> Stashed changes
=======

        private static readonly HashSet<(GameObject target, EventHook hook, string eventID)> registeredEvents
            = new();

        public static void RegisterCustomEvent(GameObject target, Action<CustomEventArgs> action, string eventID)
        {
            var hook = new EventHook(EventHooks.Custom, target);
>>>>>>> Stashed changes

            var key = (target, hook, eventID);

            if (registeredEvents.Add(key))
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            {
                EventBus.Register(hook, action);
            }
        }
    
        public static object GetArgument(this CustomEventArgs args, int index, Type targetType)
        {
            return args.arguments[index].ConvertType(targetType);
        }
<<<<<<< Updated upstream
    
=======

        public static T CreateDefinedEventInstance<T>(params object[] parameters)
        {
            var info = ReflectedInfo.For(typeof(T));
            var eventInstance = (T)System.Activator.CreateInstance(typeof(T));
            var members = info.reflectedFields.Select(v => v.Value).Cast<MemberInfo>().Concat(info.reflectedProperties.Select(v => v.Value)).ToListPooled();
            for (var i = 0; i < parameters.Length; i++)
            {
                var member = members[i];
                if (member is FieldInfo fieldInfo)
                {
                    fieldInfo.SetValueOptimized(eventInstance, parameters[i].ConvertTo(fieldInfo.FieldType));
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    propertyInfo.SetValueOptimized(eventInstance, parameters[i].ConvertTo(propertyInfo.PropertyType));
                }
            }
            return eventInstance;
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        public static T CreateDefinedEventInstance<T>(params object[] parameters)
        {
            var info = ReflectedInfo.For(typeof(T));
            var eventInstance = (T)System.Activator.CreateInstance(typeof(T));
            var members = info.reflectedFields.Select(v => v.Value).Cast<MemberInfo>().Concat(info.reflectedProperties.Select(v => v.Value)).ToListPooled();
            for (var i = 0; i < parameters.Length; i++)
            {
                var member = members[i];
                if (member is FieldInfo fieldInfo)
                {
                    fieldInfo.SetValueOptimized(eventInstance, parameters[i].ConvertTo(fieldInfo.FieldType));
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    propertyInfo.SetValueOptimized(eventInstance, parameters[i].ConvertTo(propertyInfo.PropertyType));
                }
            }
            return eventInstance;
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        public static void Bind(this IDelegate @delegate, IDelegate delegateToBind)
        {
            @delegate.Bind(delegateToBind);
        }
    
        public static void Bind(this IDelegate @delegate, Delegate delegateToBind)
        {
            @delegate.Combine(delegateToBind);
        }
    
        public static object CreateWaitForSeconds(float time, bool unscaled)
        {
            return unscaled ? new WaitForSecondsRealtime(time) : new WaitForSeconds(time);
        }
    
        public static bool Chance(float probability)
        {
            probability = Mathf.Clamp01(probability / 100f);
            return UnityEngine.Random.value <= probability;
        }
<<<<<<< Updated upstream
    
=======

        public static bool GetKeyAction(KeyCode key, PressState pressState)
        {
            return pressState switch
            {
                PressState.Up => Input.GetKeyUp(key),
                PressState.Down => Input.GetKeyDown(key),
                PressState.Hold => Input.GetKey(key),
                _ => throw new UnexpectedEnumValueException<PressState>(pressState)
            };
        }

        public static bool GetButtonAction(string button, PressState pressState)
        {
            return pressState switch
            {
                PressState.Up => Input.GetButtonUp(button),
                PressState.Down => Input.GetButtonDown(button),
                PressState.Hold => Input.GetButton(button),
                _ => throw new UnexpectedEnumValueException<PressState>(pressState)
            };
        }

        public static IDictionary GetDictionaryVariable(this VariableDeclarations declarations, string name)
        {
            var variableValue = declarations.Get(name) ?? throw new ArgumentException($"Indicated variable '{name}' does not exist."); ;
            if (variableValue is not IDictionary dictionary)
            {
                throw new ArgumentException($"Indicated variable '{name}' is not a dictionary.");
            }
            return dictionary;
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        /// <summary>
        /// Merges two or more dictionaries together.
        /// </summary>
        /// <remarks>
        /// If the same key is found more than once, only the value
        /// of the first dictionary with this key will be used.
        /// </remarks>
        public static AotDictionary MergeDictionaries(params IDictionary[] dictionaries)
        {
            AotDictionary mergedDictionary = new();
    
            foreach (var dictionary in dictionaries)
            {
                foreach (var key in dictionary.Keys)
                {
                    if (!mergedDictionary.Contains(key))
                    {
                        mergedDictionary.Add(key, dictionary[key]);
                    }
                }
            }
            return mergedDictionary;
        }
    
        /// <summary>
        /// Merges two or more dictionaries together.
        /// </summary>
        /// <remarks>
        /// If the same key is found more than once, only the value
        /// of the first dictionary with this key will be used.
        /// </remarks>
        public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(params IDictionary[] dictionaries)
        {
            Dictionary<TKey, TValue> mergedDictionary = new();
    
            foreach (var dictionary in dictionaries)
            {
                foreach (var key in dictionary.Keys)
                {
                    if (key is TKey convertedKey)
                    {
                        if (!mergedDictionary.ContainsKey(convertedKey))
                        {
                            if (dictionary[key] is TValue convertedValue)
                            {
                                mergedDictionary.Add(convertedKey, convertedValue);
                            }
                            else
                            {
                                Debug.LogWarning($"{dictionary[key]} is not {typeof(TValue).As().CSharpName(false, true, false)}, skipping.");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{key} is not {typeof(TKey).As().CSharpName(false, true, false)}, skipping.");
                    }
                }
            }
            return mergedDictionary;
        }
    
        /// <summary>
        /// Merges two or more dictionaries together.
        /// </summary>
        /// <remarks>
        /// If the same key is found more than once, it will be
        /// replaced with the latest key.
        /// </remarks>
        public static Dictionary<Tkey, TValue> MergeDictionariesReplace<Tkey, TValue>(params Dictionary<Tkey, TValue>[] dictionaries)
        {
            Dictionary<Tkey, TValue> mergedDictionary = new();
    
            foreach (var dictionary in dictionaries)
            {
                foreach (var key in dictionary.Keys)
                {
                    mergedDictionary[key] = dictionary[key];
                }
            }
    
            return mergedDictionary;
        }
    
        /// <summary>
        /// Merges two or more dictionaries together.
        /// </summary>
        /// <remarks>
        /// If the same key is found more than once, it will be
        /// replaced with the latest key.
        /// </remarks>
        public static IDictionary MergeDictionariesReplace(params IDictionary[] dictionaries)
        {
            IDictionary mergedDictionary = new Dictionary<object, object>();
    
            foreach (var dictionary in dictionaries)
            {
                foreach (var key in dictionary.Keys)
                {
                    mergedDictionary[key] = dictionary[key];
                }
            }
    
            return mergedDictionary;
        }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

        public static float GetTime(bool unscaled)
        {
            return !unscaled ? Time.deltaTime : Time.unscaledDeltaTime;
        }

        #region Math Functions
        // Using these methods because the Class and Methods are internal
        public static float ExponentialFunction(float value, float minWeight, float exponent, float scale)
        {
            return MathLibrary.ExponentialFunction(value, minWeight, exponent, scale);
        }
        public static float ExponentialFunctionOfRange(float value, float minValue, float maxValue, float minWeight, float exponent, float scale)
        {
            return MathLibrary.ExponentialFunctionOfRange(value, minValue, maxValue, minWeight, exponent, scale);
        }
        public static float DecayFunction(float value, float minWeight, float decayFactor, float scale)
        {
            return MathLibrary.DecayFunction(value, minWeight, decayFactor, scale);
        }
        public static float DecayFunctionOfRange(float value, float minValue, float maxValue, float minWeight, float decayFactor, float scale)
        {
            return MathLibrary.DecayFunctionOfRange(value, minValue, maxValue, minWeight, decayFactor, scale);
        }
        public static float LinearFunction(float input, float min, float max)
        {
            return MathLibrary.LinearFunction(input, min, max);
        }
        public static float LinearFunctionOfRange(float input, float minValue, float maxValue, float minimum, float maximum)
        {
            return MathLibrary.LinearFunctionOfRange(input, minValue, maxValue, minimum, maximum);
        }
        public static float ReverseLinearFunction(float input, float min, float max)
        {
            return MathLibrary.ReverseLinearFunction(input, min, max);
        }
        public static float LogarithmicFunction(float value, float minimum, float exponent, float scale)
        {
            return MathLibrary.LogarithmicFunction(value, minimum, exponent, scale);
        }
        public static float LogarithmicFunctionOfRange(float value, float minValue, float maxValue, float minWeight, float exponent, float scale)
        {
            return MathLibrary.LogarithmicFunctionOfRange(value, minValue, maxValue, minWeight, exponent, scale);
        }
        public static float DecayingSigmoid(float value, float inflectionPoint, float minWeight, float decayFactor, float scale)
        {
            return MathLibrary.DecayingSigmoid(value, inflectionPoint, minWeight, decayFactor, scale);
        }
        public static float DecayingSigmoidOfRange(float value, float minValue, float maxValue, float inflectionPoint, float minWeight, float decayFactor, float scale)
        {
            return MathLibrary.DecayingSigmoidOfRange(value, minValue, maxValue, inflectionPoint, minWeight, decayFactor, scale);
        }
        #endregion

        // Using these methods because its cleaner for the Generator

        #region Average
>>>>>>> Stashed changes
        public static float CalculateAverage(params float[] values)
        {
            if (values.Length == 0)
            {
                return 0f;
            }
    
            float sum = 0f;
            foreach (float value in values)
            {
                sum += value;
            }
    
            return sum / values.Length;
        }
    
        public static float CalculateMax(params float[] values)
        {
            if (values.Length == 0)
            {
                return 0f;
            }
    
            var value = values.Max();
    
            return value;
        }
    
        public static float CalculateMin(params float[] values)
        {
            if (values.Length == 0)
            {
                return 0f;
            }
    
            var value = values.Min();
    
            return value;
        }
    
        public static float Normalize(float value)
        {
            if (value == 0)
            {
                return 0f;
            }
    
            return value / Mathf.Abs(value);
        }
<<<<<<< Updated upstream
    } 
=======
        #endregion

        #region String
        public static string ReverseString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            char[] chars = input.ToCharArray();
            int left = 0;
            int right = chars.Length - 1;

            while (left < right)
            {
                (chars[left], chars[right]) = (chars[right], chars[left]);
                left++;
                right--;
            }

            return new string(chars);
        }

        public static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
        }

        #endregion

        public static IList RandomNumbers(int count, float min, float max, bool integers = false, bool aotList = false, bool unique = true)
        {
            if (count <= 0)
                return aotList ? new AotList() : integers ? new List<int>() : new List<float>();

            if (integers && unique && (max - min) < count)
                throw new ArgumentException("Unique integer range is too small for the number of elements requested.");

            IList list = aotList ? new AotList(count) : integers ? new List<int>(count) : new List<float>(count);

            HashSet<object> uniqueness = unique ? new HashSet<object>() : null;

            while (list.Count < count)
            {
                object value = integers ? UnityEngine.Random.Range((int)min, (int)max) : UnityEngine.Random.Range(min, max);

                if (unique)
                {
                    if (uniqueness.Add(value))
                        list.Add(value);
                }
                else
                {
                    list.Add(value);
                }
            }

            return list;
        }

        public static (object key, object value) GetRandomElement(IEnumerable collection, bool isDictionary = false)
        {
            if (collection == null)
            {
                Debug.LogWarning("Collection is null. Returning null.");
                return (null, null);
            }

            if (collection is IList list)
            {
                if (list.Count == 0)
                {
                    Debug.LogWarning("Collection is empty. Returning null.");
                    return (null, null);
                }

                if (isDictionary)
                {
                    Debug.LogWarning("Requested dictionary mode, but received list. Key will be null.");
                }

                var index = UnityEngine.Random.Range(0, list.Count);
                return (null, list[index]);
            }

            if (collection is IDictionary dictionary)
            {
                if (dictionary.Count == 0)
                {
                    Debug.LogWarning("Dictionary is empty. Returning null.");
                    return (null, null);
                }

                var keys = dictionary.Keys.Cast<object>().ToList();
                var randomKey = keys[UnityEngine.Random.Range(0, keys.Count)];
                var value = dictionary[randomKey];
                return (isDictionary ? randomKey : null, value);
            }

            var casted = collection.Cast<object>().ToList();
            if (casted.Count == 0)
            {
                Debug.LogWarning("Enumerable is empty. Returning null.");
                return (null, null);
            }

            if (isDictionary)
            {
                Debug.LogWarning("Requested dictionary mode, but received non-dictionary collection. Key will be null.");
            }

            var item = casted[UnityEngine.Random.Range(0, casted.Count)];
            return (null, item);
        }

        public static bool IsWithin(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static bool Equal(params object[] values)
        {
            if (values == null || values.Length <= 1)
                return true;

            object reference = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (!OperatorUtility.Equal(reference, values[i]))
                    return false;
            }
            return true;
        }
    }
>>>>>>> Stashed changes
}