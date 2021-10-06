using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMType
    {
        /// <summary>
        /// Structs for passing data down a type operation.
        /// </summary>
        public static partial class Data
        {
            /// <summary>
            /// Type operation data for Is.
            /// </summary>
            public struct Is
            {
                public System.Type type;

                public Is(System.Type type)
                {
                    this.type = type;
                }
            }

            /// <summary>
            /// Type operation data for As
            /// </summary>
            public struct As
            {
                public System.Type type;

                public As(System.Type type)
                {
                    this.type = type;
                }
            }

            /// <summary>
            /// Type operation data for Has
            /// </summary>
            public struct Has
            {
                public System.Type type;

                public Has(System.Type type)
                {
                    this.type = type;
                }
            }

            /// <summary>
            /// Type operation data for With
            /// </summary>
            public struct With
            {
                public Types types;

                public With(Types types)
                {
                    this.types = types;
                }
            }

            /// <summary>
            /// Type operation data for Generic
            /// </summary>
            public struct Generic
            {
                public Is isData;

                public Generic(Is isData)
                {
                    this.isData = isData;
                }
            }

            /// <summary>
            /// Type operation data for Get
            /// </summary>
            public struct Get
            {
                public System.Type type;

                public Get(System.Type type)
                {
                    this.type = type;
                }
            }

            /// <summary>
            /// Type operation data for All
            /// </summary>
            public struct All
            {
                public Get get;

                public All(Get get)
                {
                    this.get = get;
                }
            }

            /// <summary>
            /// Type operation data for Types
            /// </summary>
            public struct Types
            {
                public Get get;

                public Types(Get get)
                {
                    this.get = get;
                }
            }

            /// <summary>
            /// Type operation data for Derived
            /// </summary>
            public struct Derived
            {
                public Get get;
                public All all;

                public Derived(Get get, List<Type> types)
                {
                    this.get = get;
                    all = new All();
                }

                public Derived(All all, List<Type> types)
                {
                    this.all = all;
                    get = new Get();
                }
            }

            /// <summary>
            /// Type operation data for Of
            /// </summary>
            public struct Of
            {
                public All all;

                public Of(All all)
                {
                    this.all = all;
                }
            }
        }
    }
}