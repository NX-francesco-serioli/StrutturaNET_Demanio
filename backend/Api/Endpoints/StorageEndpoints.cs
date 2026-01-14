using AdSPMdS.DemanioDigitale.Api.Storage;
using Microsoft.AspNetCore.Mvc;

namespace AdSPMdS.DemanioDigitale.Api.Endpoints;

public static class StorageEndpoints
{
    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/storage/upload", async (
            [FromForm] StorageUploadRequest request,
            [FromQuery] string? prefix,
            BlobStorageService storage,
            CancellationToken cancellationToken) =>
        {
            if (request.File is null || request.File.Length == 0)
            {
                return Results.BadRequest("File mancante o vuoto.");
            }

            var resolvedPrefix = string.IsNullOrWhiteSpace(prefix)
                ? request.Prefix
                : prefix;
            var result = await storage.UploadAsync(request.File, resolvedPrefix, cancellationToken);
            return Results.Ok(result);
        })
        .Accepts<StorageUploadRequest>("multipart/form-data")
        .DisableAntiforgery()
        .RequireAuthorization();

        routes.MapGet("/api/storage/search", async (
            [FromQuery] string? prefix,
            BlobStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var items = await storage.ListAsync(prefix, cancellationToken);
            return Results.Ok(items);
        }).RequireAuthorization();

        routes.MapGet("/api/storage/download/{**blobName}", async (
            string blobName,
            BlobStorageService storage,
            CancellationToken cancellationToken) =>
        {
            var result = await storage.DownloadAsync(blobName, cancellationToken);
            if (result is null)
            {
                return Results.NotFound();
            }

            return Results.File(result.Stream, result.ContentType, result.FileName);
        }).RequireAuthorization();

        return routes;
    }
}

public class StorageUploadRequest
{
    public IFormFile File { get; set; } = default!;
    public string? Prefix { get; set; }
}
