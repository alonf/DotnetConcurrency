using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cancellation
{
    class Program
    {
        private static string Root { get; }

        static Program()
        {
            Root = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
        }

        static void Main()
        {
            var result = FindTextFilesWithUrls(10);
            foreach (var file in result)
            {
                Console.WriteLine("URLs found in {0} are: ", file.Key);
                file.Value.ToList().ForEach(Console.WriteLine);
            }
        }

        private static IEnumerable<KeyValuePair<string, IList<string>>> FindTextFilesWithUrls(int nFiles)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var result = new ConcurrentDictionary<string, IList<string>>();
            var directoryStack = new Stack<string>();

            directoryStack.Push(Root);

            while (directoryStack.Count > 0)
            {
                string currentDirectory = directoryStack.Pop();

                try
                {
                    var parallelOption = new ParallelOptions { CancellationToken = cancellationTokenSource.Token };

                    Parallel.ForEach(Directory.EnumerateFiles(currentDirectory), parallelOption,
                                     file =>
                                     {
                                         var urls = FindUrlInFile(file);
                                         if (urls.Count > 0)
                                         {
                                             result[file] = urls;
                                         }
                                         if (result.Keys.Count > nFiles)
                                             cancellationTokenSource.Cancel();
                                     });
                    if (cancellationTokenSource.IsCancellationRequested)
                        return result;

                    foreach (var directory in Directory.EnumerateDirectories(currentDirectory))
                    {
                        directoryStack.Push(directory);
                    }
                }
                catch (OperationCanceledException)
                {
                    return result;
                }
                catch
                {
                    //ignore Unauthorized access directories
                }
            }
            return result;
        }

        private static IList<string> AllURLS(string txt)
        {
            var regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            return regx.Matches(txt).Cast<Match>().Select(m => m.Value).ToList();
        }


        private static IList<string> FindUrlInFile(string file)
        {
            var result = new List<string>();
            var extension = Path.GetExtension(file);
            if (string.IsNullOrWhiteSpace(extension))
                return result;

            // ReSharper disable PossibleNullReferenceException
            extension = extension.ToUpper();
            // ReSharper restore PossibleNullReferenceException

            if (extension != ".TXT" && extension != ".HTM" && extension != ".HTML" && extension != ".XML")
                return result;

            try
            {
                var text = File.ReadAllText(file);
                return AllURLS(text);

            }
            catch (Exception)
            {

                return new List<string>();
            }
        }
    }
}
