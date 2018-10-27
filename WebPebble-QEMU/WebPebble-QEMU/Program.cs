using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebPebble_QEMU
{
    class Program
    {
        public static ConfigFile config;

        public static bool[] open_ids;

        public static List<Ws.WebService> connected = new List<Ws.WebService>();

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
            Console.WriteLine("Starting WebSocket server...");
            /*server = new WebSocketSharp.Server.WebSocketServer(IPAddress.Any, 43189, false);
            server.ReuseAddress = true;
            server.AddWebSocketService<Ws.WebService>("/session");
            server.Start();*/
            MainAsync().GetAwaiter().GetResult();
            Console.WriteLine("Started server. Press ENTER to quit.");
            Console.ReadLine();
            Console.WriteLine("Killing server...");
        }

        public void Configure(IApplicationBuilder app)
        {
            //app.Run(OnHttpRequest);
            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/session")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Ws.WebService.OnWebSock(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                    await next();
                }

            });
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Parse(Program.config.local_name);
                    options.Listen(addr, 43189);
                    /*options.Listen(addr, 443, listenOptions =>
                    {
                        listenOptions.UseHttps(LibRpwsCore.config.ssl_cert_path, "");
                    });*/

                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }
    }
}
