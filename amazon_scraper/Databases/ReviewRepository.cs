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
            const string sql = @"SELECT asin, id, date, title, rating, content
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
                        reader.GetString(1),
                        reader.GetDateTime(2),
                        reader.GetString(3),
                        reader.GetDouble(4),
                        reader.GetString(5)
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
            const string sql = @"SELECT asin, id, date, title, rating, content
                                 FROM Review
                                 WHERE asin = @asin
                                 ORDER BY date desc;";
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
                            reader.IsDBNull(0) ? null : reader.GetString(0),
                            reader.IsDBNull(1) ? null : reader.GetString(1),
                            reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                            reader.IsDBNull(3) ? null : reader.GetString(3),
                            reader.IsDBNull(4) ? -1 : reader.GetDouble(4),
                            reader.IsDBNull(5) ? null : reader.GetString(5)
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
            const string sql = @"SELECT asin, id, date, title, rating, content
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
                            reader.GetString(1),
                            reader.GetDateTime(2),
                            reader.GetString(2),
                            reader.GetDouble(4),
                            reader.GetString(5)
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
            const string sql = @"SELECT asin, id, date, title, rating, content
                                 FROM Review
                                 WHERE asin in @asins
                                 ORDER BY date DESC
                                 LIMIT @numberOfReviews;";
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
                            reader.GetString(1),
                            reader.GetDateTime(2),
                            reader.GetString(3),
                            reader.GetDouble(4),
                            reader.GetString(5)
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
            const string sql = @"INSERT INTO Review (asin, id, date, title, rating, content)
                                 VALUES (@asin, @reviewid, date, @title, @rating, @content);";
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
            const string sql = @"INSERT INTO Review (asin, id, date, title, rating, content)
                                 VALUES (@asin, @reviewid, @date, @title, @rating, @content);";

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

        public async Task<IList<string>> GetAllReviewsIdsAsync(string asin)
        {
            const string sql = @"SELECT id
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

                var reviewsIds = new List<string>();
                while (reader.Read())
                {
                    reviewsIds.Add(
                       reader.GetString(0));
                }
                return reviewsIds;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Getting all review ids for asin {asin} failed.");
                return null;
            }
        }
    }
}
