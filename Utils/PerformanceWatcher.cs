using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Utils
{
    class PerformanceWatcher : IDisposable
    {
        Stopwatch watch;
        string name;

        public static PerformanceWatcher Start(string name)
        {
            return new PerformanceWatcher(name);
        }

        private PerformanceWatcher(string name)
        {
            this.name = name;
            this.watch = new Stopwatch();
            this.watch.Start();
        }

        #region IDisposable implementation

        // dispose stops stopwatch and prints time, could do anytying here
        public void Dispose()
        {
            this.watch.Stop();
        }

        #endregion
    }
}
