using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public sealed class ImmediateAnimation
    {
        internal PlayableGraph graph;
        internal DirectorUpdateMode timeScale;
#pragma warning disable 0649
        internal Animator animator;
#pragma warning restore 0649
        internal AnimationClip clip;
        internal AnimationPlayableOutput output;
        internal AnimationClipPlayable playable;
        public bool isPlaying => playable.GetTime() < playable.GetDuration();
        private float delay;
        private bool loop;
        private int loopAmount;
        private bool isDelayableAndActive;
        private WaitForSeconds waitForSeconds;
        private bool resetWaitForSeconds;

        private ImmediateAnimation() { }

        internal ImmediateAnimation(Animator animator, bool unscaledTime = false)
        {
            graph = PlayableGraph.Create();
            timeScale = unscaledTime ? DirectorUpdateMode.UnscaledGameTime : DirectorUpdateMode.GameTime;
            graph.SetTimeUpdateMode(timeScale);
            output = AnimationPlayableOutput.Create(graph, "ImmediateAnimation", animator);
        }

        public void Play(AnimationClip clip = null, float time = 0, float speed = 0)
        {
            if (clip == null && this.clip == null) throw new Exception("An AnimationClip must be set at least once to play.");

            if (clip != null)
            {
                playable = AnimationClipPlayable.Create(graph, clip);
                this.clip = clip;
                output.SetSourcePlayable(playable);
            }

            if (speed == 0f) speed = clip.apparentSpeed;
            playable.SetSpeed(speed);
            playable.SetTime(time);
            playable.SetDuration(clip.averageDuration);
            graph.Play();
        }

        /// <summary>
        /// Plays an AnimationClip as a loop with a possible delay. loopAmount set to 0 is infinite.
        /// </summary>
        public IEnumerator Play(AnimationClip clip = null, float time = 0, float delay = 0, bool loop = false, int loopAmount = 0, float speed = 0)
        {
            if (clip == null && this.clip == null) throw new Exception("An AnimationClip must be set at least once to play.");

            if (clip.isLooping)
            {
                throw new Exception("You cannot play an AnimationClip whose asset is set to loop. Could potentially cause an infinite loop.");
            }

            if (clip != null)
            {
                playable = AnimationClipPlayable.Create(graph, clip);
                this.clip = clip;
                output.SetSourcePlayable(playable);
            }

            isDelayableAndActive = true;
            if (!Mathf.Approximately(this.delay, delay)) resetWaitForSeconds = true;
            this.delay = delay;
            this.loop = loop;
            this.loopAmount = loopAmount;
            if (speed == 0f) speed = clip.apparentSpeed;
            playable.SetSpeed(speed);
            playable.SetTime(time);
            playable.SetDuration(clip.averageDuration);
            yield return PlayWithRoutine();
        }

        internal IEnumerator PlayWithRoutine()
        {
            if (waitForSeconds == null || resetWaitForSeconds) waitForSeconds = new WaitForSeconds(delay);
            yield return waitForSeconds;

            int loopsComplete = 0;
            graph.Play();

            if (loopAmount > 0)
            {
                while (loopsComplete < loopAmount)
                {
                    yield return new WaitWhile(() => isPlaying);
                    playable.SetTime(0f);
                    loopsComplete++;
                    yield return null;
                }
            }
            else
            {
                loopsComplete = -1;

                while (loopsComplete < loopAmount)
                {
                    yield return new WaitWhile(() => isPlaying);
                    playable.SetTime(0f);
                    yield return null;
                }
            }

            graph.Stop();

            isDelayableAndActive = false;
        }

        public void Stop(bool destroy, MonoBehaviour behaviour = null)
        {
            if (isDelayableAndActive && behaviour != null) behaviour.StopCoroutine(PlayWithRoutine());
            graph.Stop();
            if (destroy) graph.Destroy();
        }

        public void Speed(float speed)
        {
            playable.SetSpeed(speed);
        }

        public void Goto(float time)
        {
            playable.SetTime(time);
        }
    }
}