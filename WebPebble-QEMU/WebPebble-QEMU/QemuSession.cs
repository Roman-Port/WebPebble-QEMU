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
        //PROCESSES
        public Process qemu_process;
        public Process pypkjs_process;
        public Process websockify_process;

        //MAIN
        public int sessionId;
        public string unique_sessionId;
        public string persist_dir;
        public string platform;

        //IMAGES
        public string qemu_spi_image;
        public string qemu_micro_image;

        //PORTS
        public int qemu_port;
        public int qemu_serial_port;
        public int qemu_gdb_port;
        public int pypkjs_port;
        public int websockify_port;

        //SOCKETS
        public Socket qemu_serial_client;

        public static QemuSession SpawnSession(int sessionId, string platform)
        {
            //Create the QEMU session and object here.
            QemuSession s = new QemuSession();
            s.sessionId = sessionId;
            s.unique_sessionId = DateTime.UtcNow.Ticks.ToString();
            s.platform = platform;
            //Choose ports.
            int basePort = (sessionId * 5) + Program.config.private_port_start;
            s.qemu_port = basePort;
            s.qemu_serial_port = basePort + 1;
            s.qemu_gdb_port = basePort + 2;
            s.pypkjs_port = basePort + 3;
            s.websockify_port = basePort + 4;
            //Create persist dir.
            s.persist_dir = Program.config.persist_dir.Replace("SESSION", s.unique_sessionId);
            Directory.CreateDirectory(s.persist_dir);
            //Copy firmware images.
            s.CopyImages();
            //Start QEMU
            s.SpawnProcess();
            //Begin trying to connect.
            s.WaitForQemu();
            //Start Websockify.
            s.CreateWebsockify();
            //Wait.
            Thread.Sleep(1000);
            //Start Pypkjs.
            s.StartPypjks();
            return s;
        }

        public void EndSession()
        {
            //Clean up and end this session.
            Log("Session end command issued.");
            //Terminate PYPJKS.
            pypkjs_process.Kill();
            Log("Killed Pypkjs.");
            //Kill QEMU
            qemu_process.Kill();
            Log("Killed QEMU.");
            //Kill websockify.
            websockify_process.Kill();
            Log("Killed Websockify.");
            //Delete the session folder.
            Directory.Delete(persist_dir, true);
            Log("Deleted session directory.");
            Log("Goodbye, world!"); //haha very clever
        }

        private void Log(string msg)
        {
            Console.WriteLine("[QEMU SESSION " + sessionId.ToString() + "] " + msg);
        }
    }
}
