using System;

namespace amazon_scraper.Models
{
    public class Review
    {
        public string Asin { get; }
        public DateTime ReviewDate { get; }
        public string ReviewTitle { get; }
        public string ReviewContent { get; }
        public double Rating { get; }

        public Review(string asin, DateTime reviewDate, string reviewTitle, string reviewContent, double rating)
        {
            Asin = asin;
            ReviewDate = reviewDate;
            ReviewTitle = reviewTitle;
            ReviewContent = reviewContent;
            Rating = rating;
        }
    }
}