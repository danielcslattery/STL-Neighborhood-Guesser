using STL_Neighborhood_Guesser.Models;
using System.Collections.Generic;
using System.Linq;

namespace STL_Neighborhood_Guesser.Services
{

    public interface INeighborhoodService
    {
        public List<string> GetAll();

        public List<Neighborhood> GetHintNeighborhoods();

        public bool ProcessGuess(double lon, double lat);

        public void IncrementScore(IQueryable<Score> scores, bool answer);

        public Score ProcessScore();

        public Score ProcessNewUserScore(string userId);
    }
}
