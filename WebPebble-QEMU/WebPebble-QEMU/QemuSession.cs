﻿using System;
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
        public Socket qemu_serial_client;

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
            qemu_gdb_port = basePort + 2;
            //Create command line arguments.
            string args = "-rtc base=localtime ";
            args += "-serial null ";
            args += "-serial tcp::" + qemu_port + ",server,nowait ";
            args += "-serial tcp::" + qemu_serial_port + ",server ";
            args += "-drive file="+ fp.micro_flash +",if=pflash,format=raw ";
            args += "-gdb tcp::" + qemu_gdb_port + ",server ";
            args += "-vnc :"+sessionId.ToString()+" "; //This pushes the video output to vnc. See more here: https://stackoverflow.com/questions/22967925/running-qemu-remotely-via-ssh
            //Get the command line arguments from the specific platform.
            foreach (string ss in fp.args)
            {
                args += ss.Replace("qemu_spi_flash",fp.spi_flash) + " ";
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
            Log("Waiting for firmware to boot. Using IP "+qemu_serial_port.ToString());
            int i = 0;
            for (i = 0; i<40; i++)
            {
                try
                {
                    Thread.Sleep(50);
                    qemu_serial_client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    qemu_serial_client.ReceiveTimeout = 20000;
                    IAsyncResult result = qemu_serial_client.BeginConnect(IPAddress.Loopback, qemu_serial_port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(200, true);
                    if (qemu_serial_client.Connected)
                    {
                        qemu_serial_client.EndConnect(result);
                        break;
                    }
                    throw new Exception("Timed out, trying again.");
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
            //Ignore messages until boot is done. This is a bit gross
            byte[] buf = new byte[2];
            qemu_serial_client.Receive(buf);
            Log(Encoding.ASCII.GetString(buf));
        }

        private void Log(string msg)
        {
            Console.WriteLine("[QEMU SESSION " + sessionId.ToString() + "] " + msg);
        }
    }
}
