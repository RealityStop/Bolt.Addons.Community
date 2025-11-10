using System.Diagnostics;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Time")]
    [UnitTitle("StopWatch")]
    [TypeIcon(typeof(Timer))]
    public class StopwatchUnit : Unit
    {
        public Stopwatch Stopwatch { get; private set; }

        [DoNotSerialize]
        public ControlInput Start { get; private set; }

        [DoNotSerialize]
        public ControlInput Reset { get; private set; }

        [DoNotSerialize]
        public ControlInput Stop { get; private set; }

        [DoNotSerialize]
        public ControlOutput Started { get; private set; }

        [DoNotSerialize]
        public ControlOutput Stopped { get; private set; }

        [DoNotSerialize]
        public ValueOutput ElapsedSeconds { get; private set; }
        [DoNotSerialize]
        public ValueOutput ElapsedMilliseconds { get; private set; }
        [DoNotSerialize]
        public ValueOutput ElapsedMinutes { get; private set; }
        [DoNotSerialize]
        public ValueOutput ElapsedHours { get; private set; }

        [DoNotSerialize]
        public ValueOutput IsRunning { get; private set; }

        [Inspectable]
        public bool milliseconds;
        [Inspectable]
        public bool seconds = true;
        [Inspectable]
        public bool minutes;
        [Inspectable]
        public bool hours;

        protected override void Definition()
        {
            Started = ControlOutput("Started");
            Stopped = ControlOutput("Stopped");

            Start = ControlInput("Start", StartStopwatch);
            Stop = ControlInput("Stop", StopStopwatch);
            Reset = ControlInput("Reset", ResetStopwatch);

            if (milliseconds)
            {
                ElapsedMilliseconds = ValueOutput("elapsedMilliseconds", GetElapsedMilliseconds);
                Assignment(Start, ElapsedMilliseconds);
            }
            if (seconds)
            {
                ElapsedSeconds = ValueOutput("elapsedSeconds", GetElapsedSeconds);
                Assignment(Start, ElapsedSeconds);
            }
            if (minutes)
            {
                ElapsedMinutes = ValueOutput("elapsedMinutes", GetElapsedMinutes);
                Assignment(Start, ElapsedMinutes);
            }
            if (hours)
            {
                ElapsedHours = ValueOutput("elapsedHours", GetElapsedHours);
                Assignment(Start, ElapsedHours);
            }
            IsRunning = ValueOutput("isRunning", GetIsRunning);

            Succession(Start, Started);
            Succession(Stop, Stopped);
        }

        private ControlOutput StartStopwatch(Flow flow)
        {
            Stopwatch ??= new Stopwatch();
            Stopwatch.Start();
            return Started;
        }

        private ControlOutput ResetStopwatch(Flow flow)
        {
            Stopwatch?.Reset();
            return null;
        }

        private ControlOutput StopStopwatch(Flow flow)
        {
            Stopwatch?.Stop();
            return Stopped;
        }

        private float GetElapsedMilliseconds(Flow flow)
        {
            return Stopwatch != null ? Stopwatch.Elapsed.Milliseconds : 0f;
        }

        private float GetElapsedSeconds(Flow flow)
        {
            return Stopwatch != null ? (float)Stopwatch.Elapsed.TotalSeconds : 0f;
        }

        private float GetElapsedMinutes(Flow flow)
        {
            return Stopwatch != null ? (float)Stopwatch.Elapsed.TotalMinutes : 0f;
        }

        private float GetElapsedHours(Flow flow)
        {
            return Stopwatch != null ? (float)Stopwatch.Elapsed.TotalHours : 0f;
        }

        private bool GetIsRunning(Flow flow)
        {
            return Stopwatch != null && Stopwatch.IsRunning;
        }
    }
}
