#region

using System.Collections.Generic;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Conditions;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;

#endregion


namespace PokemonGo.RocketAPI
{
    public interface ISettings
    {
        AuthType AuthType { get; }
        string PtcPassword { get; }
        string PtcUsername { get; }
        string GoogleEmail { get; }
        string GooglePassword { get; }
        double DefaultLatitude { get; }
        double DefaultLongitude { get; }
        double DefaultAltitude { get; }

        bool UseLuckyEggs { get; }
        bool UseIncense { get; }
        bool DebugMode { get; }
        bool UseTeleportInsteadOfWalking { get; }

        string GoogleRefreshToken { get; set; }
        bool EvolveAllPokemonWithEnoughCandy { get; }

        ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter { get; }

        ICollection<PokemonId> PokemonsToEvolve { get; }

        ICollection<PokemonId> PokemonsNotToTransfer { get; }

        ICollection<PokemonId> PokemonsNotToCatch { get; }

        IDictionary<PokemonId, PokemonKeepCondition> PokemonsToKeepCondition { get; }

        IEnumerable<LocationCondition> LocationsCondition { get; }

        IEnumerable<Location> SnipeLocations { get; }

        ICollection<PokemonId> SnipePokemons { get; }

        AuthCondition UserAuthCondition { get; }
    }
}