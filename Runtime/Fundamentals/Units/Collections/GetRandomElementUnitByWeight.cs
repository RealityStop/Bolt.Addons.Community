using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Bolt.Addons.Community.Fundamentals.Units.Collections
{
    [UnitTitle("Random Element By Weight")]
    [UnitCategory("Collections")]
    [TypeIcon(typeof(IDictionary))]
    public class RandomElementUnitByWeight : Unit
    {
        /// <summary>
        /// The Control Input entered when we want a random element
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        /// <summary>
        /// The Control Output for when the query is complete.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput exit;


        /// <summary>
        /// The ValueInput for the collection we will be querying.
        /// </summary>
        [DoNotSerialize]
        public ValueInput collection;

        /// <summary>
        /// The ValueInput for the total number of item will be querying.
        /// </summary>
        [Serialize]
        [Inspectable, UnitHeaderInspectable("Item count")]
        public int count { get; set; }

        /// <summary>
        /// The ValueOutput for the randomly selected value.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput output;


        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                PerformOperation(flow);
                return exit;
            });
            collection = ValueInput<IDictionary>("collection");

            exit = ControlOutput("exit");
            if (count <= 1)
            {
                count = 1;
                output = ValueOutput<Object>("item");
            } else
            {
                output = ValueOutput<List<Object>>("item");
            }

            Succession(enter, exit);
            Assignment(enter, output);
            Requirement(collection, enter);
        }

        private void PerformOperation(Flow flow)
        {
            IDictionary dictionary = flow.GetValue<IDictionary>(collection);
            WeightedRandomBag<Object> itemsAccumulatedWeight = new WeightedRandomBag<Object>();
            List<Object> returnItems = new List<Object>();

            if (dictionary.Count == 0)
            {
                Debug.LogWarning("Collection is empty, empty list returned.");
                flow.SetValue(output, returnItems);
                return;
            }

            if (count == 0)
            {
                Debug.LogWarning("The count of get random element by weight is zero, empty list returned.");
                flow.SetValue(output, returnItems);
                return;
            }

            // Push items into accumulated weight bag
            foreach (DictionaryEntry item in dictionary)
            {
                itemsAccumulatedWeight.AddEntry(item.Key, (float)item.Value);
            }
            if (count <= 1) // Get a random item
            {
                flow.SetValue(output, itemsAccumulatedWeight.GetRandom());
                return;
            }
            
            for(var i=0; i< count; i++) // Get random 'count' items
            {
                returnItems.Add(itemsAccumulatedWeight.GetRandom());
            }

            flow.SetValue(output, returnItems);
        }


        class WeightedRandomBag<T>
        {
            private struct Entry
            {
                public float accumulatedWeight;
                public T item;
            }

            private List<Entry> entries = new List<Entry>();
            private float accumulatedWeight;

            public void AddEntry(T item, float weight)
            {
                accumulatedWeight += weight;
                entries.Add(new Entry { item = item, accumulatedWeight = accumulatedWeight });
            }

            public T GetRandom()
            {
                float r = Random.Range(0, accumulatedWeight);
                foreach (Entry entry in entries)
                {
                    if (entry.accumulatedWeight >= r)
                    {
                        return entry.item;
                    }
                }
                return default(T);
            }
        }
    }
}
