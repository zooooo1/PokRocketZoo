using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace PokemonGo.RocketAPI
{
    public class Location
    {
        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public sealed class LocationMap : CsvClassMap<Location>
    {
        public LocationMap()
        {
            Map(m => m.Latitude).Index(0);
            Map(m => m.Longitude).Index(1);
        }
    }

}
