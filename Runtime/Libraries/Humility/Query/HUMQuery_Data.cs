using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMQuery
    {
        /// <summary>
        /// Structs for passing data down a querying operation.
        /// </summary>
        public static partial class Data
        {
            /// <summary>
            /// Query operation data for starting some kind of enumerable loop action.
            /// </summary>
            public struct For<T>
            {
                public IList<T> collection;

                public For(IList<T> collection)
                {
                    this.collection = collection;
                }
            }
        }
    }
}