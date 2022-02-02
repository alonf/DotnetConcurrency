using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AsyncAwaitDemo
{
    class Program
    {
        static void Main()
        {
            Task t1 = WithoutAsync();
            Task t2 = WithAsync();
            Task.WaitAll(t1, t2);
        }

        private static void StartProcess(string processExePath)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(processExePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private static Task WithoutAsync()
        {
            var httpClient = new HttpClient();

            var task = httpClient.GetAsync("https://twitter.com/hashtag/csharp")
                .ContinueWith(requestTask =>
                {
                    var httpContent = requestTask.Result.Content;
                    httpContent.ReadAsStringAsync()
                                    .ContinueWith(contentTask =>
                                    {
                                        var fileName = Path.ChangeExtension(Path.GetTempFileName(), ".html");
                                        var file = File.CreateText(fileName);
                                        file.WriteAsync(contentTask.Result).ContinueWith(f =>
                                        {
                                            f.Dispose();
                                            StartProcess(fileName);
                                        });
                                    });
                });
            return task;

        }
        private static async Task WithAsync()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://twitter.com/hashtag/dotnet");
            var page = await response.Content.ReadAsStringAsync();
            var fileName = Path.ChangeExtension(Path.GetTempFileName(), ".html");
            using (var file = File.CreateText(fileName))
            {
                await file.WriteAsync(page);
            }
            StartProcess(fileName);
        }
    }
}
