#region

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Conditions;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Logging;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Logic.Utils;
using PokemonGo.RocketAPI.Logic;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;

#endregion


namespace PokemonGo.RocketAPI.Logic
{
    public class InventoryZoo : Inventory
    {
        public InventoryZoo(Client client) : base(client)
        {
        }

        public async Task<IEnumerable<PokemonData>> GetPokemonToTransfer(bool keepPokemonsThatCanEvolve = false, bool prioritizeIVoverCp = false, IEnumerable<PokemonId> filter = null)
        {
            IDictionary<PokemonId, PokemonKeepCondition> conditions = _client.Settings.PokemonsToKeepCondition;
            var myPokemon = await GetPokemons();
            var pokemonList = myPokemon.Where(p => p.DeployedFortId == String.Empty && p.Favorite == 0).ToList();
            if (filter != null)
                pokemonList = pokemonList.Where(p => !filter.Contains(p.PokemonId)).ToList();
            pokemonList = pokemonList.Where(p => p.Cp < conditions[p.PokemonId].MinCP).ToList();
            pokemonList = pokemonList.Where(p => PokemonInfo.CalculatePokemonPerfection(p) < conditions[p.PokemonId].MinIVRate).ToList();

            if (!keepPokemonsThatCanEvolve)
                return pokemonList
                    .GroupBy(p => p.PokemonId)
                    .Where(x => x.Any())
                    .SelectMany(
                        p =>
                            p.OrderByDescending(
                                x => (prioritizeIVoverCp) ? PokemonInfo.CalculatePokemonPerfection(x) : x.Cp)
                                .ThenBy(n => n.StaminaMax)
                                .Skip(conditions[p.Key].MaxNumber)
                                .ToList());


            var results = new List<PokemonData>();
            var pokemonsThatCanBeTransfered = pokemonList.GroupBy(p => p.PokemonId).Where(x => x.Count() > _client.Settings.TransferPokemonKeepDuplicateAmount).ToList();

            var myPokemonSettings = await GetPokemonSettings();
            var pokemonSettings = myPokemonSettings.ToList();

            var myPokemonFamilies = await GetPokemonFamilies();
            var pokemonFamilies = myPokemonFamilies.ToArray();

            foreach (var pokemon in pokemonsThatCanBeTransfered)
            {
                var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.Key);
                var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);
                var amountToSkip = _client.Settings.TransferPokemonKeepDuplicateAmount;

                if (settings.CandyToEvolve > 0)
                {
                    var amountPossible = familyCandy.Candy_/settings.CandyToEvolve;
                    if (amountPossible > amountToSkip)
                        amountToSkip = amountPossible;
                }

                results.AddRange(pokemonList.Where(x => x.PokemonId == pokemon.Key)
                    .OrderByDescending(
                        x => (prioritizeIVoverCp) ? PokemonInfo.CalculatePokemonPerfection(x) : x.Cp)
                    .ThenBy(n => n.StaminaMax)
                    .Skip(amountToSkip)
                    .ToList());
            }

            return results;
        }

        public async Task<IEnumerable<PokemonData>> GetPokemonToEvolve(bool prioritizeIVoverCp = false, IEnumerable < PokemonId> filter = null)
        {
            var myPokemons = await GetPokemons();
            myPokemons = myPokemons.Where(p => p.DeployedFortId == string.Empty);
            if (_client.Settings.UsePokemonToEvolveList && filter != null)
                myPokemons = myPokemons.Where(p => filter.Contains(p.PokemonId));		
            if (_client.Settings.EvolveOnlyPokemonAboveIV)
                myPokemons = myPokemons.Where(p => PokemonInfo.CalculatePokemonPerfection(p) >= _client.Settings.EvolveOnlyPokemonAboveIVValue);
            myPokemons = prioritizeIVoverCp ? myPokemons.OrderByDescending(PokemonInfo.CalculatePokemonPerfection) : myPokemons.OrderByDescending(p => p.Cp);

            var pokemons = myPokemons.ToList();

            var myPokemonSettings = await GetPokemonSettings();
            var pokemonSettings = myPokemonSettings.ToList();

            var myPokemonFamilies = await GetPokemonFamilies();
            var pokemonFamilies = myPokemonFamilies.ToArray();

            var pokemonToEvolve = new List<PokemonData>();
            foreach (var pokemon in pokemons)
            {
                var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
                var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);

                //Don't evolve if we can't evolve it
                if (settings.EvolutionIds.Count == 0)
                    continue;

                var pokemonCandyNeededAlready =
                    pokemonToEvolve.Count(
                        p => pokemonSettings.Single(x => x.PokemonId == p.PokemonId).FamilyId == settings.FamilyId) *
                        settings.CandyToEvolve;

                var familiecandies = familyCandy.Candy_;
                if (_client.Settings.EvolveKeepCandiesValue > 0)
                {
                    if (familyCandy.Candy_ <= _client.Settings.EvolveKeepCandiesValue) continue;
                    familiecandies = familyCandy.Candy_ - _client.Settings.EvolveKeepCandiesValue;
                    if (familiecandies - pokemonCandyNeededAlready > settings.CandyToEvolve)
                        pokemonToEvolve.Add(pokemon);
                }
                else if (familiecandies - pokemonCandyNeededAlready > settings.CandyToEvolve)
                    pokemonToEvolve.Add(pokemon);
            }

            return pokemonToEvolve;
        }
    }
}
