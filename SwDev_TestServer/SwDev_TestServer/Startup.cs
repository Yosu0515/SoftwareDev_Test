using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SwDev_TestServer.Models;
using SwDev_TestServer.Validators;
using Controller = SwDev_TestServer.Models.Controller;

namespace SwDev_TestServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            IgnoreBadCertificates();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseWebSockets();
            app.UseMvc();

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
                        Console.WriteLine("Not a valid websocket request");
                    }
                }
                else if (context.Request.Path == "/controller")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await ControllerResponse(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        Console.WriteLine("Not a valid websocket request");
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

        // TODO: Make Generic
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
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
                    CancellationToken.None);
                Console.WriteLine("Closing Connection");
            }

        }

        // TODO: Make Generic
        private static async Task ControllerResponse(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var converted = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            Console.WriteLine("Json Received: " + converted);

            Controller controller = JsonConvert.DeserializeObject<Controller>(converted);

            StringBuilder stringBuilder = CheckControllerValidation(controller);
            await Send(buffer, webSocket, result, stringBuilder);

            if (result.CloseStatus != null)
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
                    CancellationToken.None);
                Console.WriteLine("Closing Connection");
            }
        }

        private static StringBuilder CheckSimulationValidation(Simulation sim)
        {
            SimulationValidation val = new SimulationValidation();
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

        private static StringBuilder CheckControllerValidation(Controller con)
        {
            ControllerValidation val = new ControllerValidation();
            ValidationResult validationResult = val.Validate(con);
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

        public static void IgnoreBadCertificates()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
