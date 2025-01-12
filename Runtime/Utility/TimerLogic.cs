using System;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class TimerLogic
    {
        public float duration = 1f; // Duration of the timer in seconds
        public bool unscaled = false; // Use unscaled time (ignores game speed changes)

        private float elapsedTime = 0f;
        private bool isRunning = false;

        private Action onCompleted;
        private Action onTick;

        // Events
        public event Action OnCompleted
        {
            add
            {
                if (onCompleted == null || !onCompleted.GetInvocationList().Contains(value))
                {
                    onCompleted += value;
                }
            }
            remove
            {
                if (onCompleted != null && onCompleted.GetInvocationList().Contains(value))
                {
                    onCompleted -= value;
                }
            }
        }

        public event Action OnTick
        {
            add
            {
                if (onTick == null || !onTick.GetInvocationList().Contains(value))
                {
                    onTick += value;
                }
            }
            remove
            {
                if (onTick != null && onTick.GetInvocationList().Contains(value))
                {
                    onTick -= value;
                }
            }
        }

        public void Update()
        {
            if (isRunning)
            {
                // Update elapsed time
                elapsedTime += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

                // Call tick event
                onTick?.Invoke();

                // Check if timer is completed
                if (elapsedTime >= duration)
                {
                    onCompleted?.Invoke();

                    isRunning = false;
                }
            }
        }

        // Start the timer
        public void StartTimer(float duration, bool unscaled)
        {
            elapsedTime = 0f;
            isRunning = true;
            this.duration = duration;
            this.unscaled = unscaled;
        }

        // Start the timer with Action
        public void StartTimer(float duration, bool unscaled, Action OnStarted)
        {
            elapsedTime = 0f;
            isRunning = true;
            this.duration = duration;
            this.unscaled = unscaled;
            OnStarted?.Invoke();
        }

        // Pause the timer
        public void PauseTimer()
        {
            isRunning = false;
        }

        // Resume the timer
        public void ResumeTimer()
        {
            isRunning = true;
        }

        // Toggle the timer on/off
        public void ToggleTimer()
        {
            isRunning = !isRunning;
        }

        public float Elapsed => Mathf.Min(elapsedTime, duration);
        public float ElapsedPercentage => Mathf.Clamp01(elapsedTime / duration);
        public float Remaining => Mathf.Max(duration - elapsedTime, 0f);
        public float RemainingPercentage => 1f - ElapsedPercentage;
    }
}