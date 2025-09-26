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
using static Unity.VisualScripting.Round<float, float>;
using System.Reflection;
using UnityEngine.AI;

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

        public static bool MachineIs(SMachine target, ScriptGraphAsset asset)
        {
            if (target == null) return false;

            var macro = target.nest.macro;

            if (macro != null) return macro == asset;

            return false;
        }

        public static IList MergeLists(params IList[] lists)
        {
            List<object> mergedList = new List<object>();

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

        private static readonly HashSet<(GameObject target, EventHook hook, string eventID)> registeredEvents
            = new HashSet<(GameObject target, EventHook hook, string eventID)>();

        public static void RegisterCustomEvent(GameObject target, Action<CustomEventArgs> action, string eventID)
        {
            var hook = new EventHook(EventHooks.Custom, target);

            var key = (target, hook, eventID);

            if (registeredEvents.Add(key))
            {
                EventBus.Register(hook, action);
            }
        }

        public static void RegisterReturnEvent(GameObject target, Action<ReturnEventArg> action, string name, int argCount, string eventID)
        {
            var hook = CommunityEvents.ReturnEvent;

            var key = (target, hook, eventID);

            if (registeredEvents.Add(key))
            {
                void Action(ReturnEventArg arg)
                {
                    bool should = name == arg.name;

                    if ((arg.isCallback ? argCount == arg.arguments.Length : argCount + 1 == arg.arguments.Length || (argCount == 0 && arg.arguments.Length == 0)) && should)
                    {
                        if (arg.global)
                        {
                            action(arg);
                        }
                        else
                        {
                            if (arg.target != null && arg.target == target) action(arg);
                        }
                    }
                }
                EventBus.Register(hook, (Action<ReturnEventArg>)Action);
            }
        }

        public static void ClearSavedVariables()
        {
            SavedVariables.saved.Clear();
            SavedVariables.SaveDeclarations(SavedVariables.saved);
        }

        public static void ResetSavedVariable(string key)
        {
            var initalvariable = SavedVariables.initial.GetDeclaration(key).CloneViaFakeSerialization();
            SavedVariables.saved[key] = initalvariable.value;

            if (SavedVariables.current != SavedVariables.initial) SavedVariables.current[key] = initalvariable.value;
            SavedVariables.SaveDeclarations(SavedVariables.saved);
        }

        public static TGraphAsset GetGraph<TGraph, TGraphAsset, TMachine>(GameObject target)
        where TGraph : class, IGraph, new()
        where TGraphAsset : Macro<TGraph>
        where TMachine : Machine<TGraph, TGraphAsset>
        {
            return target.GetComponent<TMachine>().nest.macro;
        }

        public static List<TGraphAsset> GetGraphs<TGraph, TGraphAsset, TMachine>(GameObject target)
        where TGraph : class, IGraph, new()
        where TGraphAsset : Macro<TGraph>
        where TMachine : Machine<TGraph, TGraphAsset>
        {
            return target.GetComponents<TMachine>()
                .Where(machine => target.GetComponent<TMachine>().nest.macro != null)
                .Select(machine => machine.nest.macro)
                .ToList();
        }

        public static void SetGraph<TGraph, TMacro, TMachine>(UnityEngine.Object target, TMacro macro)
        where TGraph : class, IGraph, new()
        where TMacro : Macro<TGraph>
        where TMachine : Machine<TGraph, TMacro>
        {
            if (target is GameObject go)
            {
                go.GetComponent<TMachine>().nest.SwitchToMacro(macro);
            }
            else if (target is TMachine machine)
            {
                machine.nest.SwitchToMacro(macro);
            }
        }

        public static bool HasGraph<TGraph, TMacro, TMachine>(UnityEngine.Object target, TMacro macro)
        where TGraph : class, IGraph, new()
        where TMacro : Macro<TGraph>
        where TMachine : Machine<TGraph, TMacro>
        {
            if (target is GameObject gameObject)
            {
                if (gameObject != null)
                {
                    var stateMachines = gameObject.GetComponents<TMachine>();

                    return stateMachines
                        .Where(currentMachine => currentMachine != null)
                        .Any(currentMachine => currentMachine.graph != null && currentMachine.graph.Equals(macro.graph));
                }
            }
            else if (target is TMachine machine)
            {
                if (machine.graph != null && machine.graph.Equals(macro.graph))
                {
                    return true;
                }
            }
            return false;
        }

#if MODULE_AI_EXISTS
        public static bool DestinationReached(GameObject gameObject, float threshold, bool requireSuccess)
        {
            var navMeshAgent = gameObject.GetComponent<NavMeshAgent>();

            return navMeshAgent != null &&
                navMeshAgent.remainingDistance <= threshold &&
                (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete || !requireSuccess);
        }
#endif
        public static (float pre, float post) Increment(float value)
        {
            float pre = value;
            float post = value + 1;
            return (pre, post);
        }

        public static (int pre, int post) Increment(int value)
        {
            int pre = value;
            int post = value + 1;
            return (pre, post);
        }

        public static (float pre, float post) Decrement(float value)
        {
            float pre = value;
            float post = value - 1;
            return (pre, post);
        }

        public static (int pre, int post) Decrement(int value)
        {
            int pre = value;
            int post = value - 1;
            return (pre, post);
        }

        public static object GetArgument(this CustomEventArgs args, int index, Type targetType)
        {
            return args.arguments[index].ConvertTo(targetType);
        }

        public static object GetArgument(this ReturnEventArg args, int index, Type targetType)
        {
            return args.arguments[index].ConvertTo(targetType);
        }

        public static void TriggerAssetCustomEvent(ScriptGraphAsset asset, string name, params object[] args)
        {
            GraphReference.New(asset, true).TriggerEventHandler(hook => hook == "Custom", new CustomEventArgs(name, args), parent => true, true);
        }

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
                    fieldInfo.SetValue(eventInstance, parameters[i].ConvertTo(fieldInfo.FieldType));
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    propertyInfo.SetValue(eventInstance, parameters[i].ConvertTo(propertyInfo.PropertyType));
                }
            }
            return eventInstance;
        }

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
            return unscaled ? (object)new WaitForSecondsRealtime(time) : new WaitForSeconds(time);
        }

        public static bool Chance(float probability)
        {
            probability = Mathf.Clamp01(probability / 100f);
            return UnityEngine.Random.value <= probability;
        }

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
            if (!(variableValue is IDictionary))
            {
                throw new ArgumentException($"Indicated variable '{name}' is not a dictionary.");
            }
            return variableValue as IDictionary;
        }

        /// <summary>
        /// Merges two or more dictionaries together.
        /// </summary>
        /// <remarks>
        /// If the same key is found more than once, only the value
        /// of the first dictionary with this key will be used.
        /// </remarks>
        public static AotDictionary MergeDictionaries(params IDictionary[] dictionaries)
        {
            AotDictionary mergedDictionary = new AotDictionary();

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
            Dictionary<TKey, TValue> mergedDictionary = new Dictionary<TKey, TValue>();

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
            Dictionary<Tkey, TValue> mergedDictionary = new Dictionary<Tkey, TValue>();

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

        public static Vector2 CalculateAverage(params Vector2[] values)
        {
            if (values.Length == 0)
            {
                return new Vector2();
            }

            Vector2 sum = new Vector2();
            foreach (Vector2 value in values)
            {
                sum += value;
            }

            return sum / values.Length;
        }

        public static Vector3 CalculateAverage(params Vector3[] values)
        {
            if (values.Length == 0)
            {
                return new Vector3();
            }

            Vector3 sum = new Vector3();
            foreach (Vector3 value in values)
            {
                sum += value;
            }

            return sum / values.Length;
        }

        public static Vector4 CalculateAverage(params Vector4[] values)
        {
            if (values.Length == 0)
            {
                return new Vector4();
            }

            Vector4 sum = new Vector4();
            foreach (Vector4 value in values)
            {
                sum += value;
            }

            return sum / values.Length;
        }
        #endregion

        #region ListAverage
        public static float CalculateListAverage(List<float> values)
        {
            if (values.Count == 0)
            {
                return 0f;
            }

            float sum = 0f;
            foreach (float value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }

        public static Vector2 CalculateListAverage(List<Vector2> values)
        {
            if (values.Count == 0)
            {
                return new Vector2();
            }

            Vector2 sum = new Vector2();
            foreach (Vector2 value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }

        public static Vector3 CalculateListAverage(List<Vector3> values)
        {
            if (values.Count == 0)
            {
                return new Vector3();
            }

            Vector3 sum = new Vector3();
            foreach (Vector3 value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }

        public static Vector4 CalculateListAverage(List<Vector4> values)
        {
            if (values.Count == 0)
            {
                return new Vector4();
            }

            Vector4 sum = new Vector4();
            foreach (Vector4 value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }
        #endregion

        #region Maximum
        public static float CalculateMax(params float[] values)
        {
            return values.Length == 0 ? 0f : values.Max();
        }
        public static Vector2 CalculateMax(params Vector2[] values)
        {
            Vector2? maximum = null;

            foreach (var value in values)
            {
                maximum = maximum.HasValue ? Vector2.Max(maximum.Value, value) : value;
            }

            return maximum ?? Vector2.zero;
        }
        public static Vector3 CalculateMax(params Vector3[] values)
        {
            Vector3? maximum = null;

            foreach (var value in values)
            {
                maximum = maximum.HasValue ? Vector3.Max(maximum.Value, value) : value;
            }

            return maximum ?? Vector3.zero;
        }
        public static Vector4 CalculateMax(params Vector4[] values)
        {
            Vector4? maximum = null;

            foreach (var value in values)
            {
                maximum = maximum.HasValue ? Vector4.Max(maximum.Value, value) : value;
            }

            return maximum ?? Vector4.zero;
        }
        #endregion

        #region Minimum
        public static float CalculateMin(params float[] values)
        {
            return values.Length == 0 ? 0f : values.Min();
        }
        public static Vector2 CalculateMin(params Vector2[] values)
        {
            Vector2? minimum = null;

            foreach (var value in values)
            {
                minimum = minimum.HasValue ? Vector2.Min(minimum.Value, value) : value;
            }

            return minimum ?? Vector2.zero;
        }
        public static Vector3 CalculateMin(params Vector3[] values)
        {
            Vector3? minimum = null;

            foreach (var value in values)
            {
                minimum = minimum.HasValue ? Vector3.Min(minimum.Value, value) : value;
            }

            return minimum ?? Vector3.zero;
        }
        public static Vector4 CalculateMin(params Vector4[] values)
        {
            Vector4? minimum = null;

            foreach (var value in values)
            {
                minimum = minimum.HasValue ? Vector4.Min(minimum.Value, value) : value;
            }

            return minimum ?? Vector4.zero;
        }
        #endregion

        #region Absolute
        public static float Absolute(float value)
        {
            return Mathf.Abs(value);
        }
        public static Vector2 Absolute(Vector2 value)
        {
            return new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y));
        }
        public static Vector3 Absolute(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }
        public static Vector4 Absolute(Vector4 value)
        {
            return new Vector4(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z), Mathf.Abs(value.w));
        }
        #endregion

        #region Round
        public static float Round(float value, Rounding rounding)
        {
            return rounding switch
            {
                Rounding.Floor => Mathf.Floor(value),
                Rounding.Ceiling => Mathf.Ceil(value),
                Rounding.AwayFromZero => Mathf.Round(value),
                _ => value,
            };
        }

        public static Vector2 Round(Vector2 value, Rounding rounding)
        {
            return rounding switch
            {
                Rounding.Floor => new Vector2(Mathf.Floor(value.x), Mathf.Floor(value.y)),
                Rounding.Ceiling => new Vector2(Mathf.Ceil(value.x), Mathf.Ceil(value.y)),
                Rounding.AwayFromZero => new Vector2(Mathf.Round(value.x), Mathf.Round(value.y)),
                _ => value,
            };
        }

        public static Vector3 Round(Vector3 value, Rounding rounding)
        {
            return rounding switch
            {
                Rounding.Floor => new Vector3(Mathf.Floor(value.x), Mathf.Floor(value.y), Mathf.Floor(value.z)),
                Rounding.Ceiling => new Vector3(Mathf.Ceil(value.x), Mathf.Ceil(value.y), Mathf.Ceil(value.z)),
                Rounding.AwayFromZero => new Vector3(Mathf.Round(value.x), Mathf.Round(value.y), Mathf.Round(value.z)),
                _ => value,
            };
        }

        public static Vector4 Round(Vector4 value, Rounding rounding)
        {
            return rounding switch
            {
                Rounding.Floor => new Vector4(Mathf.Floor(value.x), Mathf.Floor(value.y), Mathf.Floor(value.z), Mathf.Floor(value.w)),
                Rounding.Ceiling => new Vector4(Mathf.Ceil(value.x), Mathf.Ceil(value.y), Mathf.Ceil(value.z), Mathf.Ceil(value.w)),
                Rounding.AwayFromZero => new Vector4(Mathf.Round(value.x), Mathf.Round(value.y), Mathf.Round(value.z), Mathf.Round(value.w)),
                _ => value,
            };
        }
        #endregion

        #region Normalize
        public static float Normalize(float value)
        {
            return value == 0f ? 0f : value / Mathf.Abs(value);
        }

        public static Vector2 Normalize(Vector2 value)
        {
            return value.normalized;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            return Vector3.Normalize(value);
        }

        public static Vector4 Normalize(Vector4 value)
        {
            return Vector4.Normalize(value);
        }
        #endregion

        #region Project
        public static Vector2 Project(Vector2 a, Vector2 b)
        {
            return Vector2.Dot(a, b) * b.normalized;
        }

        public static Vector3 Project(Vector3 a, Vector3 b)
        {
            return Vector3.Project(a, b);
        }
        #endregion

        #region Root
        public static float Root(float radicand, float degree)
        {
            if (degree == 2)
            {
                return Mathf.Sqrt(radicand);
            }
            else
            {
                return Mathf.Pow(radicand, 1 / degree);
            }
        }
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
            {
                IList list;
                if (aotList)
                    list = new AotList();
                else if (integers)
                    list = new List<int>();
                else
                    list = new List<float>();
                return list;
            }

            if (integers && unique && (max - min) < count)
                throw new ArgumentException("Unique integer range is too small for the number of elements requested.");

            IList _list;
            if (aotList)
            {
                _list = new AotList(count);
            }
            else if (integers)
            {
                _list = new List<int>(count);
            }
            else
            {
                _list = new List<float>(count);
            }

            HashSet<object> uniqueness = unique ? new HashSet<object>() : null;

            while (_list.Count < count)
            {
                object value = integers ? UnityEngine.Random.Range((int)min, (int)max) : UnityEngine.Random.Range(min, max);

                if (unique)
                {
                    if (uniqueness.Add(value))
                        _list.Add(value);
                }
                else
                {
                    _list.Add(value);
                }
            }

            return _list;
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
}