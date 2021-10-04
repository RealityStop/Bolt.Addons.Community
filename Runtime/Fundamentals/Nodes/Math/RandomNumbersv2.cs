using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Returns a list of random numbers from the specified range.
    /// </summary>
    [UnitCategory("Community\\Collections\\Lists")]
    [UnitTitle("Random Numbers")]
    [TypeIcon(typeof(IList))]
    [UnitOrder(20)]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.RandomNumbersv2")]
    public class RandomNumbersv2 : Unit
    {
        public RandomNumbersv2() : base() { }

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

        /// <summary>
        /// Whether the compared inputs are numbers.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorToggleLeft]
        public bool aotList { get; set; } = false;


        /// <summary>
        /// Whether the compared inputs are numbers.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorToggleLeft]
        public bool unique { get; set; } = true;


      //  IList _list = new List<object>();
         IList _list;

        protected override void Definition()
        {
            input = ControlInput(nameof(input), x => Enter(x));
            count = ValueInput<int>(nameof(count), 10);
            exit = ControlOutput(nameof(exit));

            if (integer)
            {
                minimum = ValueInput<int>(nameof(minimum), 0);
                maximum = ValueInput<int>(nameof(maximum), 100);
                output = ValueOutput<IList>(nameof(output), (x) => { if (_list == null) BuildList(x); return _list; });
            }
            else
            {
                minimum = ValueInput<float>(nameof(minimum), 0);
                maximum = ValueInput<float>(nameof(maximum), 100);
                output = ValueOutput<IList>(nameof(output), (x) => { if (_list == null) BuildList(x); return _list; });
            }

            Succession(input, exit);

            Requirement(count, input);
            Requirement(minimum, input);

            Requirement(count, output);
            Requirement(minimum, output);

            Assignment(input, output);
        }

        private void BuildList(Flow flow)
        {
            int num = flow.GetValue<int>(count);

            ManageList(num);

            if (integer)
            {
                int min = flow.GetValue<int>(minimum);
                int max = flow.GetValue<int>(maximum);

                //If we need unique integers, we have to ensure there are enough choices
                if (unique)
                    if (max - min < num)
                    {
                        throw new ArgumentException("Random Number generation failed.  Unique range is too small for the number of elements specified.");
                    }

                PopulateList(unique, num, _list, () => { return UnityEngine.Random.Range(min, max); });
            }
            else
            {

                float min = flow.GetValue<float>(minimum);
                float max = flow.GetValue<float>(maximum);
                PopulateList(unique, num, _list, () => { return UnityEngine.Random.Range(min, max); });
            }

        }

        private void ManageList(int num)
        {
            if (_list == null)
            {
                if (aotList)
                {
                    _list = new AotList(num);
                }
                else
                {
                    if (integer)
                        _list = new List<int>(num);
                    else
                        _list = new List<float>(num);
                }
            }

            _list.Clear();
        }

        private void PopulateList<T>(bool unique, int count, IList list, Func<T> p)
        {
            HashSet<T> uniqueNumbers = new HashSet<T>();

            while (list.Count < count)
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

        private ControlOutput Enter(Flow flow)
        {
            BuildList(flow);
            return exit;
        }
    }
}