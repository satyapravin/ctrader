using System;
using System.Timers;

namespace EmbeddedService
{
    public class Scheduler
    {
        private Timer timer = null;
        private readonly Action functor;
        public Scheduler(Action fn)
        {
            functor = fn;
        }

        public void Repeat(int millseconds)
        {
            timer = new Timer(millseconds)
            {
                AutoReset = true
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            functor();
        }
    }
}
