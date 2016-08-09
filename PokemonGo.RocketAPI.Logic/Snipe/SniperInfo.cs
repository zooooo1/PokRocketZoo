using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Enums;

namespace PokemonGo.RocketAPI.Logic.Snipe
{
    public class SniperInfo
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double Iv { get; set; }
        public DateTime TimeStamp { get; set; }
        public PokemonId Id { get; set; }

        public DateTime TimeStampAdded { get; set; } = DateTime.Now;
    }
}
