﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace WebPebble_QEMU
{
    class Program
    {
        public static ConfigFile config;

        private static WebSocketSharp.Server.WebSocketServer server;

        public static bool[] open_ids;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting QEMU WebPebble...");
            Console.WriteLine("Loading configuration file...");
            //Load the config file.
            config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText("config.json"));
            //Create the open ids array.
            open_ids = new bool[config.max_sessions];
            for (int i = 0; i < config.max_sessions; i++)
                open_ids[i] = false;
            //Start the WebSocket server.
            server = new WebSocketSharp.Server.WebSocketServer(IPAddress.Any, 43187, false);
            server.ReuseAddress = true;
            server.AddWebSocketService<Ws.WebService>("/session");
            server.Start();
            Console.WriteLine("Started server. Press ENTER to quit.");
            Console.ReadLine();
        }
    }
}
