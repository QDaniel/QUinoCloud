using Microsoft.Net.Http.Headers;
using QUinoCloud.Utils.Extensions;
namespace QUinoCloud.Classes
{
    public class MediaDownloader
    {
        public static string DefaultUserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
        private readonly HttpClient _httpClient;
        public MediaDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, DefaultUserAgent);
        }
        public async Task<bool> CanDownloadAsync(string url, CancellationToken token = default)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Head, url);
            using var ret = await _httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, token);
            return ret.IsSuccessStatusCode && ret.Content.Headers.ContentLength > 0;
        }
        public async Task<Stream?> DownloadAsync(string url, CancellationToken token = default)
        {
            if (!await CanDownloadAsync(url, token)) return null;

            using var msg = new HttpRequestMessage(HttpMethod.Get, url);
            using var ret = await _httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, token);
            ret.EnsureSuccessStatusCode();
            var cs = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            return await Task.Run(() => ret.Content.ReadAsStream().ToTempStream(), cs.Token);
        }
    }
}

