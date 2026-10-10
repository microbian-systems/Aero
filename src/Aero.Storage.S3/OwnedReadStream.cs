using Amazon.S3;
using Amazon.Runtime;

namespace Aero.Storage.S3;

// Owning adapter/decorator: closes the SDK response exactly once and detects transfer-length failures.
internal sealed class OwnedReadStream(Stream inner, long length, Action release) : Stream
{
    private long position;
    private bool disposed;
    private int released;
    public override bool CanRead => !disposed && inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => length;
    public override long Position { get => position; set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public override int Read(Span<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (buffer.Length == 0) return 0;
        try
        {
            if (position == length)
            {
                if (inner.ReadByte() != -1) throw new ObjectStorageException(StorageError.LengthMismatch);
                return 0;
            }
            int read = inner.Read(buffer[..(int)Math.Min(buffer.Length, length - position)]);
            if (read == 0) throw new ObjectStorageException(StorageError.LengthMismatch);
            position += read;
            return read;
        }
        catch (AmazonS3Exception error) { throw S3Errors.Map(error); }
        catch (AmazonClientException) { throw new ObjectStorageException(StorageError.Unavailable); }
        catch (HttpRequestException) { throw new ObjectStorageException(StorageError.Unavailable); }
        catch (IOException) { throw new ObjectStorageException(StorageError.Unavailable); }
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        if (buffer.Length == 0) return 0;
        try
        {
            if (position == length)
            {
                if (await inner.ReadAsync(new byte[1], cancellationToken) != 0)
                    throw new ObjectStorageException(StorageError.LengthMismatch);
                return 0;
            }
            int read = await inner.ReadAsync(buffer[..(int)Math.Min(buffer.Length, length - position)], cancellationToken);
            if (read == 0) throw new ObjectStorageException(StorageError.LengthMismatch);
            position += read;
            return read;
        }
        catch (AmazonS3Exception error) { cancellationToken.ThrowIfCancellationRequested(); throw S3Errors.Map(error); }
        catch (AmazonClientException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (HttpRequestException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (IOException) { cancellationToken.ThrowIfCancellationRequested(); throw new ObjectStorageException(StorageError.Unavailable); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new ObjectStorageException(StorageError.Unavailable); }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && Interlocked.Exchange(ref released, 1) == 0)
        {
            disposed = true;
            release();
        }
        base.Dispose(disposing);
    }
    public override ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
