using System.Collections.Generic;
using System.Threading.Tasks;

namespace amazon_scraper.Services
{
    public interface IScrapingService
    {
        Task<List<string>> GetPageData(string url, List<string> results);
    }
}