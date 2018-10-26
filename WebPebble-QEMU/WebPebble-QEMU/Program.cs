﻿using Newtonsoft.Json;
using System;
using System.IO;

namespace WebPebble_QEMU
{
    class Program
    {
        public static ConfigFile config;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting QEMU WebPebble...");
            Console.WriteLine("Loading configuration file...");
            //Load the config file.
            config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText("config.json"));
            //This is for testing before this is made into a real thing.
            QemuSession s = null;
            try
            {
                s = QemuSession.SpawnSession(13);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "@" + ex.StackTrace);
            }
            s.qemu_process.Kill();
        }
    }
}
