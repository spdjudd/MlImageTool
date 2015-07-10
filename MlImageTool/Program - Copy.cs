using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using Bing;

namespace TestBingSearch
{
    class Program
    {
        /// <summary>
        /// Next: 
        /// download images to file
        /// name according to bird type 
        /// delete bad images in explorer
        /// crop
        /// resize
        /// flip
        /// 
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var searches = new NameValueCollection
            {
                {"bluetit", "bluetit"},
                {"coaltit", "coaltit"},
                {"greattit", "greattit"},
                {"longtailedtit", "longtailed tit"},
                {"greenfinch", "greenfinch"},
                {"goldfinch", "european goldfinch"},
                {"chaffinch", "chaffinch"},
                {"blackbirdm", "turdus merula male"},
                {"blackbirdf", "turdus merula female"},
                {"thrush", "song thrush"},
                {"robin", "european robin"},
                {"dunnock", "dunnock"},
                {"nuthatch", "eurasian nuthatch"},
                {"greaterspottedwoodpecker", "greater spotted woodpecker"},
                {"greenwoodpecker", "green woodpecker"},
                {"woodpigeon", "columba palumbus"},
                {"collareddove", "collared dove"},
                {"wren", "eurasian wren"},
                {"magpie", "eurasian magpie"},
                {"starling", "european starling"},
            };
            const string apiKey = "px10eK/V0YPxELtEk4t8CIesrDR90AkoauBbx+lh/s4";
            var searchContainer = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/Search"))
            {
                Credentials = new NetworkCredential(apiKey, apiKey)
            };
            var webClient = new WebClient();
            while (true)
            {
                Console.WriteLine("Enter search term:");
                var searchTerm = Console.ReadLine();
                if (string.IsNullOrEmpty(searchTerm))
                    break;
                Console.WriteLine("Enter file prefix:");
                var filePrefix = Console.ReadLine();
                if (string.IsNullOrEmpty(filePrefix))
                    filePrefix = searchTerm;
                var query = searchContainer.Image(searchTerm, null, "en-GB", null, null, null, null);
                query.AddQueryOption("$top", 20);
                Console.WriteLine("Starting search");
                var results = query.Execute();
                if (results == null)
                {
                    Console.WriteLine("results == null");
                    Console.ReadKey();
                    break;
                }
                Console.WriteLine("Top 20 results:");
                if (Directory.Exists(filePrefix))
                {
                    Directory.Move(filePrefix, string.Format("{0}-{1}", filePrefix, DateTime.Now.ToString("yyyyMMddTHHmmssfff")));
                }
                Directory.CreateDirectory(filePrefix);
                var i = 1;
                foreach (var result in results.Take(20))
                {
                    Console.WriteLine("{0} - {1} [{2}x{3}]", result.Title, result.MediaUrl, result.Height, result.Width);
                    var ext = Path.GetExtension(result.MediaUrl);
                    try
                    {
                        webClient.DownloadFile(result.MediaUrl, Path.Combine(filePrefix, string.Format("{0}-{1}{2}", filePrefix, i++, ext)));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception downloading: " + ex.Message);
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
