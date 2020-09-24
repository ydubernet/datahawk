using amazon_scraper.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace amazon_scraper.Databases
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ReviewRepository> _logger;

        public ReviewRepository(string connectionString, ILogger<ReviewRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<Review> GetReviewAsync(int reviewId)
        {
            const string sql = @"SELECT asin, date, title, rating, content
                                 FROM Review
                                 WHERE Id = @ReviewId;";
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                var reader = await connection.ExecuteReaderAsync(sql,
                    new
                    {
                        reviewId
                    },
                    commandType: CommandType.Text).ConfigureAwait(false);

                if (!reader.IsDBNull(0))
                {
                    return new Review
                    (
                        reader.GetString(0),
                        reader.GetDateTime(1),
                        reader.GetString(2),
                        reader.GetDouble(3),
                        reader.GetString(4)
                    );
                }
                else return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Getting review for reviewId {reviewId} failed.");
                return null;
            }
        }

        public async Task<IList<Review>> GetReviewsAsync(string asin)
        {
            const string sql = @"SELECT asin, date, title, rating, content
                                 FROM Review
                                 WHERE asin = @asin;";
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                var reader = await connection.ExecuteReaderAsync(sql,
                    new
                    {
                        asin
                    },
                    commandType: CommandType.Text).ConfigureAwait(false);

                var reviews = new List<Review>();
                while (reader.Read())
                {
                    reviews.Add(
                        new Review
                        (
                            reader.GetString(0),
                            reader.GetDateTime(1),
                            reader.GetString(2),
                            reader.GetDouble(3),
                            reader.GetString(4)
                        ));
                }

                return reviews;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Getting reviews for asin {asin} failed.");
                return null;
            }
        }

        public async Task<IList<Review>> GetReviewsForManyAsinsAsync(IList<string> asins)
        {
            const string sql = @"SELECT asin, date, title, rating, content
                                 FROM Review
                                 WHERE asin in @asins;";
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                var reader = await connection.ExecuteReaderAsync(sql,
                    new
                    {
                        asins
                    },
                    commandType: CommandType.Text).ConfigureAwait(false);

                var reviews = new List<Review>();
                while (reader.Read())
                {
                    reviews.Add(
                        new Review
                        (
                            reader.GetString(0),
                            reader.GetDateTime(1),
                            reader.GetString(2),
                            reader.GetDouble(3),
                            reader.GetString(4)
                        ));
                }

                return reviews;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Getting reviews for asins {asins.Aggregate((r, s) => string.Join(",", r, s))} failed.");
                return null;
            }
        }

        public async Task<IList<Review>> GetLastReviewsForManyAsinsAsync(IList<string> asins, int numberOfReviewsByAsin)
        {
            const string sql = @"SELECT TOP(@numberOfReviews) asin, date, title, rating, content
                                 FROM Review
                                 WHERE asin in @asins
                                 ORDER BY date DESC;";
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                var reader = await connection.ExecuteReaderAsync(sql,
                    new
                    {
                        numberOfReviewsByAsin,
                        asins
                    },
                    commandType: CommandType.Text).ConfigureAwait(false);

                var reviews = new List<Review>();
                while (reader.Read())
                {
                    reviews.Add(
                        new Review
                        (
                            reader.GetString(0),
                            reader.GetDateTime(1),
                            reader.GetString(2),
                            reader.GetDouble(3),
                            reader.GetString(4)
                        ));
                }

                return reviews;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Getting last reviews for asins {asins.Aggregate((r, s) => string.Join(",", r, s))} failed.");
                return null;
            }
        }


        public async Task<bool> InsertReviewAsync(Review review)
        {
            const string sql = @"INSERT INTO Review (asin, date, title, rating, content)
                                 VALUES (@asin, @date, @title, @rating, @content);";
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                await connection.ExecuteAsync(sql,
                    new
                    {
                        review.Asin,
                        review.Date,
                        review.Title,
                        review.Rating,
                        review.Content
                    },
                    commandType: CommandType.Text).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Inserting review for asin {review.Asin} failed.");
                return false;
            }

            return true;
        }

        public async Task<bool> InsertReviewsForOneAsinAsync(IList<Review> reviews)
        {
            const string sql = @"INSERT INTO Review (asin, date, title, rating, content)
                                 VALUES (@asin, @date, @title, @rating, @content);";

            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(sql, reviews, transaction: transaction, commandType: CommandType.Text).ConfigureAwait(false);
                transaction.Commit();            
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Inserting review for asin {reviews[0].Asin} failed.");
                transaction.Rollback();
                return false;
            }
            finally
            {
                await connection.CloseAsync();
            }

            return true;
        }
    }
}
