using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Create a new multi dimensional array.
    /// </summary>
    [TypeIcon(typeof(object[]))]
    [UnitCategory("Community/Collections/Arrays")]
    [RenamedFrom("Lasm.BoltExtensions.CreateArray")]
    [RenamedFrom("Lasm.BoltExtensions.CreateMultiArray")]
    [RenamedFrom("Lasm.UAlive.CreateMultiArray")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Collections.CreateMultiArray")]
    public sealed class CreateArray : Unit
    {
        [Serialize]
        private int _dimensions = 2;
        /// <summary>
        /// The amount of dimensions we want this array to have. Max of 32 dimensions.
        /// </summary>
        [Inspectable][UnitHeaderInspectable("Dimensions")]
        public int dimensions
        {
            get { return _dimensions; } set { _dimensions = Mathf.Clamp(value, 1, 32); }
        }

        /// <summary>
        /// The type of this array.
        /// </summary>
        [Serialize]
        [Inspectable][UnitHeaderInspectable("Type")]
        public System.Type type;

        /// <summary>
        /// The Value Inputs of each dimensions length.
        /// </summary>
        [DoNotSerialize]
        public List<ValueInput> indexes = new List<ValueInput>();

        /// <summary>
        /// The Value Ouput that returns the newly created array.
        /// </summary>
        [DoNotSerialize]
        [RenamedFrom("Lasm.BoltExtensions.CreateMultiArray.list")]
        public ValueOutput result;

        protected override void Definition()
        {
            indexes.Clear();

            for (int i = 0; i < dimensions; i++)
            {
                var dimension = ValueInput<int>(i.ToString() + " Length", 0);
                indexes.Add(dimension);
            }

            result = ValueOutput<Array>("array", Create);
        }

        private Array Create(Flow flow)
        {
            var lengths = new List<int>();

            for (int i = 0; i < indexes.Count; i++)
            {
                lengths.Add(flow.GetValue<int>(indexes[i]));
            }

            return MakeArrayType(dimensions, lengths);
        }

        /// <summary>
        /// Creates an array manually to get over AOT problems.
        /// </summary>
        public Array MakeArrayType(int dimensions, List<int> lengths)
        {
            switch (dimensions)
            {
                case 1:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0] });
                    }

                case 2:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1] });
                    }

                case 3:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2] });
                    }

                case 4:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3] });
                    }

                case 5:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4] });
                    }

                case 6:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5] });
                    }

                case 7:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6] });
                    }

                case 8:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7] });
                    }

                case 9:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8] });
                    }

                case 10:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9] });
                    }

                case 11:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10] });
                    }

                case 12:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11] });
                    }

                case 13:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12] });
                    }

                case 14:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13] });
                    }

                case 15:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14] });
                    }

                case 16:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15] });
                    }

                case 17:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16] });
                    }

                case 18:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17] });
                    }

                case 19:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18] });
                    }

                case 20:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19] });
                    }

                case 21:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20] });
                    }

                case 22:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21] });
                    }

                case 23:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22] });
                    }

                case 24:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23] });
                    }

                case 25:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24] });
                    }

                case 26:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25] });
                    }

                case 27:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26] });
                    }

                case 28:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27] });
                    }

                case 29:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28] });
                    }

                case 30:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28], lengths[29] });
                    }

                case 31:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28], lengths[29], lengths[30] });
                    }

                case 32:
                    {
                        return Array.CreateInstance(type, new int[] { lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15], lengths[16], lengths[17], lengths[18], lengths[19], lengths[20], lengths[21], lengths[22], lengths[23], lengths[24], lengths[25], lengths[26], lengths[27], lengths[28], lengths[29], lengths[30], lengths[31]});
                    }
            }

            return null;
        }
    }
}