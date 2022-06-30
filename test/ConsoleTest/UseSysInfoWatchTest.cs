using System;
using System.Collections.Generic;
using System.Text;
using Anno.EngineData.SysInfo;

namespace ConsoleTest
{
    public class UseSysInfoWatchTest
    {
        public void Handle()
        {
            UseSysInfoWatch usi = new UseSysInfoWatch();
            while (true)
            {
                var info = usi.GetServerStatus();
                Console.WriteLine(info.RunTime);
                Console.WriteLine($"CPU:{info.Cpu}");
                Console.WriteLine($"Memory:{info.Memory}");

                Console.WriteLine($"CpuTotalUse:{info.CpuTotalUse}");
                Console.WriteLine($"MemoryTotalUse:{info.MemoryTotalUse}");
                Console.WriteLine($"EngineCounter:{info.EngineCounter}");
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
