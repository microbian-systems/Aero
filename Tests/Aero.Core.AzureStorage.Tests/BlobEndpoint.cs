using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aero.Core.AzureStorage.Tests;

// A synthetic loopback service exercises the real SDK, without Azure credentials
// or an emulator. Both SDK generations run the same characterization suite.
internal sealed class BlobEndpoint : IAsyncDisposable
{
    private readonly WebApplication app;
    public ConcurrentQueue<BlobRequest> Requests { get; } = new();
    public bool ContainerExists { get; set; }
    public bool RejectAcl { get; set; }
    public string ConnectionString { get; private set; } = "";

    private BlobEndpoint(WebApplication app) => this.app = app;

    public static async Task<BlobEndpoint> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        var app = builder.Build();
        var endpoint = new BlobEndpoint(app);
        app.Run(endpoint.HandleAsync);
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        // Synthetic key used only to sign requests to this in-process fixture.
        var key = Convert.ToBase64String(new byte[32]);
        endpoint.ConnectionString = $"DefaultEndpointsProtocol=http;AccountName=demotest;AccountKey={key};BlobEndpoint={address}/demotest";
        return endpoint;
    }

    private async Task HandleAsync(HttpContext context)
    {
        using var body = new MemoryStream();
        await context.Request.Body.CopyToAsync(body);
        Requests.Enqueue(new BlobRequest(context.Request.Path.Value!,
            context.Request.QueryString.Value ?? "", context.Request.Method,
            context.Request.Headers.ToDictionary(entry => entry.Key,
                entry => entry.Value.ToString(), StringComparer.OrdinalIgnoreCase), body.ToArray()));
        var acl = context.Request.Query["comp"] == "acl";
        var create = context.Request.Query["restype"] == "container" && !acl;
        context.Response.Headers["ETag"] = "\"fixture-etag\"";
        context.Response.Headers["Last-Modified"] = "Mon, 12 Oct 2026 00:00:00 GMT";
        context.Response.Headers["x-ms-request-id"] = "fixture-request";
        if (acl && RejectAcl || create && ContainerExists)
        {
            var code = acl ? "AuthorizationFailure" : "ContainerAlreadyExists";
            context.Response.StatusCode = acl ? 403 : 409;
            context.Response.Headers["x-ms-error-code"] = code;
            context.Response.ContentType = "application/xml";
            await context.Response.WriteAsync($"<Error><Code>{code}</Code><Message>Fixture response</Message></Error>");
            return;
        }
        context.Response.StatusCode = acl ? 200 : 201;
        context.Response.ContentLength = 0;
    }

    public async ValueTask DisposeAsync()
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }
}

internal sealed record BlobRequest(string Path, string Query, string Method,
    Dictionary<string, string> Headers, byte[] Body);
