using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Convert")]
    [UnitCategory("Community/Utility")]
    [TypeIcon(typeof(object))]
    [RenamedFrom("Lasm.BoltExtensions.ConvertUnit")]
    [RenamedFrom("Lasm.UAlive.ConvertUnit")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Utility.ConvertUnit")]
    public sealed class ConvertNode : Unit
    {
        [Inspectable]
        public ConversionType conversion;

        [Inspectable]
        public Type type = typeof(object);

        [PortLabelHidden]
        [DoNotSerialize]
        public ValueInput value;

        [PortLabelHidden]
        [DoNotSerialize]
        public ValueOutput result;

        protected override void Definition()
        {
            value = ValueInput<object>("value");
            result = ValueOutput(conversion == ConversionType.Any ? type : (conversion == ConversionType.ToArrayOfObject ? typeof(object[]) : typeof(List<object>)), "result", (flow) => 
            {
                switch (conversion)
                {
                    case ConversionType.Any:
                        return flow.GetValue<object>(value).ConvertTo(type);
                    case ConversionType.ToArrayOfObject:
                        return flow.GetValue<IEnumerable>(value).Cast<object>().ToArray<object>();
                    case ConversionType.ToListOfObject:
                        return flow.GetValue<IEnumerable>(value).Cast<object>().ToList<object>();
                }

                return null;
            });
        }
    }
}