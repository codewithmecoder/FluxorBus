using FluxorBus.DependencyInjection;
using FluxorBus.SourceGen;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .ReadFrom.Services(services)
          .WriteTo.Console());

// FluxorBus core
builder.Services
    .AddFluxorBus(opt =>
    {
        opt.EnableBatchConsume = true;
        opt.BatchSize = 32;
        opt.BatchTimeReleased = 5000; // ms
        opt.Capacity = 10;
    })
    .AddFluxorBusGenerated();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

await app.RunAsync();
