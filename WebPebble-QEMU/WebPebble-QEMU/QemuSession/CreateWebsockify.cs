using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebPebble_QEMU
{
    public partial class QemuSession
    {
        /* This file has the creation functions just for Websockify */
        private void CreateWebsockify()
        {
            string args = Program.config.local_name+":"+ websockify_port + " localhost:59"+ sessionId.ToString("00");
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = Program.config.websockify_binary, Arguments = args };
            websockify_process = new Process() { StartInfo = startInfo, };
            websockify_process.Start();
        }
    }
}

