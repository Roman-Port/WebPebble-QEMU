using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WebPebble_QEMU
{
    public class QemuSession
    {
        public Process process;

        public static QemuSession SpawnSession()
        {
            //Create the QEMU session and object here.
            QemuSession s = new QemuSession();
            //Run the QEMU process.
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/usr/bin/python3", Arguments = "" };
            s.process = new Process() { StartInfo = startInfo, };
            s.process.Start();
            

            return s;
        }
    }
}
