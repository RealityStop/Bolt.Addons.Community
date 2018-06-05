using Ludiq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    /// <summary>
    /// Takes a given float input (0-1) and scales it across the specified range.
    /// </summary>
    [UnitCategory("Community\\Collections\\Lists")]
    [RenamedFrom("Bolt.Addons.Community.Custom_Units.Math.RandomNumbers")]
    [UnitTitle("Random Numbers")]
    [TypeIcon(typeof(IList))]
    [UnitOrder(20)]
    public class RandomNumbers : Unit
    {
        public RandomNumbers() : base() { }

        /// <summary>
        /// Flow In
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput input { get; private set; }

        [DoNotSerialize]
        public ValueInput count { get; private set; }

        [DoNotSerialize]
        public ValueInput minimum { get; private set; }

        [DoNotSerialize]
        public ValueInput maximum { get; private set; }

        [DoNotSerialize]
        public ValueInput unique { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }


        //Flow Out
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        /// <summary>
        /// Whether the compared inputs are numbers.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorToggleLeft]
        public bool integer { get; set; } = false;


        IList _list = new List<object>();

        protected override void Definition()
        {
            input = ControlInput(nameof(input),x=>Enter(x));
            count = ValueInput<int>(nameof(count), 10);
            unique = ValueInput<bool>(nameof(unique), true);
            exit = ControlOutput(nameof(exit));

            if (integer)
            {
                minimum = ValueInput<int>(nameof(minimum), 0);
                maximum = ValueInput<int>(nameof(maximum), 100);
                output = ValueOutput<IList>(nameof(output), (x) => { if (_list == null) BuildList(); return _list; });
            }
            else
            {
                minimum = ValueInput<float>(nameof(minimum), 0);
                maximum = ValueInput<float>(nameof(maximum), 100);
                output = ValueOutput<IList>(nameof(output), (x) => { if (_list == null) BuildList(); return _list; });
            }

            Relation(input, exit);
            Relation(count, output);
            Relation(minimum, output);
            Relation(unique, output);
        }

        private void BuildList()
        {
            bool createUnique = unique.GetValue<bool>();
            int num = count.GetValue<int>();

            _list.Clear();

            //If we need unique integers, we have to ensure there are enough choices
            if (createUnique && integer)
            {
               
            }
            if (integer)
            {
                int min = minimum.GetValue<int>();
                int max = maximum.GetValue<int>();
                if (createUnique)
                    if (max - min < num)
                    {
                        throw new ArgumentException("Random Number generation failed.  Unique range is too small for the number of elements specified.");
                    }

                PopulateList(createUnique, num, _list, () => { return UnityEngine.Random.Range(min, max); });
            }
            else
            {

                float min = minimum.GetValue<float>();
                float max = maximum.GetValue<float>();
                PopulateList(createUnique, num, _list, () => { return UnityEngine.Random.Range(min, max); });
            }

        }

        private void PopulateList<T>(bool unique, int count, IList list, Func<T> p)
        {
            HashSet<T> uniqueNumbers = new HashSet<T>();

            while(list.Count < count)
            {
                T newNumber = p();
                if (unique)
                {
                    if (!uniqueNumbers.Contains(newNumber))
                    {
                        uniqueNumbers.Add(newNumber);
                        list.Add(newNumber);
                    }
                }
                else
                    list.Add(newNumber);
            }
        }

        private void Enter(Flow flow)
        {
            BuildList();
            flow.Invoke(exit);
        }
    }
}