#region

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Conditions;
using PokemonGo.RocketAPI.Logic.Utils;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Logging;
using System.Diagnostics;
using Google.Protobuf;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

#endregion


namespace PokemonGo.RocketAPI.Logic
{
    public class LogicZoo : Logic
    {
        private AuthCondition UserAuthCondition;
        private ICollection<PokemonId> PokemonsNotToTransfer;
        private ICollection<PokemonId> PokemonsToEvolve;
        private ICollection<PokemonId> PokemonsNotToCatch;
        private IDictionary<PokemonId, PokemonKeepCondition> PokemonsToKeep;
        private IEnumerable<LocationCondition> LocationsCondition;
        private ICollection<PokemonId> SnipePokemons;
        private IEnumerable<Location> SnipeLocations;

        public LogicZoo(ISettings clientSettings) : base(clientSettings)
        {
            _inventory = new InventoryZoo(_client);
        }

        public async Task Execute()
        {
            // load config files
            Logger.Write($"Load config files ...", LogLevel.Info);
            UserAuthCondition = _clientSettings.UserAuthCondition;
            PokemonsNotToTransfer = _clientSettings.PokemonsToNotTransfer;
            PokemonsNotToCatch = _clientSettings.PokemonsToNotCatch;
            PokemonsToEvolve = _clientSettings.PokemonsToEvolve;
            PokemonsToKeep = _clientSettings.PokemonsToKeepCondition;
            LocationsCondition = _clientSettings.LocationsCondition;
            SnipePokemons = _clientSettings.SnipePokemons;
            SnipeLocations = _clientSettings.SnipeLocations;

            if (LocationsCondition.First() == null)
            {
                Logger.Write($"Please change first Latitude and/or Longitude because currently your using default values!", LogLevel.Error);
                Logger.Write($"Window will be auto closed in 15 seconds!", LogLevel.Error);
                await Task.Delay(15000);
                System.Environment.Exit(1);
            }
            else
            {
                Logger.Write($"Make sure Lat & Lng is right. Exit Program if not! Lat: {_client.CurrentLatitude} Lng: {_client.CurrentLongitude}", LogLevel.Warning);
                for (int i = 3; i > 0; i--)
                {
                    Logger.Write($"Script will continue in {i * 5} seconds!", LogLevel.Warning);
                    await Task.Delay(5000);
                }
            }

            Logger.Write($"Logging in via: {_clientSettings.AuthType}", LogLevel.Info);
            while (true)
            {
                try
                {
                    switch (_clientSettings.AuthType)
                    {
                        case AuthType.Ptc:
                            await _client.Login.DoLogin();
                            break;
                        case AuthType.Google:
                            await _client.Login.DoLogin();
                            break;
                        default:
                            Logger.Write("wrong AuthType");
                            Environment.Exit(0);
                            break;
                    }


                    await PostLoginExecute();
                }
                catch (AccountNotVerifiedException)
                {
                    Logger.Write("Account not verified! Exiting...", LogLevel.Error);
                    await Task.Delay(5000);
                    Environment.Exit(0);
                }
                catch (GoogleException e)
                {
                    if (e.Message.Contains("NeedsBrowser"))
                    {
                        Logger.Write("As you have Google Two Factor Auth enabled, you will need to insert an App Specific Password into the UserSettings.", LogLevel.Error);
                        Logger.Write("Opening Google App-Passwords. Please make a new App Password (use Other as Device)", LogLevel.Error);
                        await Task.Delay(7000);
                        try
                        {
                            Process.Start("https://security.google.com/settings/security/apppasswords");
                        }
                        catch (Exception)
                        {
                            Logger.Write("https://security.google.com/settings/security/apppasswords");
                            throw;
                        }
                    }
                    Logger.Write("Make sure you have entered the right Email & Password.", LogLevel.Error);
                    await Task.Delay(5000);
                    Environment.Exit(0);
                }
                catch (InvalidProtocolBufferException ex) when (ex.Message.Contains("SkipLastField"))
                {
                    Logger.Write("Connection refused. Your IP might have been Blacklisted by Niantic. Exiting..", LogLevel.Error);
                    await Task.Delay(5000);
                    Environment.Exit(0);
                }
                catch (Exception e)
                {
                    Logger.Write(e.Message + " from " + e.Source);
                    Logger.Write("Error, trying automatic restart..", LogLevel.Error);
                    await Execute();
                }
                await Task.Delay(10000);
            }
        }

