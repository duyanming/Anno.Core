using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace Anno.CronNET
{
    public interface ICronDaemon
    {
        ICronJob AddJob(string schedule, ThreadStart action);
        bool RemoveJob(ICronJob job);
        void Start();
        void Stop();
        DaemonStatus Status { get; set; }
    }

    public class CronDaemon : ICronDaemon
    {
        private readonly System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private readonly List<ICronJob> _cronJobs = new List<ICronJob>();
        private DateTime _last = DateTime.Now;
        private static readonly object JobLock = new object();


        public CronDaemon()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += timer_elapsed;
        }

        public DaemonStatus Status { get; set; } = DaemonStatus.Stop;
         /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="schedule">调度例如：*/30 * * * * ? *</param>
        /// <param name="action">任务</param>
        /// <returns></returns>
        public ICronJob AddJob(string schedule, ThreadStart action)
        {
            var cj = new CronJob(schedule, action);
            lock (JobLock)
            {
                _cronJobs.Add(cj);
            }
            return cj;
        }
        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="job">被移除任务</param>
        /// <returns></returns>
        public bool RemoveJob(ICronJob job)
        {
            lock (JobLock)
            {
                job.abort();
                return _cronJobs.Remove(job);
            }
        }
        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="jobId">被移除任务</param>
        /// <returns></returns>
        public bool RemoveJob(Guid jobId)
        {
            lock (JobLock)
            {
                var job = _cronJobs.Find(j => j.ID == jobId);
                if (job != null)
                {
                    job.abort();
                    _cronJobs.Remove(job);
                }
                return false;
            }
        }
        /// <summary>
        ///  启动任务管理器
        /// </summary>
        public void Start()
        {
              #region 确保执行时间是 每秒的中间时刻执行
            int millisecond = DateTime.Now.Millisecond;
            if (millisecond > 500)
            {
                millisecond = 1500 - millisecond;
            }
            else if (millisecond < 500)
            {
                millisecond = 500 - millisecond;
            }

            if (millisecond != 500)
            {
                Task.Delay(millisecond).Wait();
            }
            #endregion
            _timer.Start();
            Status = DaemonStatus.Started;
        }
         /// <summary>
        ///  停止任务管理器
        /// </summary>
        public void Stop()
        {
            Status = DaemonStatus.Stop;
            _timer.Stop();
            //foreach (CronJob job in cron_jobs)
            //    job.abort();
        }

        private void timer_elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Second != _last.Second)
            {
                _last = DateTime.Now;
                lock (JobLock)
                {
                    foreach (ICronJob job in _cronJobs)
                        job.execute(DateTime.Now);
                }
            }
        }
    }

    public enum DaemonStatus
    {
        Started = 0,
        Stop
    }
}
