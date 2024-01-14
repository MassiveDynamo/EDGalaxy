using Serilog.Core;
using System.Net.Http;

internal class Downloader
{
    internal static void DownloadFile(Logger logger, string? url, string? fileName)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(fileName);

        // Check for url
        var client = new HttpClient();
        using HttpRequestMessage request = new(HttpMethod.Head, url);

        using HttpResponseMessage response = client.Send(request);
        response.EnsureSuccessStatusCode();
        logger.Information("StatucCode: {statusCode}. Headers.Etag.Tag:{etag}", response.StatusCode, response.Headers.ETag.Tag);
            
        if (File.Exists(fileName)) { return; }
        var fi = new FileInfo(fileName);
        if (!fi.Directory.Exists)
        {
            logger.Information("Creating folder {folder}", fi.Directory.FullName);
            Directory.CreateDirectory(fi.DirectoryName);
        }

    }
}