using Ludiq;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Bolt.Addons.Community.Fundamentals.Units.Collections
{
    [UnitTitle("Get Random Element")]
    [UnitCategory("Collections")]
    [TypeIcon(typeof(IEnumerable))]
    public class GetRandomElementUnit : Unit
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
        /// The ValueOutput for the randomly selected value.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput item;


        private static UnityEngine.Random rand = new UnityEngine.Random();


        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                var current = PerformOperation(flow);
                flow.SetValue(item, current);
                return exit;
            });


            collection = ValueInput<IEnumerable>("collection");
            item = ValueOutput<object>("item");
            exit = ControlOutput("exit");

            Succession(enter, exit); 
            Assignment(enter, item);
            Requirement(collection, enter);

        }

        private object PerformOperation(Flow flow)
        {
            var collectionValue = flow.GetValue<IEnumerable>(collection);

            IList list = collectionValue as IList;
            if (list != null)
            {
                if (list.Count == 0)
                    return null;

                //Lists have fast random access, so... use it.
                //Prevents overhead associated with .ElementAt
                return list[UnityEngine.Random.Range(0, list.Count)];
            }
            else
            {
                IDictionary dictionary = collectionValue as IDictionary;
                if (dictionary != null)
                {
                    if (dictionary.Count == 0)
                        return null;

                    //If is slightly faster to select a random key than it is to do a straight .ElementAt
                    return dictionary[dictionary.Keys.Cast<object>().ElementAt(UnityEngine.Random.Range(0, dictionary.Count))];
                }
                else
                {
                    //Default implementation.  Might be slow, we don't know!
                    return collectionValue.Cast<object>().ElementAt(UnityEngine.Random.Range(0, list.Count));
                }
            }
        }
    }
}