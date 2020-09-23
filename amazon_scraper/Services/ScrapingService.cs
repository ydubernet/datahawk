using amazon_scraper.Models;
using AngleSharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace amazon_scraper.Services
{
    public class ScrapingService : IScrapingService
    {
        private const string WEB_SITE_URL = "https://www.amazon.com/product-reviews/";
        private readonly ILogger<ScrapingService> _logger;

        public ScrapingService()
        {
            //_logger = logger;
        }

        public async Task<List<Review>> GetPageData(string asin, List<Review> results)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var url = WEB_SITE_URL + asin;
            var document = await context.OpenAsync(url);

            // Debug
            //_logger.LogInformation(document.DocumentElement.OuterHtml);

            var rawReviews = document.QuerySelectorAll(".review");

            foreach (var rawReview in rawReviews)
            {
                //string divId = rawReview.GetAttribute("id");
                string reviewTitle = rawReview.QuerySelector(".review-title > span").TextContent;
                string reviewDateRow = rawReview.QuerySelector(".review-date").TextContent;
                //var dateRegexMatch = Regex.Match(reviewDateRow, "on (?<Date>[A-Z]{1}[a-z]+, [19|20][0-9]{2}$)");
                var dateRegexMatch = Regex.Match(reviewDateRow, "on (?<Date>[A-Z][a-z]+ [0-9]{1,2}, (19|20)[0-9]{2}$)");

                DateTime reviewDate = DateTime.MinValue;
                if(dateRegexMatch.Success)
                {
                    DateTime.TryParse(dateRegexMatch.Groups["Date"].Value, out reviewDate);   
                }

                string reviewContent = rawReview.FirstElementChild.FirstChild.ChildNodes[4].FirstChild.TextContent.Trim();//rawReview.QuerySelector($"{divId}-review-card > customer_review-{divId} > div.review-data > .review-body > span").TextContent;
                string reviewRatingRow = rawReview.FirstElementChild.FirstChild.ChildNodes[1].FirstChild.TextContent.Trim(); //rawReview.QuerySelector(".review-start-rating > span").TextContent;
                var ratingRegexMatch = Regex.Match(reviewRatingRow, "^(?<ReviewRating>[0-9].[0-9]) out of 5 stars$");

                double reviewRating = -1.0;
                if(ratingRegexMatch.Success)
                {
                    double.TryParse(ratingRegexMatch.Groups["ReviewRating"].Value, out reviewRating);
                }

                results.Add(new Review(asin, reviewDate, reviewTitle, reviewContent, reviewRating));
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
    }
}
