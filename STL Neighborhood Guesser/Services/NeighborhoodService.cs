using STL_Neighborhood_Guesser.Data;
using STL_Neighborhood_Guesser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace STL_Neighborhood_Guesser.Services
{
    public class NeighborhoodService : INeighborhoodService
    {
        private NeighborhoodDbContext context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static Random rnd = new Random();
        private static Neighborhood promptNeighborhood;

        public NeighborhoodService(NeighborhoodDbContext dbContext,
                                   IHttpContextAccessor httpContextAccessor)
        {
            context = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public List<string> GetAll()
        {
            List<Neighborhood> neighborhoods = context.Neighborhoods.ToList();
            List<string> neighborhoodsGeoJson = neighborhoods.Select(x => x.GeoJson).ToList();

            return neighborhoodsGeoJson;
        }

        public List<Neighborhood> GetHintNeighborhoods()
        {
            List<Neighborhood> neighborhoods = context.Neighborhoods.ToList();

            promptNeighborhood = neighborhoods[rnd.Next(neighborhoods.Count)];
            List<Neighborhood> hintNeighborhoods = new List<Neighborhood>()
            {
                promptNeighborhood
            };


            while (hintNeighborhoods.Count < 6)
            {
                // Get random neighborhood and check that its not yet in the list
                Neighborhood nextNeighborhood = neighborhoods[rnd.Next(neighborhoods.Count)];
                if (!hintNeighborhoods.Contains(nextNeighborhood))
                {
                    hintNeighborhoods.Add(neighborhoods[rnd.Next(neighborhoods.Count)]);
                }
                else
                {
                    //Do nothing
                }
            }

            return hintNeighborhoods;
        }


        public string ProcessGuess(double lon, double lat)
        {
            List<Neighborhood> response = context.CheckClickLocation(lon, lat);

            if (response.Count > 0)
            {
                string userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                IQueryable<Score> scores = context.Scores.Where(x => x.UserId == userId);
                // If the user doesn't have a score in the table yet, add them

                if (response[0].Equals(promptNeighborhood))
                {
                    IncrementScore(scores);

                    return "[\"Correct\"]";
                }
                else
                {
                    IncrementAttempts(scores);

                    return "[\"Incorrect\"]";
                }
            }
            //Send empty json as return
            return "[]";
        }

        public void IncrementScore(IQueryable<Score> scores)
        {
            if (scores.Any())
            {
                scores.First().Points += 1;
                scores.First().Attempts += 1;
                context.SaveChanges();
            }
        }

        public void IncrementAttempts(IQueryable<Score> scores)
        {
            if (scores.Any())
            {
                scores.First().Attempts += 1;
                context.SaveChanges();
            }
        }


        public Score ProcessScore()
        {
            string userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                IQueryable<Score> scores = context.Scores.Where(x => x.UserId == userId);
                // If the user doesn't have a score in the table yet, add them
                if (!scores.Any())
                {

                    Score newUserScore = new Score()
                    {
                        UserId = userId,
                        Points = 0,
                        Attempts = 0
                    };
                    context.Scores.Add(newUserScore);
                    context.SaveChanges();

                    return newUserScore;
                }

                Score userScore = context.Scores.Where(x => x.UserId == userId).First();

                return userScore;
            }

            return new Score();
        }


    }

    public interface INeighborhoodService
    {
        public List<string> GetAll();

        public List<Neighborhood> GetHintNeighborhoods();

        public string ProcessGuess(double lon, double lat);

        public void IncrementScore(IQueryable<Score> scores);

        public void IncrementAttempts(IQueryable<Score> scores);

        public Score ProcessScore();
    }
}
