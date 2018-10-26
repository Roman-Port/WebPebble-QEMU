using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebPebble_QEMU
{
    public class QemuSession
    {
        public Process process;
        public int sessionId;

        //PORTS
        public int qemu_port;
        public int qemu_serial_port;
        public int qemu_gdb_port;

        //SOCKETS
        public TcpClient qemu_client;
        public TcpClient qemu_serial_client;
        public TcpClient qemu_gdb_client;

        public static QemuSession SpawnSession(int sessionId)
        {
            //Create the QEMU session and object here.
            QemuSession s = new QemuSession();
            s.sessionId = sessionId;
            //Start QEMU
            s.SpawnProcess();
            //Begin trying to connect.
            s.WaitForQemu();

            return s;
        }

        private void SpawnProcess()
        {
            //Get objects, such as the flash locations.
            FlashPair fp = Program.config.flash_bins["basalt"];
            //Choose ports.
            int basePort = (sessionId * 3) + Program.config.private_port_start;
            qemu_port = basePort;
            qemu_serial_port = basePort + 1;
            qemu_gdb_port = basePort + 1;
            //Create command line arguments.
            string args = "-rtc base=localtime ";
            args += "-serial null ";
            args += "-serial null ";
            args += "-serial tcp::" + qemu_port + ",server,nowait ";
            args += "-serial tcp::" + qemu_serial_port + ",server ";
            args += "-pflash " + fp.micro_flash + " ";
            args += "-gdb tcp::" + qemu_gdb_port + ",server ";
            //Get the command line arguments from the specific platform.
            foreach (string ss in fp.args)
            {
                args += ss + " ";
            }
            //Run the QEMU process.
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = Program.config.qemu_binary, Arguments = args };
            process = new Process() { StartInfo = startInfo, };
            process.Start();
            Log("QEMU process started with ID " + process.Id);
        }

        private void WaitForQemu()
        {
            //Keep trying to connect to QEMU.
            Log("Waiting for firmware to boot.");
            for(int i = 0; i<40; i++)
            {
                try
                {
                    Thread.Sleep(200);
                    qemu_serial_client = new TcpClient();
                    qemu_serial_client.ReceiveTimeout = 500;
                    qemu_serial_client.SendTimeout = 500;
                    qemu_serial_client.Connect(new IPEndPoint(IPAddress.Loopback, qemu_serial_port));
                } catch (Exception ex)
                {
                    Log("Connection failed on attempt #" + i.ToString() + "; " + ex.Message);
                    qemu_serial_client = null;
                }
            }
            //Check if this timed out.
            if(qemu_serial_client == null)
            {
                throw new Exception("Timed out while waiting for QEMU firmware to boot.");
            }
            Log("Got client connection. Waiting for ready.");
            //Ignore messages until boot is done.
            byte[] buf = new byte[256];
            while(true)
            {
                qemu_serial_client.Client.Receive(buf);
                Log("Got " + buf.Length.ToString() + " bytes of data: " + Encoding.ASCII.GetString(buf).ToString());
            }
        }

        private void Log(string msg)
        {
            Console.WriteLine("[QEMU SESSION " + sessionId.ToString() + "] " + msg);
        }
    }
}
