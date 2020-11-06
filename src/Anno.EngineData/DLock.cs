using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData
{
    /// <summary>
    /// 分布式锁
    /// </summary>
    public class DLock : IDisposable
    {
        private string channel = "Anno.Plugs.DLock";
        private string router = "DLock";
        private bool _disposed;
        private bool _getLockStatus = false;
        private readonly string _key;

        /// <summary>
        /// 超时时间
        /// </summary>
        private readonly int _timeOut = 5000;

        /// <summary>
        /// 所有者
        /// </summary>
        private readonly string _owner;
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key">锁可以</param>
        /// <param name="timeOut">锁超时时间 Default 5000 毫秒 最小1000毫秒。</param>
        public DLock(string key, int timeOut = 5000)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new DLockException($"锁Key不能为null或者string.Empty");
            }
            if (timeOut < 1000)
            {
                timeOut = 1000;
            }
            _disposed = false;
            _key = key;
            this._timeOut = timeOut;
            _owner = Const.SettingService.AppName + "_" + Guid.NewGuid().ToString("N") + System.Threading.Thread.CurrentThread.ManagedThreadId;
            EnterLock();
        }

        private DLock()
        {
        }
        ~DLock()
        {
            Dispose(false);
        }

        private void EnterLock()
        {
            var response = new Dictionary<string, string>();
            response.Add("DLKey", _key);
            response.Add("TimeOut", _timeOut.ToString());
            response.Add("Owner", _owner);
            var rltStr = InvokeEngine.InvokeProcessor(channel, router, "EnterLock", response);
            var rlt = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionResult>(rltStr);
            if (!rlt.Status)
            {
                throw new DLockException("EnterLock Error:" + rlt.Msg);
            }
            _getLockStatus = true;
        }
        private void DisposeLock()
        {
            try
            {
                if (_getLockStatus)
                {
                    var response = new Dictionary<string, string>();
                    response.Add("DLKey", _key);
                    response.Add("Owner", _owner);
                    InvokeEngine.InvokeProcessor(channel, router, "DisposeLock", response);
                }
            }
            catch (Exception)
            {
                //throw new DLockException("DisposeLock:" + e.Message);
            }
        }
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeLock();
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
