using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoftwareDev_TestServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private static async Task Send(byte[] buffer, WebSocket webSocket, WebSocketReceiveResult result)
        {
            const string returnString = "Received, Thank you!";
            var buffer2 = Encoding.UTF8.GetBytes(returnString);

            await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer2, 0, returnString.Length),
                    result.MessageType,
                    result.EndOfMessage,
                    CancellationToken.None);
        }

        private static async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var converted = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            
            while (!result.CloseStatus.HasValue)
            {
                await Send(buffer, webSocket, result);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                var json = JsonConvert.DeserializeObject<TrafficLightModel>(converted);
                ConsoleTests(json);
                
                Console.WriteLine("Json Received: " + converted);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        //TODO replace console test with proper NUnit tests
        private static void ConsoleTests(TrafficLightModel trafficLightModel)
        {
            if (trafficLightModel.A1 == null)
            {
                Console.WriteLine("A1 is empty!");
            }
            else
            {
                Console.WriteLine("A1 set to value: " + trafficLightModel.A1);
            }
            if (trafficLightModel.A2 == null)
            {
                Console.WriteLine("A2 is empty!");
            }else
            {
                Console.WriteLine("A2 set to value: " + trafficLightModel.A2);
            }
            if (trafficLightModel.A3 == null)
            {
                Console.WriteLine("A3 is empty!");
            }else
            {
                Console.WriteLine("A3 set to value: " + trafficLightModel.A3);
            }
            if (trafficLightModel.A4 == null)
            {
                Console.WriteLine("A4 is empty!");
            }else
            {
                Console.WriteLine("A4 set to value: " + trafficLightModel.A4);
            }
        }
    }
}