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
                Console.WriteLine("Response: " + response[0].Name);
                Console.WriteLine("Prompt Neighborhood: " + promptNeighborhood.Name);
                if (response[0].Equals(promptNeighborhood))
                {
                    //string responseStr = response.Select(x => x.Name).ElementAt(0);

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
            Console.WriteLine("Initial prompt: " + promptNeighborhood.Name);
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

            string eturnString = string.Join(", ", hintNeighborhoodNames);

            return "[" + returnString + "]";
        }
    }
}
