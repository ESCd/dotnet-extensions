using Microsoft.IO;

namespace ESCd.Extensions.Http;

/// <summary> Extensions to <see cref="HttpClient"/>. </summary>
public static class HttpClientExtensions
{
    private const string ContentStreamTag = "ESCd.Extensions.Http.RecyclableStream";

    /// <summary> Send a GET request to the specified Uri and return the response body as a recyclable stream. </summary>
    /// <param name="http"> The <see cref="HttpClient"/> to make the request with. </param>
    /// <param name="url"> The url to be requested. </param>
    /// <param name="streamManager"> The <see cref="RecyclableMemoryStreamManager"/> to retrieve a recyclable stream from. </param>
    /// <param name="cancellation"> A token to trigger cancellation of the operation. </param>
    public static async Task<Stream> GetStreamAsync( this HttpClient http, Uri url, RecyclableMemoryStreamManager streamManager, CancellationToken cancellation = default )
    {
        ArgumentNullException.ThrowIfNull( http );
        ArgumentNullException.ThrowIfNull( streamManager );

        using var response = await http.GetAsync( url, cancellation ).ConfigureAwait( false )!;

        var length = response.EnsureSuccessStatusCode().Content.Headers.ContentLength;
        var stream = length.HasValue ? streamManager.GetStream( ContentStreamTag, length.Value ) : streamManager.GetStream( ContentStreamTag );

        try
        {
            await response.Content.CopyToAsync( stream, cancellation ).ConfigureAwait( false );

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