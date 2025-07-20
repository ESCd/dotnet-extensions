using Microsoft.IO;

namespace ESCd.Extensions.Http;

/// <summary> Extensions to <see cref="HttpContent"/>. </summary>
public static class HttpContentExtensions
{
    private const string ContentStreamTag = "ESCd.Extensions.Http.RecyclableStream";

    /// <summary> Read HTTP Content, and return the response body as a recyclable stream. </summary>
    /// <param name="content"> The <see cref="HttpContent"/> to read. </param>
    /// <param name="streamManager"> The <see cref="RecyclableMemoryStreamManager"/> to retrieve a recyclable stream from. </param>
    /// <param name="cancellation"> A token to trigger cancellation of the operation. </param>
    public static async Task<Stream> ReadAsStreamAsync( this HttpContent content, RecyclableMemoryStreamManager streamManager, CancellationToken cancellation = default )
    {
        ArgumentNullException.ThrowIfNull( content );
        ArgumentNullException.ThrowIfNull( streamManager );

        var length = content.Headers.ContentLength;
        var stream = length.HasValue ? streamManager.GetStream( ContentStreamTag, length.Value ) : streamManager.GetStream( ContentStreamTag );

        try
        {
            await content.CopyToAsync( stream, cancellation ).ConfigureAwait( false );

            stream.Seek( 0, SeekOrigin.Begin );
            return stream;
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait( false );
            throw;
        }
    }
}