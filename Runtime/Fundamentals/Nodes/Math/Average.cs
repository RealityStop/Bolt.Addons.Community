using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
[RenamedFrom("Vector4ListAverage")]    
    [UnitCategory("Community/Math")]
    [UnitTitle("List Average (Scalar)")]
    [TypeIcon(typeof(ScalarAverage))]
    public class ScalarListAverage : Unit
    {
        [DoNotSerialize]
        public ValueInput numbers;
        [DoNotSerialize]
        public ValueOutput average;
    
    
        protected override void Definition()
        {
            numbers = ValueInput<List<float>>(nameof(numbers));
            average = ValueOutput(nameof(average), Result);
        }
    
        private float Result(Flow flow) 
        {
            var _numbers = flow.GetValue<List<float>>(numbers);
    
            if (_numbers == null || _numbers.Count == 0)
            {
                return 0;
            }
    
            float sum = 0f;
    
            foreach (float num in _numbers)
            {
                sum += num;
            }
    
            return sum / _numbers.Count;
        }
    }
    
    [UnitCategory("Community/Math")]
    [UnitTitle("List Average (Vector2)")]
    [TypeIcon(typeof(ScalarAverage))]
    public class Vector2ListAverage : Unit
    {
        [DoNotSerialize]
        public ValueInput numbers;
        [DoNotSerialize]
        public ValueOutput average;
    
    
        protected override void Definition()
        {
            numbers = ValueInput<List<Vector2>>(nameof(numbers));
            average = ValueOutput(nameof(average), Result);
        }
    
        private Vector2 Result(Flow flow)
        {
            var _numbers = flow.GetValue<List<Vector2>>(numbers);
    
            if (_numbers == null || _numbers.Count == 0)
            {
                return new();
            }
    
            Vector2 sum = new();
    
            foreach (Vector2 num in _numbers)
            {
                sum += num;
            }
    
            return sum / _numbers.Count;
        }
    }
    
    [UnitCategory("Community/Math")]
    [UnitTitle("List Average (Vector3)")]
    [TypeIcon(typeof(Vector3Average))]
    public class Vector3ListAverage : Unit
    {
        [DoNotSerialize]
        public ValueInput numbers;
        [DoNotSerialize]
        public ValueOutput average;
    
    
        protected override void Definition()
        {
            numbers = ValueInput<List<Vector3>>(nameof(numbers));
            average = ValueOutput(nameof(average), Result);
        }
    
        private Vector3 Result(Flow flow)
        {
            var _numbers = flow.GetValue<List<Vector3>>(numbers);
    
            if (_numbers == null || _numbers.Count == 0)
            {
                return new Vector3();
            }
    
            Vector3 sum = new Vector3();
    
            foreach (Vector3 num in _numbers)
            {
                sum += num;
            }
    
            return sum / _numbers.Count;
        }
    }
    
    [UnitCategory("Community/Math")]
    [UnitTitle("List Average (Vector4)")]
    [TypeIcon(typeof(Vector4Average))]
    public class Vector4ListAverage : Unit
    {
        [DoNotSerialize]
        public ValueInput numbers;
        [DoNotSerialize]
        public ValueOutput average;
    
    
        protected override void Definition()
        {
            numbers = ValueInput<List<Vector4>>(nameof(numbers));
            average = ValueOutput(nameof(average), Result);
        }
    
        private Vector4 Result(Flow flow)
        {
            var _numbers = flow.GetValue<List<Vector4>>(numbers);
    
            if (_numbers == null || _numbers.Count == 0)
            {
                return new Vector4();
            }
    
            Vector4 sum = new Vector4();
    
            foreach (Vector4 num in _numbers)
            {
                sum += num;
            }
    
            return sum / _numbers.Count;
        }
    }
    
    
}