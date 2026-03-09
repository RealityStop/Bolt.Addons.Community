using System.Collections;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Random Element")]
    [UnitCategory("Collections")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Collections.RandomElementUnit")]
    [TypeIcon(typeof(IEnumerable))]
    public class RandomElementNode : Unit
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
        /// The ValueInput for the collection/list we will be querying.
        /// </summary>
        [DoNotSerialize]
        public ValueInput collection;

        /// <summary>
        /// The ValueOutput for the randomly selected key.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput key;

        /// <summary>
        /// The ValueOutput for the randomly selected value.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput value;

        [Inspectable]
        public bool Dictionary;

        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                PerformOperation(flow);
                return exit;
            });


            collection = ValueInput<IEnumerable>("collection");
            
            exit = ControlOutput("exit");

            if (Dictionary)
            {
                key = ValueOutput<object>("key");
                Assignment(enter, key);
            }

            value = ValueOutput<object>("item");

            Succession(enter, exit);
            Assignment(enter, value);
            Requirement(collection, enter);

        }

        private void PerformOperation(Flow flow)
        {
            var collectionValue = flow.GetValue<IEnumerable>(collection);

            IList list = collectionValue as IList;
            if (list != null)
            {
                if (Dictionary)
                {
                    Debug.LogWarning("Unit is configured as Dictionary, but recieved list.  Key will be unset.");
                    flow.SetValue(key, null);
                }

                if (list.Count == 0)
                {
                    Debug.LogWarning("Collection is empty, null returned.");
                    flow.SetValue(value, null);
                    return;
                }

                //Lists have fast random access, so... use it.
                //Prevents overhead associated with .ElementAt
                flow.SetValue(value, list[UnityEngine.Random.Range(0, list.Count)]);
            }
            else
            {
                IDictionary dictionary = collectionValue as IDictionary;
                if (dictionary != null)
                {
                    if (!Dictionary)
                    {
                        Debug.LogWarning("Unit is configured as simple collection, but recieved Dictionary.  Key will be innaccessible.");
                    }

                    if (dictionary.Count == 0)
                    {
                        Debug.LogWarning("Collection is empty, null returned.");

                        if (Dictionary)
                            flow.SetValue(key, null);
                        flow.SetValue(value, null);
                        return;
                    }

                    //If is slightly faster to select a random key than it is to do a straight .ElementAt
                    object randomKey = dictionary.Keys.Cast<object>().ElementAt(UnityEngine.Random.Range(0, dictionary.Count));

                    if (Dictionary)
                        flow.SetValue(key, randomKey);
                    flow.SetValue(value, dictionary[randomKey]);
                }
                else
                {
                    if (Dictionary)
                    {
                        Debug.LogWarning("Unit is configured as Dictionary, but recieved non-dictionary collection.  Key will be unset.");
                        flow.SetValue(key, null);
                    }
                    //Default implementation.  Might be slow, we don't know!
                    flow.SetValue(value, collectionValue.Cast<object>().ElementAt(UnityEngine.Random.Range(0, list.Count)));
                }
            }
        }
    }
}
