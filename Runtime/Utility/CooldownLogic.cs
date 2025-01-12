using System;
using System.Linq;
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

        // Events
        public event Action OnReady
        {
            add
            {
                if (onReady == null || !onReady.GetInvocationList().Contains(value))
                {
                    onReady += value;
                }
            }
            remove
            {
                if (onReady != null && onReady.GetInvocationList().Contains(value))
                {
                    onReady -= value;
                }
            }
        }

        public event Action NotReady
        {
            add
            {
                if (notReady == null || !notReady.GetInvocationList().Contains(value))
                {
                    notReady += value;
                }
            }
            remove
            {
                if (notReady != null && notReady.GetInvocationList().Contains(value))
                {
                    notReady -= value;
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

        public event Action OnCompleteAction
        {
            add
            {
                if (onComplete == null || !onComplete.GetInvocationList().Contains(value))
                {
                    onComplete += value;
                }
            }
            remove
            {
                if (onComplete != null && onComplete.GetInvocationList().Contains(value))
                {
                    onComplete -= value;
                }
            }
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

        public bool IsReady => _remainingTime <= 0;
    }
}