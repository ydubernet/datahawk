using amazon_scraper.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace amazon_scraper.Services
{
    public interface IScrapingService
    {
        Task<List<Review>> GetPageData(string asin, List<Review> results);
    }
}