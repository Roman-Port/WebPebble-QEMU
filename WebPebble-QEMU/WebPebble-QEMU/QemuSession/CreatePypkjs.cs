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
        /* This file has the creation functions just for Pypkjs */
        private void StartPypjks()
        {
            //Run some yucky Python code.
            //Get platform.
            FlashPair fp = Program.config.flash_bins["basalt"];
            string persist = persist_dir + "pypkjs/";
            Directory.CreateDirectory(persist);
            //Create arguments
            string args = "--qemu localhost:"+qemu_serial_port.ToString()+" ";
            args += "--port " + pypkjs_port.ToString() + " ";
            args += "--persist " + persist + " ";
            args += "--layout " + fp.layouts + " ";
            args += "--debug ";
            //Run
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/usr/bin/python", Arguments = Program.config.pypkjs_binary + " " + args };
            pypkjs_process = new Process() { StartInfo = startInfo, };
            pypkjs_process.Start();
        }
    }
}
