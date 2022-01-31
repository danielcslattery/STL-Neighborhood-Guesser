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

        // Return all neighborhoods as a List of GeoJSON
        public List<string> GetAll()
        {
            List<Neighborhood> neighborhoods = context.Neighborhoods.ToList();
            List<string> neighborhoodsGeoJson = neighborhoods.Select(x => x.GeoJson).ToList();

            return neighborhoodsGeoJson;
        }

        // Randomly choose the prompt neighborhood and five more to act as hints and return their names.
        // The prompt neighborhood is always first, so the frontend knows which neighborhood to use.  
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
                    hintNeighborhoods.Add(nextNeighborhood);
                }
            }

            return hintNeighborhoods;
        }

        // Given the latitude and longitude where a user clicked, check which neighborhood they clicked and if it matches the prompt
        // New users scores are handled on registration, so the logged-in user is assumed to have one here.  
        public bool ProcessGuess(double lon, double lat)
        {
            List<Neighborhood> response = context.CheckClickLocation(lon, lat);

            // Client should not make requests that return a neighborhood list not equal to 1
            if (response.Count != 1)
            {
                throw new Exception();
            }

            Console.WriteLine(response[0].Name);
            Console.WriteLine(promptNeighborhood.Name);

            string userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            IQueryable<Score> scores = context.Scores.Where(x => x.UserId == userId);

            if (scores.Any())
            {
                IncrementScore(scores, response[0].Equals(promptNeighborhood));
            }


            return response[0].Equals(promptNeighborhood);

        }

        // Increment both Points and Attempts when the answer is right; increment just attempts after wrong answer.
        public void IncrementScore(IQueryable<Score> scores, bool answer)
        {
            if (answer)
            {
                scores.First().Points += 1;
            }
            scores.First().Attempts += 1;
            context.SaveChanges();
        }

        // On page load, check if the user is logged in, return their score.  If they are not logged in return blank Score object.
        // If a user is new, create a new Score object for them with their unique Id.  
        public Score ProcessScore()
        {
            string userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
 
            if (userId != null)
            {
                IQueryable<Score> scores = context.Scores.Where(x => x.UserId == userId);
                // If the user doesn't have a score in the table yet, add them
                return !scores.Any() ? ProcessNewUserScore(userId) : scores.First();
            }

            return new Score();
        }

        public Score ProcessNewUserScore(String userId)
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


    }

}
