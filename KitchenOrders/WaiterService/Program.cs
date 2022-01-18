using System.Runtime.InteropServices;
using Common.Kafka;
using Confluent.Kafka;
using KitchenOrders.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        // Setup a HTTP/2 endpoint without TLS.
        options.ListenLocalhost(5000, o => o.Protocols =
            HttpProtocols.Http2);
    });
}

builder.Services.AddGrpc();
            
var kafkaOptions = new KafkaOptions();
builder.Configuration.Bind("Kafka", kafkaOptions);
            
builder.Services.AddConsumer<Null, Order>(kafkaOptions, "Solo group");

builder.Services.AddSingleton<ChitService>();

var app=builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<WaiterService.Services.WaiterService>();

    endpoints.MapGet("/",
        async context =>
        {
            await context.Response.WriteAsync(
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        });
});

app.Services.GetRequiredService<ChitService>();

app.Run();