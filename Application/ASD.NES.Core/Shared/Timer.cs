using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASD.NES.Core.Shared {

    internal sealed class Timer : IDisposable {

        private CancellationTokenSource cancellation;
        private DateTime prevStep = DateTime.Now;

        public TimeSpan Interval { get; set; }
        public event Action Tick;

        public void Dispose() => Stop();

        public Timer(TimeSpan interval = default(TimeSpan), Action tick = default(Action)) {
            Interval = interval; Tick += tick;
        }

        public void Start() {
            Stop(); Task.Factory.StartNew(Process, (cancellation = new CancellationTokenSource()).Token);
        }

        public void Stop() => cancellation?.Cancel();

        private void Process() {
            var token = cancellation.Token;
            while (!token.IsCancellationRequested) { Step(); }
        }

        private void Step() {

            var now = DateTime.Now;
            var nxt = prevStep + Interval;

            if (now > nxt) {
                Tick?.Invoke();
                prevStep = now;
            }
            else {
                Task.Delay(TimeSpan.FromTicks(1)).Wait(); // 10 000 = 1ms // 10 = 0.0001 ms
            }
        }
    }
}