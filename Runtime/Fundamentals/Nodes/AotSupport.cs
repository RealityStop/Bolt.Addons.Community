using UnityEngine.Events;

namespace Unity.VisualScripting.Community
{

    public class AotUnityEvent<T1> : UnityEvent<T1> {
        internal void Use() {
            new OnUnityEvent().OneParamHandler<T1>(null);
        }
    }
    
    public class AotUnityEvent<T1, T2> : UnityEvent<T1, T2> {
        internal void Use() {
            new OnUnityEvent().TwoParamsHandler<T1, T2>(null);
        }
    }
    
    public class AotUnityEvent<T1, T2, T3> : UnityEvent<T1, T2, T3> {
        internal void Use() {
            new OnUnityEvent().ThreeParamsHandler<T1, T2, T3>(null);
        }
    }
    
    public class AotUnityEvent<T1, T2, T3, T4> : UnityEvent<T1, T2, T3, T4> {
        internal void Use() {
            new OnUnityEvent().FourParamsHandler<T1, T2, T3, T4>(null);
        }
    }

}