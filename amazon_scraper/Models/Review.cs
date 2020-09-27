using System;

namespace amazon_scraper.Models
{
    public class Review
    {
        public string Asin { get; }
        public string ReviewId { get; }
        public DateTime Date { get; }
        public string Title { get; }
        public double Rating { get; }
        public string Content { get; }

        public Review(string asin, string reviewId, DateTime date, string title, double rating, string content)
        {
            Asin = asin;
            ReviewId = reviewId;
            Date = date;
            Title = title;
            Rating = rating;
            Content = content;
        }
    }
}