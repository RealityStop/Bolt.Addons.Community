using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMCollections_Children
    {
        public static HUMCollections.Data.MoveItem Item(this HUMCollections.Data.Move move, object item)
        {
            return new HUMCollections.Data.MoveItem(move, item);
        }

        public static HUMCollections.Data.MoveIndex Index(this HUMCollections.Data.Move move, int index)
        {
            return new HUMCollections.Data.MoveIndex(move, index);
        }

        public static void Increment(this HUMCollections.Data.MoveItem moveItem)
        {
            var index = moveItem.move.collection.IndexOf(moveItem.item);
            var newIndex = Mathf.Clamp(index + 1, 0, moveItem.move.collection.Count - 1);
            moveItem.move.collection.Remove(moveItem.item);
            moveItem.move.collection.Insert(newIndex, moveItem);
        }

        public static void Decrement(this HUMCollections.Data.MoveItem moveItem)
        {
            var index = moveItem.move.collection.IndexOf(moveItem.item);
            var newIndex = Mathf.Clamp(index - 1, 0, moveItem.move.collection.Count - 1);
            moveItem.move.collection.Remove(moveItem.item);
            moveItem.move.collection.Insert(newIndex, moveItem);
        }

        public static void Increment(this HUMCollections.Data.MoveIndex moveIndex)
        {
            var newIndex = Mathf.Clamp(moveIndex.index + 1, 0, moveIndex.move.collection.Count - 1);
            var item = moveIndex.move.collection[moveIndex.index];
            moveIndex.move.collection.RemoveAt(moveIndex.index);
            moveIndex.move.collection.Insert(newIndex, item);
        }

        public static void Decrement(this HUMCollections.Data.MoveIndex moveIndex)
        {
            var newIndex = Mathf.Clamp(moveIndex.index - 1, 0, moveIndex.move.collection.Count - 1);
            var item = moveIndex.move.collection[moveIndex.index];
            moveIndex.move.collection.RemoveAt(moveIndex.index);
            moveIndex.move.collection.Insert(newIndex, item);
        }
    }
}
