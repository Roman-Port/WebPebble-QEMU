using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace WebPebble_QEMU
{
    class Program
    {
        public static ConfigFile config;

        private static WebSocketSharp.Server.WebSocketServer server;

        public static bool[] open_ids;

        public static Process websockify_process; //Websockify is used for the JS VNC client.

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
            //Start Websockify.
            Console.WriteLine("Starting Websockify...");
            StartWebsockify();
            Console.WriteLine("Websockify started.");
            //Start the WebSocket server.
            Console.WriteLine("Starting WebSocket server...");
            server = new WebSocketSharp.Server.WebSocketServer(IPAddress.Any, 43187, false);
            server.ReuseAddress = true;
            server.AddWebSocketService<Ws.WebService>("/session");
            server.Start();
            Console.WriteLine("Started server. Press ENTER to quit.");
            Console.ReadLine();
            Console.WriteLine("Killing server...");
            websockify_process.Kill();
        }

        static void StartWebsockify()
        {
            string args = "10.0.1.52:43188 10.0.1.52:5901";
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = config.websockify_binary, Arguments = args };
            websockify_process = new Process() { StartInfo = startInfo, };
            websockify_process.Start();
        }
    }
}
