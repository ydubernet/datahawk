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
using System.Text;
using System.IO;

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

            foreach(var res in results)
            {
                _logger.LogInformation("Asin: " + res.Asin);
                _logger.LogInformation("Rating: " + res.Rating);
                _logger.LogInformation("ReviewDate: " + res.Date);
                _logger.LogInformation("ReviewTitle: " + res.Title);
                _logger.LogInformation("ReviewContent: " + res.Content);
            }

            return results.Select(r => r.Content).Aggregate((r, s) => (r + Environment.NewLine + Environment.NewLine + s));
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

            var productIds =  Jil.JSON.Deserialize<string[]>(body);

            StringBuilder globalResult = new StringBuilder();
            foreach(var productId in productIds)
            {
                var results = new List<Review>();
                //B082XY23D5 or B082XY23D5
                await _scrapingService.GetPageData(WEB_SITE_REVIEW_URL + productId, 1, results);

                foreach (var res in results)
                {
                    _logger.LogInformation("Asin: " + res.Asin);
                    _logger.LogInformation("Rating: " + res.Rating);
                    _logger.LogInformation("ReviewDate: " + res.Date);
                    _logger.LogInformation("ReviewTitle: " + res.Title);
                    _logger.LogInformation("ReviewContent: " + res.Content);
                }

                globalResult.Append($"RESULTS FOR PRODUCT {productId}: + {Environment.NewLine + results.Select(r => r.Content).Aggregate((r, s) => r + Environment.NewLine + Environment.NewLine + s) + Environment.NewLine}");
            }

            return globalResult.ToString();
        }
    }
}
