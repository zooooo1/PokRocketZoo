using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace PokemonGo.RocketAPI
{
    public class LocationCondition
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double WalkingSpeedInKmPerHour { get; set; }
        public int StayTime { get; set; }
        public int OperationType { get; set; }
        public int MovePokeStopMax { get; set; }
    }

    public sealed class LocationConditionMap : CsvClassMap<LocationCondition>
    {
        public LocationConditionMap()
        {
            Map(m => m.Latitude).Index(0);
            Map(m => m.Longitude).Index(1);
            Map(m => m.Altitude).Index(2);
            Map(m => m.WalkingSpeedInKmPerHour).Index(3);
            Map(m => m.StayTime).Index(4);
            Map(m => m.OperationType).Index(5);
            Map(m => m.MovePokeStopMax).Index(6);
        }
    }
}
