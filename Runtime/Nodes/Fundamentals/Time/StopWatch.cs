using System.Diagnostics;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Time")]
    [UnitTitle("Stopwatch")]
    [TypeIcon(typeof(Timer))]
    public class StopwatchUnit : Unit, IGraphElementWithData
    {
        private class Data : IGraphElementData
        {
            public Stopwatch Stopwatch;
        }

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
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            stopwatch ??= new Stopwatch();
            stopwatch.Start();
            return Started;
        }

        private ControlOutput ResetStopwatch(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            stopwatch?.Reset();
            return null;
        }

        private ControlOutput StopStopwatch(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            stopwatch?.Stop();
            return Stopped;
        }

        private float GetElapsedMilliseconds(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            return stopwatch != null ? stopwatch.Elapsed.Milliseconds : 0f;
        }

        private float GetElapsedSeconds(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            return stopwatch != null ? (float)stopwatch.Elapsed.TotalSeconds : 0f;
        }

        private float GetElapsedMinutes(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            return stopwatch != null ? (float)stopwatch.Elapsed.TotalMinutes : 0f;
        }

        private float GetElapsedHours(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            return stopwatch != null ? (float)stopwatch.Elapsed.TotalHours : 0f;
        }

        private bool GetIsRunning(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var stopwatch = data.Stopwatch;
            return stopwatch != null && stopwatch.IsRunning;
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }
    }
}
