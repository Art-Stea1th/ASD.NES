using System;
using System.Threading.Tasks;

namespace ASD.NES.Core.Shared {

    internal sealed class Timer {

        private bool alive = false;
        private DateTime lastUpdate = DateTime.Now;
        public TimeSpan Interval { get; set; }
        public event Action Tick;

        public Timer(TimeSpan interval = default(TimeSpan), Action tick = default(Action)) {
            Interval = interval; Tick += tick;            
        }

        public void Start() => new Task(Process).Start();
        public void Stop() => alive = false;

        private void Process() {
            alive = true;
            while (alive) { Step(); }
        }

        private void Step() {

            var now = DateTime.Now;

            if (lastUpdate + Interval < now) {
                Tick?.Invoke();
                lastUpdate = now;
            }
            else {
                Task.Delay(TimeSpan.FromMilliseconds(Interval.TotalMilliseconds / 4)).Wait();
            }
        }
    }
}