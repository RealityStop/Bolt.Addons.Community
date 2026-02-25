using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.VariadicNode")]
    public abstract class VariadicNode<T> : Unit
    {
        [SerializeAs(nameof(argumentCount))]
        private int _argumentCount;

        [DoNotSerialize]
        public List<ValueInput> arguments { get; protected set; }

        /// <summary>
        /// Enables customization of default behavior, including allowing default types
        /// that are normally not supported.
        /// </summary>
        protected virtual bool customDefaults => false;

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Arguments")]
        public int argumentCount
        {
            get
            {
                return Mathf.Max(2, _argumentCount);
            }
            set
            {
                _argumentCount = Mathf.Clamp(value, 2, ArgumentLimit());
            }
        }

        protected abstract void BuildRelations(ValueInput arg);

        protected override void Definition()
        {
            arguments = new List<ValueInput>();

            ConstructArgs<T>();
        }

        protected void ConstructArgs<T1>()
        {
            for (var i = 0; i < Math.Min(argumentCount, ArgumentLimit()); i++)
            {
                var argument = ValueInput<T1>(GetArgumentName(i));
                if (!customDefaults)
                {
                    argument.SetDefaultValue(GetDefaultValue(typeof(T1)));
                }
                else
                {
                    var value = GetPortDefaultValue(typeof(T1), argument);
                    if (defaultValues.ContainsKey(argument.key))
                    {
                        defaultValues[argument.key] = value;
                    }
                    else
                    {
                        defaultValues.Add(argument.key, value);
                    }
                }
                arguments.Add(argument);
                BuildRelations(argument);
            }
        }

        protected object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            if (t == typeof(string))
                return "";

            return null;
        }

        /// <summary>
        /// Allows you to manually return the default value for the input.
        /// Requires <see cref="customDefaults"/> to be true.
        /// </summary>
        protected virtual object GetPortDefaultValue(Type t, ValueInput input)
        {
            return null;
        }

        protected virtual string GetArgumentName(int index)
        {
            return "Arg_" + index;
        }

        protected virtual int ArgumentLimit()
        {
            return 10;
        }
    }
}