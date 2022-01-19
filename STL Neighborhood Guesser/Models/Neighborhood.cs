

using STL_Neighborhood_Guesser.Controllers;
using System;
using System.Collections.Generic;

namespace STL_Neighborhood_Guesser.Models
{
    public class Neighborhood
    {
        public int Id { get; set; }
        public string GeoJson { get; set; }
        public string Name { get; set; }

        public byte[] GeoData { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Neighborhood neighborhood &&
                   Name == neighborhood.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, GeoJson, Name, GeoData);
        }



    }
}
