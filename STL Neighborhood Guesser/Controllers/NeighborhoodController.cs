using Microsoft.AspNetCore.Mvc;
using STL_Neighborhood_Guesser.Data;
using STL_Neighborhood_Guesser.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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

        public NeighborhoodController(  NeighborhoodDbContext dbContext)
        {
            context = dbContext;
        }

        [EnableCors]
        [Route("all")]
        public string GetAll()
        {
            List<Neighborhood> neighborhoods = context.Neighborhoods.ToList();
            List<string> neighborhoodsGeoJson = neighborhoods.Select(x => x.GeoJson).ToList();

            string returnString = string.Join(", ", neighborhoodsGeoJson);

            return "[" + returnString + "]";
        }

        [Route("click")]
        public string Guess(double lon, double lat)
        {
            // st_contains will never return more than a single row
            List<Neighborhood> response = context.Neighborhoods
                .FromSqlRaw("SELECT * " +
                "FROM neighborhoods " +
                "WHERE st_contains(geodata, ST_SRID(Point ({0}, {1}), 4326));", lon, lat)
                .ToList();
            if (response.Count > 0)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId

                IQueryable<Score> scores = context.Scores.Where(x => x.UserId == userId);
                // If the user doesn't have a score in the table yet, add them

                if (response[0].Equals(promptNeighborhood))
                {
                    if (scores.Any())
                    {
                        scores.First().Points += 1;
                        scores.First().Attempts += 1;
                        context.SaveChanges();
                    }

                    return "[\"Correct\"]";
                } else
                {
                    if (scores.Any())
                    {
                        scores.First().Attempts += 1;
                        context.SaveChanges();
                    }

                    return "[\"Incorrect\"]";
                }

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
                } else
                {
                    //Do nothing
                }   
            }

            List<string> hintNeighborhoodNames = hintNeighborhoods.Select(x => x.Name).ToList();

            string returnString = string.Join(", ", hintNeighborhoodNames);

            return "[" + returnString + "]";
        }

        // Retrieve user's score from Score table using the userId when they log in.  New users have a blank score instantiated.
        [Route("Score")]
        public Score GetScore()
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                IQueryable<Score> scores = context.Scores.Where(x => x.UserId == userId);
                // If the user doesn't have a score in the table yet, add them
                if (! scores.Any())
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
