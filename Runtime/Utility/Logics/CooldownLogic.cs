using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class CooldownLogic
    {
        public float Duration { get; private set; } = 1f;
        public bool UseUnscaledTime { get; private set; } = false;

        private float _remainingTime = 0f;
        private bool _isRunning = false;

        private Action onReady;
        private Action notReady;
        private Action onTick;
        private Action onComplete;

        public void Initialize(Action onReady = null, Action notReady = null, Action onTick = null, Action onComplete = null)
        {
            this.onReady = onReady;
            this.notReady = notReady;
            this.onTick = onTick;
            this.onComplete = onComplete;
        }

        public void StartCooldown(float duration, bool useUnscaledTime = false)
        {
            if (IsReady)
            {
                Duration = duration;
                UseUnscaledTime = useUnscaledTime;
                ResetCooldown();
            }
            else
            {
                notReady?.Invoke();
            }
        }

        public void ResetCooldown()
        {
            _remainingTime = Duration;
            _isRunning = true;
            onReady?.Invoke();
        }

        public void Update()
        {
            if (!_isRunning) return;

            _remainingTime -= UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _remainingTime = Mathf.Max(0, _remainingTime);

            onTick?.Invoke();

            if (IsReady)
            {
                _isRunning = false;
                onComplete?.Invoke();
            }
        }

        public float RemainingTime => _remainingTime;
        public float RemainingPercentage => Mathf.Clamp01(_remainingTime / Duration);
        public bool IsReady => _remainingTime <= 0f;
    }
}