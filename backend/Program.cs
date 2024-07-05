using GeographyGame.Models;
using GeographyGame.Filters;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using NLog.Extensions.Logging;
using NLog.Web;
using NLog;

var builder = WebApplication.CreateBuilder(args);
ConfigSettingLayoutRenderer.DefaultConfiguration = builder.Configuration;
var logger = NLog.LogManager.Setup().GetCurrentClassLogger();
try
{
  logger.Debug("init main");
  builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });

  const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

  // Add services to the container
  builder.Services.AddScoped<BaseAuthorizationFilter>();
  builder.Services.AddScoped<AdminAuthorizationFilter>();
  builder.Services.AddScoped<AdminOrTeacherAuthorizationFilter>();
  builder.Services.AddScoped<OptionalAuthorizationFilter>();
  builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

  builder.Services.AddSwaggerGen();

  builder.Configuration.AddEnvironmentVariables();
  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
  builder.Services.AddDbContext<GeographyGameContext>(options => options.UseNpgsql(connectionString));

  var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
  builder.Services.AddCors(options => options.AddPolicy(name: MyAllowSpecificOrigins, policy => policy.WithOrigins(allowedOrigins).WithHeaders("content-type", "authorization").WithMethods("GET", "POST", "PUT", "DELETE")));

  var app = builder.Build();

  // Configure the HTTP request pipeline.

  app.UseForwardedHeaders(new ForwardedHeadersOptions
  {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
  });

  string? pathBase = app.Configuration["PathBase"];
  if (!string.IsNullOrWhiteSpace(pathBase))
  {
    app.UsePathBase(pathBase);
  }

  if (app.Environment.IsDevelopment())
  {
    app.UseSwagger();
    app.UseSwaggerUI();
  }

  app.UseHttpsRedirection();

  app.UseCors(MyAllowSpecificOrigins);

  app.UseAuthorization();

  app.MapControllers();

  app.Run();
}
catch (Exception exception)
{
  // NLog: catch setup errors
  logger.Error(exception, "Stopped program because of exception");
  throw;
}
finally
{
  // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
  NLog.LogManager.Shutdown();
}





