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

namespace amazon_scrapper.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebScraperController : ControllerBase
    {
        private readonly ILogger<WebScraperController> _logger;
        private readonly IScrapingService _scrapingService;
        
        public WebScraperController(
            IScrapingService scrapingService,
            ILogger<WebScraperController> logger)
        {
            _logger = logger;
            _scrapingService = scrapingService;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            //// Load default configuration
            //   var config = Configuration.Default.WithDefaultLoader();
            //   // Create a new browsing context
            //   var context = BrowsingContext.New(config);
            //   // This is where the HTTP request happens, returns <IDocument> that // we can query later
            //   var document = await context.OpenAsync("https://www.amazon.com/product-reviews/B082XY23D5");
            //   // Log the data to the console
            //   _logger.LogInformation(document.DocumentElement.OuterHtml);

            //   return document.DocumentElement.OuterHtml;
            var results = new List<string>();
            await _scrapingService.GetPageData("https://www.amazon.com/product-reviews/B082XY23D5", results);

            foreach(var res in results)
            {
                _logger.LogInformation((string)res);
            }
           
            return results.ToString();
        }
    }
}
