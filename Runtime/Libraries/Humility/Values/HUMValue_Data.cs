using System;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMValue
    {
        /// <summary>
        /// Structs for passing data down a value operation.
        /// </summary>
        public static class Data
        {
            /// <summary>
            /// Value operation data for creating a type.
            /// </summary>
            public struct Create
            {
            }

            /// <summary>
            /// Value operation data for ensuring a type exists on a GameObject.
            /// </summary>
            public struct Ensure
            {
                public GameObject target;

                public Ensure(GameObject target)
                {
                    this.target = target;
                }
            }

            /// <summary>
            /// Value operation data for ensuring a MonoBehaviour of some kind exists on a GameObject.
            /// </summary>
            public struct MBehaviour
            {
                public Ensure ensure;

                public MBehaviour(Ensure ensure)
                {
                    this.ensure = ensure;
                }
            }

            /// <summary>
            /// Value operation data for ensuring a MonoBehaviour of an interface of some kind exists on a GameObject.
            /// </summary>
            public struct Interface
            {
                public MBehaviour ensure;

                public Interface(MBehaviour ensure)
                {
                    this.ensure = ensure;
                }
            }

            /// <summary>
            /// Value operation data for conversion operations.
            /// </summary>
            public struct As
            {
                public object value;

                public As(object value)
                {
                    this.value = value;
                }
            }

            /// <summary>
            /// Value operation data for type checking operations.
            /// </summary>
            public struct Is
            {
                public object value;

                public Is(object value)
                {
                    this.value = value;
                }
            }

            /// <summary>
            /// Value operation data for Or
            /// </summary>
            public struct Or
            {
                public bool wasNull;
                public object value;

                public Or(object value, bool wasNull)
                {
                    this.wasNull = wasNull;
                    this.value = value;
                }
            }

            /// <summary>
            /// Value operation for IsNull.
            /// </summary>
            public struct IsNull
            {
                public Is @is;
                public Or or;
                public bool isNull;

                public IsNull(Is @is, bool isNull)
                {
                    this.@is = @is;
                    this.or = new Or();
                    this.isNull = isNull;
                }

                public IsNull(Or or, bool isNull)
                {
                    this.@is = new Is();
                    this.or = or;
                    this.isNull = isNull;
                }
            }

            /// <summary>
            /// Value operation data for NotNull.
            /// </summary>
            public struct NotNull
            {
                public Is @is;
                public Or or;
                public bool isNull;

                public NotNull(Is @is, bool isNull)
                {
                    this.@is = @is;
                    this.or = new Or();
                    this.isNull = isNull;
                }

                public NotNull(Or or, bool isNull)
                {
                    this.@is = new Is();
                    this.or = or;
                    this.isNull = isNull;
                }
            }
        }
    }
}