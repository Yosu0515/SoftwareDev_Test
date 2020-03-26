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
using Microsoft.CodeAnalysis;
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
        public Boolean isConnected = false;
        private WebSocket clientConnection = null;
        
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
            
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseWebSockets();
            app.UseMvc();

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    try
                    {
                        clientConnection = await context.WebSockets.AcceptWebSocketAsync();
                        isConnected = true;
                    }
                    catch (WebSocketException e)
                    {
                        Console.WriteLine(e);
                    }

                    while (isConnected)
                    {
                        await Response(context, clientConnection);
                    }
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

        private static async Task Response(HttpContext context, WebSocket clientConnection)
        {
            var buffer = new byte[1024 * 4];
            var result = await clientConnection.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            StringBuilder stringBuilder = new StringBuilder();
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await clientConnection.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close received, closing", CancellationToken.None);
            }
            
            var converted = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            Console.WriteLine("Json Received: " + converted);
            
            // Switch statement to determine which response should be given depending on the httpcontext request path
            switch (context.Request.Path)
            {
                case "/simulation":
                    Simulation simulation = JsonConvert.DeserializeObject<Simulation>(converted);
                    stringBuilder = CheckSimulationValidation(simulation);
                    break;
                case "/controller":
                    Controller controller = JsonConvert.DeserializeObject<Controller>(converted);
                    stringBuilder = CheckControllerValidation(controller);
                    break;
                default:
                context.Response.StatusCode = 400;
                Console.WriteLine("Invalid http path");
                await clientConnection.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Closed", CancellationToken.None);
                break;
            }
            
            //     if (result.CloseStatus != null)
            //     {
            //         await clientConnection.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
            //             CancellationToken.None);
            //         Console.WriteLine("Closing Connection");
            //     }

            await Send(buffer, clientConnection, result, stringBuilder);
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
