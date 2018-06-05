using System;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;

namespace Lasm.BoltAddons.UnitTools.FlowControl
{
    [TypeIcon(typeof(While))]
    [UnitSurtitle("Value")]
    [UnitTitle("Cycle Value")]
    [UnitShortTitle("Cycle")]
    [UnitCategory("Control")]
    public class CycleValueUnit : Unit
    {
        private int count = -1;

        [SerializeAs("valueCount")]
        private int _valueCount;

        [Inspectable]
        [UnitHeaderInspectable("Steps"), InspectorLabel("Steps")]
        public int valueCount
        {
            get { return _valueCount; }
            set { _valueCount = Mathf.Clamp(value, 1, 10); }
        }

        [SerializeAs("type")]
        private Type _type = typeof(int);

        [Inspectable]
        [UnitHeaderInspectable("Type"), InspectorLabel("Type")]
        public Type type
        {
            get { return _type; }
            set { _type = value; }
        }

        [Inspectable]
        [InspectorToggleLeft]
        [UnitHeaderInspectable("Use Tags"), InspectorLabel("Use Tags ")]
        public bool useTags;


        [DoNotSerialize]
        public ValueInput ascend;

        [DoNotSerialize]
        public ValueOutput indexOut;
        [DoNotSerialize]
        public ValueOutput tagOut;
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlInput reset;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ReadOnlyCollection<ValueOutput> currentValueOutput;
        [DoNotSerialize]
        public ReadOnlyCollection<ValueInput> multiValueInputs;
        [DoNotSerialize]
        public bool lastAscended = false;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Trigger));
            reset = ControlInput("reset", new Action<Flow>(Reset));

            exit = ControlOutput("exit");

            ascend = ValueInput<bool>("ascend", false);

            List<ValueInput> _multiValueInputs = new List<ValueInput>();
            this.multiValueInputs = _multiValueInputs.AsReadOnly();

            for (int i = 0; i < _valueCount; i++)
            {   
                if (useTags)
                {
                    ValueInput tagIndex = ValueInput<string>("tag_" + i.ToString(), String.Empty);
                   // Relation(tagIndex, tagOut);
                } else
                {
                    //Relation(_multiValueInputs[i], indexOut);
                }

                _multiValueInputs.Add(ValueInput(type, "value_" + i.ToString()));
                if (_multiValueInputs[i].type != typeof(int) && _multiValueInputs[i].type != typeof(float) && _multiValueInputs[i].type != typeof(string) 
                    && _multiValueInputs[i].type != typeof(bool)) {
                    
                    _multiValueInputs[i].SetDefaultValue(_multiValueInputs[i].type.IsValueType ? Activator.CreateInstance(type) : null);
                }
                else
                {
                    if (_multiValueInputs[i].type == typeof(int))
                    {
                        _multiValueInputs[i].SetDefaultValue(0);
                    }
                    if (_multiValueInputs[i].type == typeof(float))
                    {
                        _multiValueInputs[i].SetDefaultValue(0f);
                    }
                    if (_multiValueInputs[i].type == typeof(bool))
                    {
                        _multiValueInputs[i].SetDefaultValue(false);
                    }
                    if (_multiValueInputs[i].type == typeof(string))
                    {
                        _multiValueInputs[i].SetDefaultValue(String.Empty);
                    }
                }
            }

            Func<Recursion, object> getValue = getCurrentValue => CurrentValue();
            ValueOutput valueOut = ValueOutput("currentValueOut", getValue);

            if (useTags)
            {
                Func<Recursion, string> getTag = getCurrentTag => CurrentTag();
                tagOut = ValueOutput("tag", getTag);
                
                //Relation(ascend, tagOut);
            }
            else
            {
                Func<Recursion, int> getIndex = getCurrentIndex => CurrentIndex();
                indexOut = ValueOutput("index", getIndex);

               // Relation(ascend, indexOut);

            }

            Relation(enter, exit);
            
        }

        public string CurrentTag()
        {
            return valueInputs["tag_" + count].GetValue<string>();
        }

        public int CurrentIndex()
        {
            return count;
        }

        public object CurrentValue()
        {
            return valueInputs["value_" + count.ToString()].GetValue<object>();
        }

        public void Trigger(Flow flow)
        {
            Flow _flow = Flow.New();

            if (!ascend.GetValue<bool>())
            {
                if (count < multiValueInputs.Count - 1)
                {
                    count++;
                }
                else
                {
                    count = 0;
                }
            }
            else
            {

                if (count > 0)
                {
                    count--;

                }
                else
                {
                    count = multiValueInputs.Count - 1;
                }
            }

            _flow.Invoke(exit);
            _flow.Dispose();
        }

        public void Reset(Flow flow)
        {
            count = -1;
        }

    }


}





