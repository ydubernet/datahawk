using amazon_scraper.Models;
using amazon_scraper.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IReviewIndexerService _reviewIndexerService;

        public WebScraperController(
            IScrapingService scrapingService,
            IReviewIndexerService reviewIndexerService,
            ILogger<WebScraperController> logger)
        {
            _logger = logger;
            _scrapingService = scrapingService;
            _reviewIndexerService = reviewIndexerService;
        }

        /// <summary>
        /// Retrieves product reviews for a given productId
        /// </summary>
        /// <param name="productId">Amazon product id, aka ASIN</param>
        /// <returns></returns>
        [HttpGet("{productId}")]
        public async Task<string> GetAsync(string productId, int sortByRecent)
        {
            var results = new List<Review>();
            //B082XY23D5 or B084M1M1DZ
            await _scrapingService.GetPageData(WEB_SITE_REVIEW_URL + productId, sortByRecent, results);

            foreach (var res in results)
            {
                _logger.LogInformation("Asin: " + res.Asin);
                _logger.LogInformation("Rating: " + res.Rating);
                _logger.LogInformation("ReviewDate: " + res.Date);
                _logger.LogInformation("ReviewTitle: " + res.Title);
                _logger.LogInformation("ReviewContent: " + res.Content);
            }

            if (results.Count() == 0)
            {
                HttpContext.Response.StatusCode = 204;
                HttpContext.Response.Redirect($"{productId}/existing");
                return string.Empty;
            }

            return results.Select(r => r.Content).Aggregate((r, s) => (r + Environment.NewLine + Environment.NewLine + s));
        }

        [HttpGet("{productId}/existing")]
        public async Task<string> GetWithoutScrapping(string productId)
        {
            var results = await _reviewIndexerService.GetReviewsAsync(productId);

            // Yes, I know I shouldn't do that. It should be in a front.
            // So I'll return a string so that human eye may quickly get the final output idea.

            var resultBuilder = new StringBuilder();
            foreach (var result in results)
            {
                resultBuilder
                    .Append(result.Title)
                    .Append("\t")
                    .Append(result.Rating)
                    .Append("\t")
                    .Append(result.Date)
                    .Append("\t\n")
                    .Append(result.Content)
                    .Append("\n\n");
            }

            return resultBuilder.ToString();
        }

        /// <summary>
        /// Advanced feature : retrieve as many URLs as you want within one single call.
        /// Because it can be quite heavy, we will pass this as a POST request
        /// </summary>
        /// <remarks>
        /// Should receive an application/json data format,
        /// itself being an array (for instance ["B082XY23D5", "B082XY23D5"])
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> PostManyAsync()
        {
            string body;
            using (var streamReader = new StreamReader(HttpContext.Request.Body))
            {
                body = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }

            var productIds = Jil.JSON.Deserialize<string[]>(body);

            StringBuilder globalResult = new StringBuilder();
            foreach (var productId in productIds)
            {
                var results = new List<Review>();
                //B082XY23D5 or B082XY23D5
                await _scrapingService.GetPageData(WEB_SITE_REVIEW_URL + productId, 1, results);

                if (results.Count == 0)
                {
                    _logger.LogInformation($"No new review for asin {productId}");
                }

                foreach (var res in results)
                {
                    _logger.LogInformation("Asin: " + res.Asin);
                    _logger.LogInformation("Rating: " + res.Rating);
                    _logger.LogInformation("ReviewDate: " + res.Date);
                    _logger.LogInformation("ReviewTitle: " + res.Title);
                    _logger.LogInformation("ReviewContent: " + res.Content);
                }

                globalResult.Append($"{productId} {Environment.NewLine}");
            }

            return globalResult.ToString();
        }
    }
}
