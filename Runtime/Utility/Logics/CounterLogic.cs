using System;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class CounterLogic
    {
        private int _count;

        public void Update()
        {
            _count++;
        }

        public void Reset()
        {
            _count = 0;
        }

        public int Count => _count;
    }
}