        public async Task PostLoginExecute()
        {
            Logger.Write($"Client logged in", LogLevel.Info);

            while (true)
            {
                if (!_isInitialized)
                {
                    await Inventory.GetCachedInventory(_client);
                    _playerProfile = await _client.Player.GetPlayer();
                    var playerName = BotStats.GetUsername(_client, _playerProfile);
                    _stats.UpdateConsoleTitle(_client, _inventory);
                    var currentLevelInfos = await BotStats._getcurrentLevelInfos(_inventory);

                    var stats = await _inventory.GetPlayerStats();
                    var stat = stats.FirstOrDefault();
                    if (stat != null) BotStats.KmWalkedOnStart = stat.KmWalked;

                    Logger.Write("----------------------------", LogLevel.None, ConsoleColor.Yellow);
                    if (_clientSettings.AuthType == AuthType.Ptc)
                        Logger.Write($"PTC Account: {playerName}\n", LogLevel.None, ConsoleColor.Cyan);
                    Logger.Write($"Latitude: {_clientSettings.DefaultLatitude}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write($"Longitude: {_clientSettings.DefaultLongitude}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write("----------------------------", LogLevel.None, ConsoleColor.Yellow);
                    Logger.Write("Your Account:\n");
                    Logger.Write($"Name: {playerName}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write($"Team: {_playerProfile.PlayerData.Team}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write($"Level: {currentLevelInfos}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write($"Stardust: {_playerProfile.PlayerData.Currencies.ToArray()[1].Amount}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write("----------------------------", LogLevel.None, ConsoleColor.Yellow);
                    await DisplayHighests();
                    Logger.Write("----------------------------", LogLevel.None, ConsoleColor.Yellow);

                    var pokemonsToNotTransfer = _clientSettings.PokemonsToNotTransfer;
                    var pokemonsToNotCatch = _clientSettings.PokemonsToNotCatch;
                    var pokemonsToEvolve = _clientSettings.PokemonsToEvolve;

                    if (_clientSettings.UseLuckyEggs) await UseLuckyEgg();
                    if (_clientSettings.EvolvePokemon || _clientSettings.EvolveOnlyPokemonAboveIV) await EvolvePokemon();
                    await TransferPokemon();
                    await _inventory.ExportPokemonToCsv(_playerProfile.PlayerData);
                    await RecycleItems();
                    _isInitialized = true;
                }

                foreach (var location in LocationsCondition)
                {
                    Logger.Write("----------------------------", LogLevel.None, ConsoleColor.Yellow);
                    Logger.Write($"Latitude: {location.Latitude}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write($"Longitude: {location.Longitude}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write($"OperationType: {location.OperationType}", LogLevel.None, ConsoleColor.DarkGray);
                    Logger.Write("----------------------------", LogLevel.None, ConsoleColor.Yellow);

                    if (_clientSettings.EvolvePokemon || _clientSettings.EvolveOnlyPokemonAboveIV) await EvolvePokemon();
                    await TransferPokemon();
                    await _inventory.ExportPokemonToCsv(_playerProfile.PlayerData);
                    await RecycleItems();
                    await ExecuteFarmingPokestopsAndPokemons(location);
                    await RefreshTokens();
                }
                await Task.Delay(10000);
            }
        }

        protected async Task ExecuteFarmingPokestopsAndPokemons(LocationCondition location)
        {
            // set location
            _client.Settings.setDefaultLocation(location); // for GetPokestops

            var distanceFromStart = LocationUtils.CalculateDistanceInMeters(
                location.Latitude, location.Longitude,
                _client.CurrentLatitude, _client.CurrentLongitude);

            // Edge case for when the client somehow ends up outside the defined radius
            if (location.Radius != 0 && distanceFromStart > location.Radius)
            {
                Logger.Write(
                    $"You're outside of your defined radius! Walking to start ({distanceFromStart:0.##}m away) in 5 seconds. Is your LastCoords.ini file correct?",
                    LogLevel.Warning);
                await Task.Delay(5000);
                Logger.Write("Moving to start location now.");
                await _navigation.HumanLikeWalking(
                    new GeoUtils(location.Latitude, location.Longitude),
                    location.WalkingSpeedInKmPerHour, ExecuteCatchAllNearbyPokemons);
            }

            var pokeStops = await _inventory.GetPokestops();
            var pokestopList = pokeStops.ToList();
            if (pokestopList.Count <= 0)
                Logger.Write("No usable PokeStops found in your area. Is your maximum distance too small?",
                    LogLevel.Warning);
            else
                Logger.Write($"Found {pokeStops.Count()} {(pokeStops.Count() == 1 ? "Pokestop" : "Pokestops")}", LogLevel.Info);

            while (pokestopList.Any())
            {
                if (_clientSettings.UseLuckyEggs)
                    await UseLuckyEgg();
                if (_clientSettings.UseIncense)
                    await UseIncense();

                await ExecuteCatchAllNearbyPokemons();

                pokestopList =
                    pokestopList.OrderBy(
                        i =>
                            LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                                _client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                var pokeStop = pokestopList.First();
                pokestopList.Remove(pokeStop);

                var distance = LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await _client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                var latlngDebug = $"| Latitude: {pokeStop.Latitude} - Longitude: {pokeStop.Longitude}";

                Logger.Write($"Name: {fortInfo.Name} in {distance:0.##} m distance {latlngDebug}", LogLevel.Pokestop);

                if (location.OperationType == 0)
                {
                    await
                        _client.Player.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude,
                            _clientSettings.DefaultAltitude);
                    Logger.Write($"Using Teleport instead of Walking!", LogLevel.Warning);
                }
                else
                {
                    await
                        _navigation.HumanLikeWalking(new GeoUtils(pokeStop.Latitude, pokeStop.Longitude),
                        location.WalkingSpeedInKmPerHour, ExecuteCatchAllNearbyPokemons);
                }

                var timesZeroXPawarded = 0;
                var fortTry = 0;      //Current check
                const int retryNumber = 45; //How many times it needs to check to clear softban
                const int zeroCheck = 5; //How many times it checks fort before it thinks it's softban
                do
                {
                    var fortSearch = await _client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                    if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                    if (fortSearch.ExperienceAwarded == 0)
                    {
                        timesZeroXPawarded++;

                        if (timesZeroXPawarded <= zeroCheck) continue;
                        if ((int)fortSearch.CooldownCompleteTimestampMs != 0)
                        {
                            break; // Check if successfully looted, if so program can continue as this was "false alarm".
                        }
                        fortTry += 1;

                        Logger.Write($"Seems your Soft-Banned. Trying to Unban via Pokestop Spins. Retry {fortTry} of {retryNumber - zeroCheck}", LogLevel.Warning);

                        await RandomHelper.RandomDelay(75, 100);
                    }
                    else
                    {
                        _stats.AddExperience(fortSearch.ExperienceAwarded);
                        _stats.UpdateConsoleTitle(_client, _inventory);
                        var eggReward = fortSearch.PokemonDataEgg != null ? "1" : "0";
                        Logger.Write($"XP: {fortSearch.ExperienceAwarded}, Gems: {fortSearch.GemsAwarded}, Eggs: {eggReward}, Items: {StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded)}", LogLevel.Pokestop);
                        _recycleCounter++;
                        break; //Continue with program as loot was succesfull.
                    }
                } while (fortTry < retryNumber - zeroCheck); //Stop trying if softban is cleaned earlier or if 40 times fort looting failed.

                if (_recycleCounter >= 5)
                    await RecycleItems();
            }
        }

        protected async Task ExecuteCatchAllNearbyPokemons()
        {
            var mapObjects = await _client.Map.GetMapObjects();

            var pokemons =
                mapObjects.Item1.MapCells.SelectMany(i => i.CatchablePokemons)
                .OrderBy(
                    i =>
                    LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();

            // not catch filter
            ICollection<PokemonId> filter = _clientSettings.PokemonsToNotCatch;
            pokemons = pokemons.Where(p => !filter.Contains(p.PokemonId)).ToList();

            if (pokemons.Any())
                Logger.Write($"Found {pokemons.Count()} catchable Pokemon", LogLevel.Info);
            else
            {
                Logger.Write($"No catchable Pokemon", LogLevel.Info);
                return;
            }

            foreach (var pokemon in pokemons)
            {
                var encounter = await _client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                    await CatchEncounter(encounter, pokemon);
                else
                    Logger.Write($"Encounter problem: {encounter.Status}", LogLevel.Warning);
            }

            //await EvolvePokemon();
            await TransferPokemon();
        }

        protected async Task TransferPokemon()
        {
            await Inventory.GetCachedInventory(_client, true);
            var pokemonToTransfer = await _inventory.GetPokemonToTransfer(false, false, _clientSettings.PokemonsToNotTransfer);
            if (pokemonToTransfer != null && pokemonToTransfer.Any())
                Logger.Write($"Found {pokemonToTransfer.Count()} Pokemon for Transfer:", LogLevel.Info);

            foreach (var pokemon in pokemonToTransfer)
            {
                await _client.Inventory.TransferPokemon(pokemon.Id);

                await Inventory.GetCachedInventory(_client, true);
                var myPokemonSettings = await _inventory.GetPokemonSettings();
                var pokemonSettings = myPokemonSettings.ToList();
                var myPokemonFamilies = await _inventory.GetPokemonFamilies();
                var pokemonFamilies = myPokemonFamilies.ToArray();
                var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
                var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);
                var familyCandies = $"{familyCandy.Candy_}";

                _stats.IncreasePokemonsTransfered();
                _stats.UpdateConsoleTitle(_client, _inventory);

                var bestPokemonOfType = await _inventory.GetHighestPokemonOfTypeByCp(pokemon);
                var bestPokemonInfo = "NONE";
                if (bestPokemonOfType != null)
                    bestPokemonInfo = $"CP: {bestPokemonOfType.Cp}/{PokemonInfo.CalculateMaxCp(bestPokemonOfType)} | IV: {PokemonInfo.CalculatePokemonPerfection(bestPokemonOfType).ToString("0.00")}% perfect";

                Logger.Write($"{pokemon.PokemonId} [CP {pokemon.Cp}/{PokemonInfo.CalculateMaxCp(pokemon)} | IV: { PokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00")}% perfect] | Best: [{bestPokemonInfo}] | Family Candies: {familyCandies}", LogLevel.Transfer);
            }
        }
    }
}
 