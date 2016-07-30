#region

using PokemonGo.RocketAPI.Enums;
using System.Collections.Generic;
using PokemonGo.RocketAPI.GeneratedCode;

#endregion


namespace PokemonGo.RocketAPI
{
    public interface ISettings
    {
        AuthType AuthType { get; }
        string GoogleRefreshToken { get; set; }
        string PtcPassword { get; }
        string PtcUsername { get; }
        bool EvolveAllPokemonWithEnoughCandy { get; }
        string GooglePassword { get; }
        string GoogleUsername { get; }

        ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter { get; }

        ICollection<PokemonId> PokemonsToEvolve { get; }

        ICollection<PokemonId> PokemonsNotToTransfer { get; }

        ICollection<PokemonId> PokemonsNotToCatch { get; }

        IDictionary<PokemonId, PokemonKeepCondition> PokemonsToKeepCondition { get; }

        IEnumerable<LocationCondition> LocationsCondition { get; }
    }
}