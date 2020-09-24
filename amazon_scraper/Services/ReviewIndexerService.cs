using amazon_scraper.Databases;
using amazon_scraper.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amazon_scraper.Services
{
    /// <summary>
    /// Service to retrieve or insert reviews
    /// </summary>
    public class ReviewIndexerService : IReviewIndexerService
    {
        private readonly ILogger<ReviewIndexerService> _logger;
        private readonly IReviewRepository _reviewRepository;

        public ReviewIndexerService(ILogger<ReviewIndexerService> logger, IReviewRepository reviewRepository)
        {
            _logger = logger;
            _reviewRepository = reviewRepository;
        }

        public Task<IList<Review>> GetReviewsAsync(string asin)
        {
            _logger.LogInformation($"Retrieving all reviews for asin {asin}");
            return _reviewRepository.GetReviewsAsync(asin);
        }

        public Task<IList<Review>> GetReviewsForManyAsinsAsync(IList<string> asins)
        {
            _logger.LogInformation($"Retrieving all reviews for asins {asins.Aggregate((r, s) => string.Join(",", r, s))}");
            return _reviewRepository.GetReviewsForManyAsinsAsync(asins);
        }


        public Task<IList<Review>> GetLastReviewsForManyAsinsAsync(IList<string> asins, int numberOfReviewsByAsin)
        {
            _logger.LogInformation($"Retrieving last {numberOfReviewsByAsin} reviews for asins {asins.Aggregate((r, s) => string.Join(",", r, s))}");
            return _reviewRepository.GetLastReviewsForManyAsinsAsync(asins, numberOfReviewsByAsin);
        }


        public Task<bool> InsertReviewAsync(Review review)
        {
            _logger.LogInformation($"Inserting a new review for asin {review.Asin}");
            return _reviewRepository.InsertReviewAsync(review);
        }

        public Task<bool> InsertReviewsForOneAsinAsync(IList<Review> reviews)
        {
            _logger.LogInformation($"Inserting reviews for asin {reviews[0].Asin}");
            return _reviewRepository.InsertReviewsForOneAsinAsync(reviews);
        }
    }
}
