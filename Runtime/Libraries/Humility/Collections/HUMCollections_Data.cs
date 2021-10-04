using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMCollections
    {
        public static partial class Data
        {
            public struct Move
            {
                public IList collection;

                public Move(IList collection, int itemIndex)
                {
                    this.collection = collection;
                }

                public Move(IList collection)
                {
                    this.collection = collection;
                }
            }

            public struct MoveItem
            {
                public Move move;
                public object item;

                public MoveItem(Move move, object item)
                {
                    this.move = move;
                    this.item = item;
                }
            }

            public struct MoveIndex
            {
                public Move move;
                public int index;

                public MoveIndex(Move move, int index)
                {
                    this.move = move;
                    this.index = index;
                }
            }
        }
    }
}
