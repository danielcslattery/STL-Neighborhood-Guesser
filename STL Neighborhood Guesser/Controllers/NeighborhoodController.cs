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

        // Taken from https://stackoverflow.com/questions/273313/randomize-a-listt



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

            /*            string returnString = "{" +
                                                "\"type\": \"FeatureCollection\", " +
                                                " \"features\": [";*/

            string returnString = "[";

            returnString += string.Join(", ", neighborhoodsGeoJson);

            /*            returnString += "] }";*/

            returnString += "]";

            return returnString;
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

        // Sets prompt neighborhood and returns its name.
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
            List<Neighborhood> hintNeighborhoods = new List<Neighborhood>();


            while (hintNeighborhoods.Count < 5)
            {
                // Get random neighborhood and check that its not the promptNeighborhood
                Neighborhood nextNeighborhood = neighborhoods[rnd.Next(neighborhoods.Count)];
                if (!hintNeighborhoods.Contains(nextNeighborhood) && !nextNeighborhood.Equals(promptNeighborhood))
                {
                    hintNeighborhoods.Add(neighborhoods[rnd.Next(neighborhoods.Count)]);
                } else
                {
                    //Do nothing
                }   
            }

            hintNeighborhoods.Insert(rnd.Next(5),promptNeighborhood);


            List<string> hintNeighborhoodNames = hintNeighborhoods.Select(x => x.Name).ToList();

            string returnString = "[";

            returnString += string.Join(", ", hintNeighborhoodNames);

            /*            returnString += "] }";*/

            returnString += "]";

            return returnString;
        }
    }
}
