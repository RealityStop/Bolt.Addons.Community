using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class TimerLogic
    {
        public float duration = 1f;
        public bool unscaled = false;

        private float elapsedTime = 0f;
        private bool isRunning = false;

        private Action onCompleted;
        private Action onTick;

        public void Initialize(Action onCompleted = null, Action onTick = null)
        {
            this.onCompleted = onCompleted;
            this.onTick = onTick;
        }

        public void Update()
        {
            if (!isRunning) return;

            elapsedTime += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

            onTick?.Invoke();

            if (elapsedTime >= duration)
            {
                onCompleted?.Invoke();
                isRunning = false;
            }
        }

        public void StartTimer(float duration, bool unscaled)
        {
            this.duration = duration;
            this.unscaled = unscaled;
            elapsedTime = 0f;
            isRunning = true;
        }

        public void StartTimer(float duration, bool unscaled, Action onStarted)
        {
            StartTimer(duration, unscaled);
            onStarted?.Invoke();
        }

        public void PauseTimer() => isRunning = false;
        public void ResumeTimer() => isRunning = true;
        public void ToggleTimer() => isRunning = !isRunning;

        public float Elapsed => Mathf.Min(elapsedTime, duration);
        public float ElapsedPercentage => Mathf.Clamp01(elapsedTime / duration);
        public float Remaining => Mathf.Max(duration - elapsedTime, 0f);
        public float RemainingPercentage => 1f - ElapsedPercentage;
    }
}