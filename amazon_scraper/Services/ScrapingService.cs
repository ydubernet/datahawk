using AngleSharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amazon_scraper.Services
{
    public class ScrapingService : IScrapingService
    {
        private const string WEB_SITE_URL = "https://www.amazon.com";
        private readonly ILogger<ScrapingService> _logger;

        public ScrapingService()
        {
            //_logger = logger;
        }

        public async Task<List<string>> GetPageData(string url, List<string> results)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url);

            // Debug
            //_logger.LogInformation(document.DocumentElement.OuterHtml);

            var reviews = document.QuerySelectorAll(".review");

            foreach (var review in reviews)
            {
                //// Create a container object
                //CarAdvert advert = new CarAdvert();

                //// Use regex to get all the numbers from this string
                //MatchCollection regxMatches = Regex.Matches(row.QuerySelector(".price").TextContent, @"\d+\.*\d+");
                //uint.TryParse(string.Join("", regxMatches), out uint price);
                //advert.Price = price;

                //regxMatches = Regex.Matches(row.QuerySelector(".year").TextContent, @"\d+");
                //uint.TryParse(string.Join("", regxMatches), out uint year);
                //advert.Year = year;

                //// Get the fuel type from the ad
                //advert.Fuel = row.QuerySelector(".fuel").TextContent[0];

                //// Make and model
                //advert.MakeAndModel = row.QuerySelector(".make_and_model > a").TextContent;

                //// Link to the advert
                //advert.AdvertUrl = WEB_SITE_URL + row.QuerySelector(".make_and_model > a").GetAttribute("Href");

                //_logger.LogInformation(review.TextContent);
                results.Add(review.TextContent);
            }

            // Check if a next page link is present
            string nextPageUrl = "";
            var nextPageLink = document.QuerySelector(".a-pagination > li.a-last > a");
            if (nextPageLink != null)
            {
                nextPageUrl = WEB_SITE_URL + nextPageLink.GetAttribute("href");
            }

            // If next page link is present recursively call the function again with the new url
            if (!String.IsNullOrEmpty(nextPageUrl))
            {
                return await GetPageData(nextPageUrl, results);
            }

            return results;
        }

        private async void CheckForUpdates(string url, string mailTitle)
        {
            // We create the container for the data we want
            List<string> adverts = new List<string>();

            /**
             * GetPageData will recursively fill the container with data
             * and the await keyword guarantees that nothing else is done
             * before that operation is complete.
             */
            await GetPageData(url, adverts);

            // TODO: Diff the data
        }
    }
}
