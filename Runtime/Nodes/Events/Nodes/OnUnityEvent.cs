using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.VisualScripting.Community
{
    internal class OnUnityEventData : EventUnit<EventData>.Data
    {
        public object EventListener { get; set; }
    }

    public class EventData
    {
        public object Value0 { get; set; }
        public object Value1 { get; set; }
        public object Value2 { get; set; }
        public object Value3 { get; set; }
    }

    [UnitTitle("On Unity Event")]
    [UnitCategory("Events")]
    public class OnUnityEvent : EventUnit<EventData>
    {
        protected override bool register => false;

        [DoNotSerialize]
        public ValueInput UnityEvent;

        public Type Type { get; private set; }

        public override IGraphElementData CreateData()
        {
            return new OnUnityEventData();
        }

        protected override void Definition()
        {
            base.Definition();
            UnityEvent = ValueInput<UnityEventBase>("event");

            if (Type != null)
            {
                var genericArguments = Type.GetGenericArguments();
                for (var i = 0; i < genericArguments.Length; i++)
                {
                    ValueOutput(genericArguments[i], $"arg{i}");
                }
            }
        }

        public override void StartListening(GraphStack stack)
        {
            var data = GetData(stack);

            if (data.EventListener != null || !UnityEvent.hasValidConnection) return;

            var stackRef = stack.ToReference();
            UpdatePorts();

            var eventBase = Flow.FetchValue<UnityEventBase>(UnityEvent, stackRef);
            var method = Type.GetMethod(nameof(UnityEngine.Events.UnityEvent.AddListener));
            var delegateType = method?.GetParameters()[0].ParameterType;

            data.EventListener = CreateAction(delegateType, stackRef);

            method?.Invoke(eventBase, new[] { data.EventListener });
        }

        public override void StopListening(GraphStack stack)
        {
            var data = GetData(stack);

            if (data.EventListener == null) return;

            var stackRef = stack.ToReference();
            var eventBase = Flow.FetchValue<UnityEventBase>(UnityEvent, stackRef);
            var method = Type.GetMethod(nameof(UnityEngine.Events.UnityEvent.RemoveListener));
            method?.Invoke(eventBase, new[] { data.EventListener });

            data.EventListener = null;
        }

        public void UpdatePorts()
        {
            Type = GetEventType();
            Define();
        }

        private Type GetEventType()
        {
            var eventType = UnityEvent?.connection?.source?.type;

            while (eventType != null && eventType.BaseType != typeof(UnityEventBase))
            {
                eventType = eventType.BaseType;
            }

            return eventType;
        }

        private object CreateAction(Type delegateType, GraphReference reference)
        {
            var numParams = delegateType.GetGenericArguments().Length;

            if (numParams == 0)
            {
                void Action()
                {
                    Trigger(reference, new EventData());
                }
                return (UnityAction)Action;
            }

#if ENABLE_IL2CPP
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                string methodName;

                if (numParams == 1) methodName = nameof(OneParamHandler);
                else if (numParams == 2) methodName = nameof(TwoParamsHandler);
                else if (numParams == 3) methodName = nameof(ThreeParamsHandler);
                else methodName = nameof(FourParamsHandler);

                var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

                return method?.MakeGenericMethod(delegateType.GetGenericArguments()).Invoke(this, new object[] { reference });
            }
            else if (GetAotSupportMethodsType() is Type type)
            {
                Type aotSupportMethodsType = type;
                var method = aotSupportMethodsType.GetMethods().First(method => method.ReturnType == delegateType);
                return method?.InvokeOptimized(null, new object[] { reference, this });
            }
            else
            {
                throw new InvalidOperationException("Could not find AOT Support Methods");
            }
#else
            string methodName;

            if (numParams == 1) methodName = nameof(OneParamHandler);
            else if (numParams == 2) methodName = nameof(TwoParamsHandler);
            else if (numParams == 3) methodName = nameof(ThreeParamsHandler);
            else methodName = nameof(FourParamsHandler);

            var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

            return method?.MakeGenericMethod(delegateType.GetGenericArguments()).Invoke(this, new object[] { reference });
#endif
        }

        internal UnityAction<T1> OneParamHandler<T1>(GraphReference reference)
        {
            return arg0 =>
            {
                Trigger(reference, new EventData
                {
                    Value0 = arg0
                });
            };
        }

        internal UnityAction<T1, T2> TwoParamsHandler<T1, T2>(GraphReference reference)
        {
            return (arg0, arg1) =>
            {
                Trigger(reference, new EventData
                {
                    Value0 = arg0,
                    Value1 = arg1
                });
            };
        }

        internal UnityAction<T1, T2, T3> ThreeParamsHandler<T1, T2, T3>(GraphReference reference)
        {
            return (arg0, arg1, arg2) =>
            {
                Trigger(reference, new EventData
                {
                    Value0 = arg0,
                    Value1 = arg1,
                    Value2 = arg2
                });
            };
        }

        internal UnityAction<T1, T2, T3, T4> FourParamsHandler<T1, T2, T3, T4>(GraphReference reference)
        {
            return (arg0, arg1, arg2, arg3) =>
            {
                Trigger(reference, new EventData
                {
                    Value0 = arg0,
                    Value1 = arg1,
                    Value2 = arg2,
                    Value3 = arg3
                });
            };
        }

        protected override void AssignArguments(Flow flow, EventData args)
        {
            var numOutputs = valueOutputs.Count;

            if (numOutputs > 0) flow.SetValue(valueOutputs[0], args.Value0);
            if (numOutputs > 1) flow.SetValue(valueOutputs[1], args.Value1);
            if (numOutputs > 2) flow.SetValue(valueOutputs[2], args.Value2);
            if (numOutputs > 3) flow.SetValue(valueOutputs[3], args.Value3);
        }

        private OnUnityEventData GetData(GraphPointer stack)
        {
            return stack.GetElementData<OnUnityEventData>(this);
        }
        private static Type AotSupportMethodsType;
        private static bool aotSupportMethodsTypeChecked = false;
        const string AotSupportTypeFullName = "Unity.VisualScripting.Community.Generated.AotSupportMethods";
        private Type GetAotSupportMethodsType()
        {
            if (AotSupportMethodsType != null || aotSupportMethodsTypeChecked)
                return AotSupportMethodsType;

            aotSupportMethodsTypeChecked = true;
            AotSupportMethodsType = Type.GetType("Unity.VisualScripting.Community.Generated.AotSupportMethods, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (AotSupportMethodsType == null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AotSupportMethodsType = asm.GetType(AotSupportTypeFullName);
                    if (AotSupportMethodsType != null)
                        break;
                }
            }
            return AotSupportMethodsType;
        }
    }
}