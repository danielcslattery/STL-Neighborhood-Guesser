using STL_Neighborhood_Guesser.Models;
using System.Collections.Generic;
using System.Linq;

namespace STL_Neighborhood_Guesser.Services
{

    public interface INeighborhoodService
    {
        public List<string> GetAll();

        public List<Neighborhood> GetHintNeighborhoods();

        public string ProcessGuess(double lon, double lat);

        public void IncrementScore(IQueryable<Score> scores);

        public void IncrementAttempts(IQueryable<Score> scores);

        public Score ProcessScore();
    }
