using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json.Linq;
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

        private static async Task Send(byte[] buffer, WebSocket webSocket, WebSocketReceiveResult result, object stringBuilder)
        {
            string returnString = stringBuilder.ToString();

            var buffer2 = Encoding.UTF8.GetBytes(returnString);

            await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer2, 0, returnString.Length),
                    result.MessageType,
                    result.EndOfMessage,
                    CancellationToken.None);
        }

        //TODO: Make more readable in smaller function @MK
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
                    
                    List<String> stringList = typeof(Simulation).GetProperties().Select(property => property.Name).ToList();
                    List<String> jsonList = new List<string>(); 
                    
                    var data = (JObject) JsonConvert.DeserializeObject(converted);

                    foreach (var item in data)
                    {
                        jsonList.Add(item.Key);
                    }
                    
                    if (!stringList.Intersect(jsonList).SequenceEqual(jsonList))
                    {
                        stringBuilder = new StringBuilder("Wrong alphabetical order");
                        break;
                    }

                    SimulationValidation simVal = new SimulationValidation();
                    ValidationResult simValResult = simVal.Validate(simulation);
                    stringBuilder = Validate(simValResult);
                    break;
                case "/controller":
                    Controller controller = JsonConvert.DeserializeObject<Controller>(converted);
                    
                    List<String> contStringList = typeof(Controller).GetProperties().Select(property => property.Name).ToList();
                    List<String> contJsonList = new List<string>(); 
                    
                    var contData = (JObject) JsonConvert.DeserializeObject(converted);
                    
                    foreach (var item in contData)
                    {
                        contJsonList.Add(item.Key);
                    }

                    if (!contStringList.Intersect(contJsonList).SequenceEqual(contJsonList))
                    {
                        stringBuilder = new StringBuilder("Wrong alphabetical order");
                        break;
                    }
                    
                    ControllerValidation contVal = new ControllerValidation();
                    ValidationResult contValResult = contVal.Validate(controller);
                    stringBuilder = Validate(contValResult);
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
        
        private static StringBuilder Validate(ValidationResult validationResult)
        {
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
