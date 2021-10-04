using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Deprecated
{
    [UnitCategory("Community\\Collections\\Lists")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.RandomNumbers")]
    [RenamedFrom("Bolt.Addons.Community.Custom_Units.Math.RandomNumbers")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Deprecated.IsNullValue")]
    [UnitTitle("Random Numbers")]
    [TypeIcon(typeof(IList))]
    [Obsolete]
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
            input = ControlInput(nameof(input), x => Enter(x));
            count = ValueInput<int>(nameof(count), 10);
            unique = ValueInput<bool>(nameof(unique), true);
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
            Requirement(unique, input);

            Requirement(count, output);
            Requirement(minimum, output);
            Requirement(unique, output);
        }

        private void BuildList(Flow flow)
        {
            bool createUnique = flow.GetValue<bool>(unique);
            int num = flow.GetValue<int>(count);

            _list.Clear();

            //If we need unique integers, we have to ensure there are enough choices
            if (createUnique && integer)
            {

            }
            if (integer)
            {
                int min = flow.GetValue<int>(minimum);
                int max = flow.GetValue<int>(maximum);
                if (createUnique)
                    if (max - min < num)
                    {
                        throw new ArgumentException("Random Number generation failed.  Unique range is too small for the number of elements specified.");
                    }

                PopulateList(createUnique, num, _list, () => { return UnityEngine.Random.Range(min, max); });
            }
            else
            {

                float min = flow.GetValue<float>(minimum);
                float max = flow.GetValue<float>(maximum);
                PopulateList(createUnique, num, _list, () => { return UnityEngine.Random.Range(min, max); });
            }

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