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
        private readonly UserManager<IdentityUser> _userManager;
        private NeighborhoodDbContext context;

        public NeighborhoodController(  NeighborhoodDbContext dbContext,
                                        UserManager<IdentityUser> userManager)
        {
            context = dbContext;
            _userManager = userManager;
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
            List<Neighborhood> response = context.Neighborhoods
                .FromSqlRaw("SELECT * " +
                "FROM neighborhoods " +
                "WHERE st_contains(geodata, ST_SRID(Point ({0}, {1}), 4326));", lon, lat)
                .ToList();
            if (response.Count > 0)
            {
                if (response[0].Equals(promptNeighborhood))
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId

                    Score userScore = context.Scores.Where(x => x.UserId == userId).First();
                    userScore.Points += 1;
                    context.SaveChanges();

                    return "[\"Correct\"]";
                } else
                {
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


        [Route("Score")]
        public int GetScore()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId

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

                    return newUserScore.Points;
                }

                Score userScore = context.Scores.Where(x => x.UserId == userId).First();

                return userScore.Points;
            }

            return 0;

        }

    }
}
