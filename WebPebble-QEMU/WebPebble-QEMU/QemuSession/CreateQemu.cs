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
        /* This file has the creation functions just for QEMU */
        private void SpawnProcess()
        {
            //Get objects, such as the flash locations.
            FlashPair fp = Program.config.flash_bins["basalt"];
            //Create command line arguments.
            string args = "-rtc base=localtime ";
            args += "-serial null ";
            args += "-serial tcp::" + qemu_port + ",server,nowait ";
            args += "-serial tcp::" + qemu_serial_port + ",server ";
            args += "-drive file=" + qemu_micro_image + ",if=pflash,format=raw ";
            args += "-gdb tcp::" + qemu_gdb_port + ",server ";
            args += "-vnc :" + sessionId.ToString() + " "; //This pushes the video output to vnc. See more here: https://stackoverflow.com/questions/22967925/running-qemu-remotely-via-ssh
            //Get the command line arguments from the specific platform.
            foreach (string ss in fp.args)
            {
                args += ss.Replace("qemu_spi_flash", qemu_spi_image) + " ";
            }
            //Run the QEMU process.
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = Program.config.qemu_binary, Arguments = args };
            qemu_process = new Process() { StartInfo = startInfo, };
            qemu_process.Start();
            Log("QEMU process started with ID " + qemu_process.Id);
        }

        private void CopyImages()
        {
            //Copy images for spi and micro.
            //Get platform.
            FlashPair fp = Program.config.flash_bins["basalt"];
            //Generate paths.
            qemu_spi_image = persist_dir + "pebble_spi_image.bin";
            qemu_micro_image = persist_dir + "pebble_micro_image.bin";
            //Copy now.
            File.Copy(fp.spi_flash, qemu_spi_image);
            File.Copy(fp.micro_flash, qemu_micro_image);
        }

        private void WaitForQemu()
        {
            //Keep trying to connect to QEMU.
            Log("Waiting for firmware to boot. Using IP " + qemu_serial_port.ToString());
            int i = 0;
            for (i = 0; i < 40; i++)
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
                }
                catch (Exception ex)
                {
                    Log("Connection failed on attempt #" + i.ToString() + "; " + ex.Message);
                    qemu_serial_client = null;
                }
            }
            //Check if this timed out.
            if (qemu_serial_client == null)
            {
                throw new Exception("Timed out while waiting for QEMU firmware to boot.");
            }
            Log("Got client connection. Waiting for ready.");
            //Ignore messages until boot is done. This is a bit gross
            List<byte> buf = new List<byte>();
            i = 0;
            using(FileStream fs = new FileStream("/home/roman/temp_dump.txt", FileMode.OpenOrCreate))
            {
                while (true)
                {
                    byte[] mini_buf = new byte[1];
                    qemu_serial_client.Receive(mini_buf, 0, 1, SocketFlags.None);
                    fs.Write(mini_buf, 0, 1);
                    buf.Add(mini_buf[0]);
                    i++;
                    //Check if QEMU is telling us we're ready.
                    string s = Encoding.ASCII.GetString(buf.ToArray());
                    if (s.Contains("<SDK Home>") || s.Contains("<Launcher>") || s.Contains("Ready for communication"))
                        break;
                    
                    //Log(Encoding.ASCII.GetString(buf));
                }
            }
            //We're ready.
        }
    }
}
