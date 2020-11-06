using System;
using System.Threading;

namespace Anno.CronNET
{
    public interface ICronJob
    {
        Guid ID { get; }
        void execute(DateTime date_time);
        void abort();
    }

    public class CronJob : ICronJob
    {
        private Guid id = Guid.NewGuid();
        private readonly ICronSchedule _cron_schedule = new CronSchedule();
        private readonly ThreadStart _thread_start;
        private Thread _thread;

        public CronJob(string schedule, ThreadStart thread_start)
        {
            _cron_schedule = new CronSchedule(schedule);
            _thread_start = thread_start;
            _thread = new Thread(thread_start);
        }

        private object _lock = new object();

        public Guid ID { get => this.id; }

        public void execute(DateTime date_time)
        {
            lock (_lock)
            {
                if (!_cron_schedule.isTime(date_time))
                    return;

                if (_thread.ThreadState == ThreadState.Running)
                    return;

                _thread = new Thread(_thread_start);
                _thread.Start();
            }
        }

        public void abort()
        {
#if !NETSTANDARD
            _thread.Abort();
#endif
        }

    }
}
