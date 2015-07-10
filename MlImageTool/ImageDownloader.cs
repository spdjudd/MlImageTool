using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Bing;

namespace MlImageTool
{
    class ImageDownloader
    {
        // api key from Bing datamarket
        private const string ApiKey = "px10eK/V0YPxELtEk4t8CIesrDR90AkoauBbx+lh/s4";
        // results per search - max is 50
        private const int PageSize = 50;

        private readonly WebClient _webClient;
        private readonly BingSearchContainer _searchContainer;

        public ImageDownloader()
        {
            _webClient = new WebClient();
            _searchContainer = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/Search"))
            {
                Credentials = new NetworkCredential(ApiKey, ApiKey)
            };
        }

        public void Download(string category, string terms, string destination, int searchCount)
        {
            var i = 0;
            var pageNumber = 0;
            var processed = new HashSet<string>();

            while (i < searchCount)
            {
                var query = _searchContainer.Image(terms, null, "en-GB", null, null, null, null);
                // always search for a full page
                query = query.AddQueryOption("$top", PageSize);
                if (pageNumber > 0)
                    // skip previous pages 
                    query = query.AddQueryOption("$skip", pageNumber * PageSize);

                Console.WriteLine("Starting search for " + terms);
                Console.WriteLine("Url: " + query.RequestUri);

                // run the query
                var results = query.Execute();
                if (results == null)
                {
                    Console.WriteLine("results == null");
                    continue;
                }

                // process results
                Console.WriteLine("Results:");
                foreach (var result in results)
                {
                    if (processed.Contains(result.Thumbnail.MediaUrl))
                    {
                        // skip duplicates
                        // https://social.msdn.microsoft.com/Forums/sqlserver/en-US/170b5fd5-cf3e-492c-a202-c460ced4d4c5/overlaping-results-on-image-search-api?forum=DataMarket
                        continue;
                    }
                    processed.Add(result.Thumbnail.MediaUrl);
                    Console.WriteLine("{0} - {1} [{2}x{3}]", result.Title, result.Thumbnail.MediaUrl, result.Thumbnail.Height, result.Thumbnail.Width);
                    try
                    {
                        var path = string.Format("{0}-{1}.png", category, ++i);
                        _webClient.DownloadFile(result.Thumbnail.MediaUrl, Path.Combine(destination, path));
                        if (i >= searchCount)
                            return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception downloading: " + ex.Message);
                    }
                }
                pageNumber++;
            }
            Console.WriteLine();
        }
    }
}
