using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AngleSharp;
using AngleSharp.Html.Parser;
using amazon_scraper.Services;
using amazon_scraper.Models;

namespace amazon_scrapper.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebScraperController : ControllerBase
    {
        private const string WEB_SITE_URL = "https://www.amazon.com";
        private const string WEB_SITE_REVIEW_URL = WEB_SITE_URL + "/product-reviews/";
        private readonly ILogger<WebScraperController> _logger;
        private readonly IScrapingService _scrapingService;
        
        public WebScraperController(
            IScrapingService scrapingService,
            ILogger<WebScraperController> logger)
        {
            _logger = logger;
            _scrapingService = scrapingService;
        }

        [HttpGet("{productId}")]
        public async Task<string> Get(string productId)
        {
            //// Load default configuration
            //   var config = Configuration.Default.WithDefaultLoader();
            //   // Create a new browsing context
            //   var context = BrowsingContext.New(config);
            //   // This is where the HTTP request happens, returns <IDocument> that // we can query later
            //   var document = await context.OpenAsync($"https://www.amazon.com/product-reviews/{productId}");
            //   // Log the data to the console
            //   _logger.LogInformation(document.DocumentElement.OuterHtml);

            //   return document.DocumentElement.OuterHtml;
            var results = new List<Review>();
            //B082XY23D5 or B084M1M1DZ
            await _scrapingService.GetPageData(WEB_SITE_REVIEW_URL + productId, results);

            foreach(var res in results)
            {
                _logger.LogInformation("Asin: " + res.Asin);
                _logger.LogInformation("Rating: " + res.Rating);
                _logger.LogInformation("ReviewDate: " + res.ReviewDate);
                _logger.LogInformation("ReviewTitle: " + res.ReviewTitle);
                _logger.LogInformation("ReviewContent: " + res.ReviewContent);
            }

            return results.Select(r => r.ReviewContent).Aggregate((r, s) => (r + Environment.NewLine + Environment.NewLine + s));
        }
    }
}
