using System.Net;
using Amazon.S3;

namespace Aero.Storage.S3;

internal static class S3Errors
{
    internal static bool Missing(AmazonS3Exception error) => error.StatusCode == HttpStatusCode.NotFound
        && error.ErrorCode != "NoSuchBucket";
    internal static ObjectStorageException Map(AmazonS3Exception error) => new(error.StatusCode switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => StorageError.AccessDenied,
        HttpStatusCode.PreconditionFailed => StorageError.AlreadyExists,
        HttpStatusCode.Conflict => StorageError.Conflict,
        HttpStatusCode.TooManyRequests => StorageError.Throttled,
        HttpStatusCode.ServiceUnavailable when error.ErrorCode == "SlowDown" => StorageError.Throttled,
        _ when (int)error.StatusCode >= 500 => StorageError.Unavailable,
        _ when error.ErrorCode == "NoSuchBucket" => StorageError.Configuration,
        _ => StorageError.InvalidResponse
    });
}
