using Microsoft.AspNetCore.Mvc;
using STL_Neighborhood_Guesser.Data;
using STL_Neighborhood_Guesser.Models;
using STL_Neighborhood_Guesser.Services;
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
        private INeighborhoodService _neighborhoodService;

        public NeighborhoodController(INeighborhoodService neighborhoodService)
        {
            _neighborhoodService = neighborhoodService;
        }

        [Route("all")]
        public string GetAll()
        {
            List<string> neighborhoodsGeoJson = _neighborhoodService.GetAll();
            return "[" + string.Join(", ", neighborhoodsGeoJson) + "]";
        }

        [Route("click")]
        public string ReceiveGuess(double lon, double lat)
        {
            return _neighborhoodService.ProcessGuess(lon, lat);
        }


        // Retrieve prompt neighborhood (the one to be guessed) and make it first in the list of hint neighborhoods.
        // The front end uses the first item in the list as the prompt neighborhood.
        // The method only returns a string, so it doesn't matter the order of neighborhoods.
        [Route("GetHints")]
        public string HintNeighborhoods()
        {
            List<Neighborhood> hintNeighborhoods = _neighborhoodService.GetHintNeighborhoods();

            List<string> hintNeighborhoodNames = hintNeighborhoods.Select(x => x.Name).ToList();

            return "[" + string.Join(", ", hintNeighborhoodNames) + "]";
        }

        // Retrieve user's score from Score table using the userId when they log in.  New users have a blank score instantiated.
        [Route("Score")]
        public Score GetScore()
        {
            return _neighborhoodService.ProcessScore();
        }


    }
}
