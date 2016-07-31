#region

using PokemonGo.RocketAPI.Enums;
using System.Collections.Generic;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Conditions;

#endregion


namespace PokemonGo.RocketAPI
{
    public interface ISettings
    {
        string GoogleRefreshToken { get; set; }
        bool EvolveAllPokemonWithEnoughCandy { get; }

        ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter { get; }

        ICollection<PokemonId> PokemonsToEvolve { get; }

        ICollection<PokemonId> PokemonsNotToTransfer { get; }

        ICollection<PokemonId> PokemonsNotToCatch { get; }

        IDictionary<PokemonId, PokemonKeepCondition> PokemonsToKeepCondition { get; }

        IEnumerable<LocationCondition> LocationsCondition { get; }

        AuthCondition UserAuthCondition { get; }
    }
}