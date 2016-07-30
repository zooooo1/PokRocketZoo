using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace PokemonGo.RocketAPI
{
    public class PokemonKeepCondition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinNumber { get; set; }
        public int MaxNumber { get; set; }
        public int MinLv { get; set; }
        public int MinCP { get; set; }
        public int MinIVRate { get; set; }
    }

    public sealed class PokemonKeepConditionMap : CsvClassMap<PokemonKeepCondition>
    {
        public PokemonKeepConditionMap()
        {
            Map(m => m.Id).Index(0);
            Map(m => m.Name).Index(1);
            Map(m => m.MinNumber).Index(2);
            Map(m => m.MaxNumber).Index(3);
            Map(m => m.MinLv).Index(4);
            Map(m => m.MinCP).Index(5);
            Map(m => m.MinIVRate).Index(6);
        }
    }
}
