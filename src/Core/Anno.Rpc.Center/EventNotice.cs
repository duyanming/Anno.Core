using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Anno.Rpc.Center
{
    /// <summary>
    /// 服务通知
    /// </summary>
    /// <param name="service"></param>
    public delegate void ServiceNotice(ServiceInfo service, NoticeType noticeType);
    /// <summary>
    /// 服务更改
    /// </summary>
    /// <param name="newService">新服务</param>
    /// <param name="oldService">旧服务</param>
    public delegate void ServiceChangeNotice(ServiceInfo newService, ServiceInfo oldService);
    public enum NoticeType
    {
        OnLine = 0,
        OffLine,
        RecoverHealth,
        NotHealth
    }
}
