using amazon_scraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amazon_scraper.Services
{
    public interface IReviewIndexerService
    {
        Task<IList<Review>> GetReviewsAsync(string asin);
        Task<IList<Review>> GetReviewsForManyAsinsAsync(IList<string> asins);
        Task<IList<Review>> GetLastReviewsForManyAsinsAsync(IList<string> asins, int numberOfReviewsByAsin);

        Task<bool> InsertReviewAsync(Review review);
        Task<bool> InsertReviewsForOneAsinAsync(IList<Review> reviews);
        Task<IList<string>> GetAllReviewsIdsAsync(string asin);
    }
}
