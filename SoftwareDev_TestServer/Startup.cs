using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

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
                if (context.Request.Path == "/simulation")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await SimulationResponse(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else if (context.Request.Path == "/controller") 
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        // TODO: Make a ControllerResponse
                        await SimulationResponse(context, webSocket);
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

        private static async Task Send(byte[] buffer, WebSocket webSocket, WebSocketReceiveResult result, StringBuilder stringBuilder)
        {
            string returnString = stringBuilder.ToString();
            
            var buffer2 = Encoding.UTF8.GetBytes(returnString);

            await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer2, 0, returnString.Length),
                    result.MessageType,
                    result.EndOfMessage,
                    CancellationToken.None);
        }

        private static async Task SimulationResponse(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var converted = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            Console.WriteLine("Json Received: " + converted);

            Simulation simulation = JsonConvert.DeserializeObject<Simulation>(converted);

            StringBuilder stringBuilder = CheckSimulationValidation(simulation);
            await Send(buffer, webSocket, result, stringBuilder);
            
            if (result.CloseStatus != null)
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
                    CancellationToken.None);
        }

        private static StringBuilder CheckSimulationValidation(Simulation sim)
        {
            SimulationValidator val = new SimulationValidator(sim);
            ValidationResult validationResult = val.Validate(sim);
            StringBuilder stringBuilder = new StringBuilder();
            
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine("Property: " + error.PropertyName + " failed validation. Error: " + error.ErrorMessage);
                    stringBuilder.Append("Property: " + error.PropertyName + " failed validation. Error: " +
                                         error.ErrorMessage);
                }
            }
            else
            {
                Console.WriteLine("Validation passed!");
                stringBuilder.Append("Validation passed!");
            }

            return stringBuilder;
        }
    }
}