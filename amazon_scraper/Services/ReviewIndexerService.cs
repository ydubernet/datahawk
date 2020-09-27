using amazon_scraper.Databases;
using amazon_scraper.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, IList<string>> _reviewsIdsCache;

        public ReviewIndexerService(ILogger<ReviewIndexerService> logger, IReviewRepository reviewRepository)
        {
            _logger = logger;
            _reviewRepository = reviewRepository;
            _reviewsIdsCache = new ConcurrentDictionary<string, IList<string>>();
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
            if (_reviewsIdsCache.ContainsKey(review.Asin))
            {
                _reviewsIdsCache[review.Asin].Add(review.ReviewId);
            }
            else
            {
                _reviewsIdsCache.TryAdd(review.Asin, new List<string>() { review.ReviewId });
            }

            return _reviewRepository.InsertReviewAsync(review);
        }

        public Task<bool> InsertReviewsForOneAsinAsync(IList<Review> reviews)
        {
            if (reviews.Count == 0)
            {
                return Task.FromResult(true);
            }

            _logger.LogInformation($"Inserting reviews for asin {reviews[0].Asin}");
            _reviewsIdsCache.TryAdd(reviews[0].Asin, reviews.Select(r => r.ReviewId).ToList()); // to check
            return _reviewRepository.InsertReviewsForOneAsinAsync(reviews);
        }

        public async Task<IList<string>> GetAllReviewsIdsAsync(string asin)
        {
            if (_reviewsIdsCache.ContainsKey(asin))
            {
                return _reviewsIdsCache[asin];
            }

            _logger.LogInformation("Retrieving all review ids");
            var reviews = await _reviewRepository.GetAllReviewsIdsAsync(asin);
            _reviewsIdsCache.TryAdd(asin, reviews);
            return reviews;
        }
    }
}
