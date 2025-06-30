using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace saehyeon_mc_env
{
    internal class Downloader
    {
        public static async Task DownloadFile(
            string url,
            string dest,
            CancellationToken cancellationToken = default)
        {
            Logger.Info($"{Constants.Messages.DOWNLOADING} \"{url}\" -> \"{dest}\"");

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.SystemDefault;

            var directory = Path.GetDirectoryName(dest);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent
                          .ParseAdd($"{Config.AppName}/{Config.AppVersion} (+{Config.RepoUrl})");

                using (var response = await httpClient
                       .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                       .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    using (var contentStream = await response.Content
                           .ReadAsStreamAsync().ConfigureAwait(false))

                    using (var fileStream = new FileStream(
                           dest,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None,
                           bufferSize: 8 * 1024,
                           useAsync: true))
                    {
                        await contentStream.CopyToAsync(fileStream, 8 * 1024, cancellationToken)
                                           .ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
