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
        public bool ReceiveGuess(double lon, double lat)
        {
            return _neighborhoodService.ProcessGuess(lon, lat);
        }


        [Route("GetHints")]
        public string HintNeighborhoods()
        {
            List<Neighborhood> hintNeighborhoods = _neighborhoodService.GetHintNeighborhoods();

            List<string> hintNeighborhoodNames = hintNeighborhoods.Select(x => x.Name).ToList();

            return "[" + string.Join(", ", hintNeighborhoodNames) + "]";
        }

        [Route("Score")]
        public Score GetScore()
        {
            return _neighborhoodService.ProcessScore();
        }
    }
}
