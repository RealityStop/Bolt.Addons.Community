using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class OnMultiKeyPressLogic
    {
        private bool key1Down;
        private bool key2Down;
        private bool key1Up;
        private bool key2Up;

        private float key1Time;
        private float key2Time;

        public bool Check(float delay, KeyCode key1, KeyCode key2, PressState state)
        {
            if (Input.GetKeyDown(key1))
            {
                key1Down = true;
                key1Time = Time.time;
            }
            if (Input.GetKeyDown(key2))
            {
                key2Down = true;
                key2Time = Time.time;
            }

            if (Input.GetKeyUp(key1))
            {
                key1Up = true;
                key1Time = Time.time;
            }
            if (Input.GetKeyUp(key2))
            {
                key2Up = true;
                key2Time = Time.time;
            }

            if (state == PressState.Down)
            {
                if (key1Down && key2Down && Mathf.Abs(key1Time - key2Time) <= delay)
                {
                    key1Down = false;
                    key2Down = false;
                    return true;
                }
            }

            if (state == PressState.Hold)
            {
                return Input.GetKey(key1) && Input.GetKey(key2);
            }

            if (state == PressState.Up)
            {
                if (key1Up && key2Up && Mathf.Abs(key1Time - key2Time) <= delay)
                {
                    key1Up = false;
                    key2Up = false;
                    return true;
                }
            }

            return false;
        }
    }
}