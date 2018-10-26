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
        public Process qemu_process;
        public Process pypkjs_process;
        public int sessionId;
        public string unique_sessionId;
        public string persist_dir;


        //IMAGES
        public string qemu_spi_image;
        public string qemu_micro_image;

        //PORTS
        public int qemu_port;
        public int qemu_serial_port;
        public int qemu_gdb_port;
        public int pypkjs_port;

        //SOCKETS
        public Socket qemu_serial_client;

        public static QemuSession SpawnSession(int sessionId)
        {
            //Create the QEMU session and object here.
            QemuSession s = new QemuSession();
            s.sessionId = sessionId;
            s.unique_sessionId = DateTime.UtcNow.Ticks.ToString();
            //Choose ports.
            int basePort = (sessionId * 4) + Program.config.private_port_start;
            s.qemu_port = basePort;
            s.qemu_serial_port = basePort + 1;
            s.qemu_gdb_port = basePort + 2;
            s.pypkjs_port = basePort + 3;
            //Create persist dir.
            s.persist_dir = Program.config.persist_dir.Replace("SESSION", s.unique_sessionId);
            Directory.CreateDirectory(s.persist_dir);
            //Copy firmware images.
            s.CopyImages();
            //Start QEMU
            s.SpawnProcess();
            //Begin trying to connect.
            s.WaitForQemu();
            Thread.Sleep(2000);
            //Start Pypkjs.
            s.StartPypjks();

            return s;
        }

        private void Log(string msg)
        {
            Console.WriteLine("[QEMU SESSION " + sessionId.ToString() + "] " + msg);
        }
    }
}
