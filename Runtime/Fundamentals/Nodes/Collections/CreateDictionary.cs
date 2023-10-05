using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Collections/Dictionaries")]
    [TypeIcon(typeof(Dictionary<,>))]
    public class CreateDictionary : Unit
    {
        [UnitHeaderInspectable("KeyType")]
        public Type KeyType = typeof(object);

        [UnitHeaderInspectable("ValueType")]
        public Type ValueType = typeof(object);

        [DoNotSerialize]
        public ValueOutput Dictionary;

        protected override void Definition()
        {
            Dictionary = ValueOutput(typeof(Dictionary<,>).MakeGenericType(KeyType, ValueType), nameof(Dictionary), CreateNewDictionary);
        }

        private object CreateNewDictionary(Flow flow)
        {
            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(KeyType, ValueType);
            return Activator.CreateInstance(dictionaryType);
        }
    }
}