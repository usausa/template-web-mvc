// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("webapp", "../Template.WebApp.Host/Template.WebApp.Host.csproj")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
