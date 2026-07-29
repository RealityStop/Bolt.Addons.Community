using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    internal class DraggedListItem : VisualScripting.DraggedListItem
    {
        public DraggedListItem(MetadataListAdaptor sourceListAdaptor, int index, object item, bool foldoutState) : base(sourceListAdaptor, index, item)
        {
            this.foldoutState = foldoutState;
        }

        public readonly bool foldoutState;
    }

    public class DraggedDictionaryItem
    {
        public DraggedDictionaryItem(DictionaryAdaptor sourceDictionaryAdaptor, int index, object item, bool foldoutState)
        {
            this.sourceDictionaryAdaptor = sourceDictionaryAdaptor;
            this.foldoutState = foldoutState;
            this.item = item;
            this.index = index;
        }

        public readonly DictionaryAdaptor sourceDictionaryAdaptor;
        public readonly object item;
        public readonly int index;

        public IDictionary sourceList => (IDictionary)sourceDictionaryAdaptor.Metadata.value;

        public static readonly string TypeName = typeof(DraggedListItem).FullName;
        public readonly bool foldoutState;
    }
}
