using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Gets an item from a multidimensional array.
    /// </summary>
    [UnitCategory("Community/Collections/Arrays")]
    [RenamedFrom("Lasm.BoltExtensions.GetArrayItem")]
    [RenamedFrom("Lasm.UAlive.GetArrayItem")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Collections.GetArrayItem")]
    public sealed class GetArrayItem : Unit
    {
        [Serialize]
        private int _dimensions = 1;

        /// <summary>
        /// Sets the dimensions on this unit, that an array has. Max of 32 dimensions.
        /// </summary>
        [Inspectable]
        [UnitHeaderInspectable("Dimensions")]
        public int dimensions
        {
            get { return _dimensions; }
            set { _dimensions = Mathf.Clamp(value, 1, 32); }
        }

        /// <summary>
        /// The target array to get the item from.
        /// </summary>
        [DoNotSerialize]
        public ValueInput array;

        /// <summary>
        /// The Value Inputs of each dimensions length.
        /// </summary>
        [DoNotSerialize]
        public List<ValueInput> indexes = new List<ValueInput>();

        /// <summary>
        /// The value of the desired selection of dimensions.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput value;

        protected override void Definition()
        {
            indexes.Clear();

            array = ValueInput<Array>("array");

            for (int i = 0; i < dimensions; i++)
            {
                var dimension = ValueInput<int>(i.ToString() + " Index", 0);
                indexes.Add(dimension);
            }

            value = ValueOutput<object>("result",  GetItem);
        }

        private object GetItem(Flow flow)
        {
            var lengths = new List<int>();

            for (int i = 0; i < indexes.Count; i++)
            {
                lengths.Add(flow.GetValue<int>(indexes[i]));
            }

            switch (dimensions)
            {
                case 1:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0] });
                    }

                case 2:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1] });
                    }

                case 3:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2] });
                    }

                case 4:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3] });
                    }

                case 5:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4]});
                    }

                case 6:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5] });
                    }

                case 7:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6] });
                    }

                case 8:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7] });
                    }

                case 9:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8] });
                    }

                case 10:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9] });
                    }

                case 11:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10] });
                    }

                case 12:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11] });
                    }

                case 13:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12] });
                    }

                case 14:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13] });
                    }

                case 15:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14] });
                    }

                case 16:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15] });
                    }

                case 17:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16] });
                    }

                case 18:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17] });
                    }

                case 19:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18] });
                    }

                case 20:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19] });
                    }

                case 21:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20] });
                    }

                case 22:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21] });
                    }

                case 23:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22] });
                    }

                case 24:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23] });
                    }

                case 25:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24] });
                    }

                case 26:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25] });
                    }

                case 27:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26] });
                    }

                case 28:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27] });
                    }

                case 29:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28] });
                    }

                case 30:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28], lengths[29] });
                    }

                case 31:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28], lengths[29], lengths[30] });
                    }

                case 32:
                    {
                        return flow.GetValue<Array>(array).GetValue(new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28], lengths[29], lengths[30], lengths[31] });
                    }
            }

            return null;
        }
    }
}