using System.Diagnostics;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Time")]
    [UnitTitle("StopWatch")]
    [TypeIcon(typeof(Timer))]
    public class StopwatchUnit : Unit
    {
        public Stopwatch stopwatch;

        [DoNotSerialize]
        public ControlInput start;

        [DoNotSerialize]
        public ControlInput stop;

        [DoNotSerialize]
        public ControlOutput started;

        [DoNotSerialize]
        public ControlOutput stopped;


        [DoNotSerialize]
        public ValueOutput elapsedSeconds;

        [DoNotSerialize]
        public ValueOutput isRunning;

        protected override void Definition()
        {
            started = ControlOutput("Started");
            stopped = ControlOutput("Stopped");

            start = ControlInput("Start", StartStopwatch);
            stop = ControlInput("Stop", StopStopwatch);


            elapsedSeconds = ValueOutput(nameof(elapsedSeconds), GetElapsedSeconds);
            isRunning = ValueOutput(nameof(isRunning), IsRunning);
            Succession(start, started);
            Succession(stop, stopped);
        }

        private ControlOutput StartStopwatch(Flow flow)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            return started;
        }

        private ControlOutput StopStopwatch(Flow flow)
        {
            stopwatch?.Stop();
            return stopped;
        }

        private float GetElapsedSeconds(Flow flow)
        {
            return stopwatch != null ? (float)stopwatch.Elapsed.TotalSeconds : 0f;
        }

        private bool IsRunning(Flow flow)
        {
            return stopwatch != null && stopwatch.IsRunning;
        }
    }
}
