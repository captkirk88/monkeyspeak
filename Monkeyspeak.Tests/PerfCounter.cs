using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monkeyspeak.Tests
{
    public class PerfCounter : IDisposable
    {
        private Stopwatch watch;
        private Thread reportTask;
        private CancellationTokenSource cancellationTokenSource;
        private readonly Action<TimeSpan, string> reportAction;
        private readonly TimeSpan periodicReport;

        public PerfCounter(Action<TimeSpan, string> reportAction, TimeSpan periodicReport = default(TimeSpan))
        {
            this.reportAction = reportAction;
            this.periodicReport = periodicReport;
            watch = new Stopwatch();
            cancellationTokenSource = new CancellationTokenSource();
            reportTask = new Thread(Run);
            reportTask.Start();
        }

        private void Run()
        {
            watch.Start();
            if (periodicReport != default(TimeSpan))
            {
                DateTime end = DateTime.Now.Add(periodicReport);
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    if (end < DateTime.Now)
                    {
                        Report();
                        end = DateTime.Now.Add(periodicReport);
                    }
                }
            }
        }

        private void Report()
        {
            reportAction?.Invoke(watch.Elapsed, GC.Statistics);
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancellationTokenSource.Cancel();
                    watch.Stop();
                    Report();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}