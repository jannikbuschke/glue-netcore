using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Glow.Files;
using Glow.Glue.AspNetCore.Tests;
using Glow.Tests;

namespace Glow.Core.Files
{
    public static class HttpClientExtensions
    {
        public static async Task<IEnumerable<T>> UploadFiles<T>(this HttpClient client, IEnumerable<T> files, string actionUrl) where T : IFile
        {
            var faker = new Faker();
            var formContent = new MultipartFormDataContent { };
            var streams = files.ToDictionary(v => v, v => new MemoryStream());
            foreach (T v in files)
            {
                MemoryStream ms = streams[v];
                TextWriter tw = new StreamWriter(ms);
                tw.Write(faker.Lorem.Paragraph());
                tw.Flush();
                ms.Position = 0;
                formContent.Add(new StreamContent(ms), Guid.NewGuid().ToString(), v.Name);
            }

            HttpResponseMessage response = await client.PostAsync(actionUrl, formContent);
            foreach (KeyValuePair<T, MemoryStream> item in streams)
            {
                item.Value.Dispose();
            }

            response.EnsureSuccessStatusCode();

            return await response.ReadAsAsync<IEnumerable<T>>();
        }
    }
}
