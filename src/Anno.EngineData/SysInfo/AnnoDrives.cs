using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Anno.EngineData.SysInfo
{
    public class AnnoDrives
    {
        public static List<AnnoDrive> GetDrivesInfo()
        {
            List<AnnoDrive> drives = new List<AnnoDrive>();
            foreach (var d in DriveInfo.GetDrives())
            {
                if (d.DriveType == DriveType.CDRom)
                {
                    continue;
                }
                if (d.TotalSize >= 1024 * 1024)//磁盘大于1M
                {
                    double freeSpace = 0;
                    if (d.TotalFreeSpace > 0)
                    {
                        freeSpace = Math.Round(d.TotalFreeSpace / 1024 / 1024 / 1024.000, 3);
                    }
                    var totalSpace = Math.Round(d.TotalSize / 1024 / 1024 / 1024.000, 3);
                    drives.Add(new AnnoDrive() { Name = d.Name.Replace(":\\", ":"), Total = totalSpace, Free = freeSpace });
                }
            }
            return drives;
        }
    }

    public class AnnoDrive
    {
        public string Name { get; set; }
        public double Total { get; set; }
        public double Free { get; set; }
    }
}
