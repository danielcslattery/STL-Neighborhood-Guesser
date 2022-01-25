using Microsoft.AspNetCore.Mvc;
using STL_Neighborhood_Guesser.Data;
using STL_Neighborhood_Guesser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace STL_Neighborhood_Guesser.Controllers
{
    [ApiController]
    [Route("Neighborhood")]
    public class NeighborhoodController : ControllerBase
    {

        // Create random number generator for prompt/hints.
        private static Random rnd = new Random();
        private static Neighborhood promptNeighborhood;
        private NeighborhoodDbContext context;

        public NeighborhoodController(NeighborhoodDbContext dbContext)
        {
            context = dbContext;
        }

        private static string ProcessGuess(List<Neighborhood> response,
                                        NeighborhoodDbContext context,
                                        string userId)
        {
            IQueryable<Score> scores = context.Scores.Where(x => x.UserId == userId);
            // If the user doesn't have a score in the table yet, add them

            if (response[0].Equals(promptNeighborhood))
            {
                IncrementScore(scores, context);

                return "[\"Correct\"]";
            }
            else
            {
                IncrementAttempts(scores, context);

                return "[\"Incorrect\"]";
            }
        }

        private static List<Neighborhood> GetHintNeighborhoods(NeighborhoodDbContext context)
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

        private static void IncrementScore(IQueryable<Score> scores,
                                         NeighborhoodDbContext context)
        {
            if (scores.Any())
            {
                scores.First().Points += 1;
                scores.First().Attempts += 1;
                context.SaveChanges();
            }
        }

        private static void IncrementAttempts(IQueryable<Score> scores,
                                         NeighborhoodDbContext context)
        {
            if (scores.Any())
            {
                scores.First().Attempts += 1;
                context.SaveChanges();
            }
        }


        [Route("all")]
        public string GetAll()
        {
            List<Neighborhood> neighborhoods = context.Neighborhoods.ToList();
            List<string> neighborhoodsGeoJson = neighborhoods.Select(x => x.GeoJson).ToList();

            return "[" + string.Join(", ", neighborhoodsGeoJson) + "]";
        }

        [Route("click")]
        public string ReceiveGuess(double lon, double lat)
        {
            // st_contains will never return more than a single row
            List<Neighborhood> response = context.CheckClickLocation(lon, lat);

            if (response.Count > 0)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

                return ProcessGuess(response, context, userId);
            }

            //Send empty json as return
            return "[]";
        }

        // Sets prompt neighborhood and returns its name.  Currently not used, but potentially useful in the future. 
        [Route("GetPrompt")]
        public string NeighborhoodToGuess()
        {
            List<Neighborhood> neighborhoods = context.Neighborhoods.ToList();
            promptNeighborhood = neighborhoods[rnd.Next(neighborhoods.Count)];
            string toGuess = promptNeighborhood.Name;

            return toGuess;
        }


        // Retrieve prompt neighborhood (the one to be guessed) and make it first in the list of hint neighborhoods.
        // The front end uses the first item in the list as the prompt neighborhood.
        // The method only returns a string, so it doesn't matter the order of neighborhoods.
        [Route("GetHints")]
        public string HintNeighborhoods()
        {
            List<Neighborhood> hintNeighborhoods = GetHintNeighborhoods(context);

            List<string> hintNeighborhoodNames = hintNeighborhoods.Select(x => x.Name).ToList();

            return "[" + string.Join(", ", hintNeighborhoodNames) + "]";
        }

        // Retrieve user's score from Score table using the userId when they log in.  New users have a blank score instantiated.
        [Route("Score")]
        public Score GetScore()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return ProcessScore(context, userId);
        }

        private static Score ProcessScore(NeighborhoodDbContext context,
                             String userId)
        {
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
}
